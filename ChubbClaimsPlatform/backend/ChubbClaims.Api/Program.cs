using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChubbClaims.Api.Data;
using ChubbClaims.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5080");
var connection = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=chubb-claims.db";
builder.Services.AddDbContext<ClaimsDbContext>(o => o.UseSqlite(connection));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o => o.AddPolicy("frontend", p => p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

var jwtKey = builder.Configuration["Jwt:Key"] ?? "LOCAL-DEMO-ONLY-CHANGE-THIS-KEY-TO-A-32-CHARACTER-SECRET";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false, ValidateAudience = false, ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey, ClockSkew = TimeSpan.FromMinutes(1)
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ClaimantOnly", p => p.RequireRole("Claimant"));
    options.AddPolicy("OfficerOnly", p => p.RequireRole("ClaimsOfficer"));
});

var app = builder.Build();
app.UseCors("frontend");
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
    await db.Database.EnsureCreatedAsync();
    await SeedData.EnsureSeeded(db);
}

app.MapGet("/", () => Results.Ok(new { name = "Chubb Claims API", swagger = "/swagger" }));

app.MapPost("/api/auth/login", async (LoginRequest request, ClaimsDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(x => x.Email == request.Email.ToLower());
    if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        return Results.Unauthorized();

    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.Name), new Claim(ClaimTypes.Role, user.Role) };
    var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddHours(4),
        signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
    return Results.Ok(new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), user.Name, user.Role));
});

var claimsApi = app.MapGroup("/api/claims").RequireAuthorization();

claimsApi.MapPost("/", async (CreateClaimRequest req, ClaimsDbContext db, ClaimsPrincipal principal) =>
{
    if (string.IsNullOrWhiteSpace(req.PolicyNumber) || string.IsNullOrWhiteSpace(req.Description) || req.EstimatedAmount <= 0)
        return Results.BadRequest(new { message = "Policy number, description and a positive estimated amount are required." });
    if (req.IncidentDate.Date > DateTime.UtcNow.Date) return Results.BadRequest(new { message = "Incident date cannot be in the future." });

    var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var claim = new ClaimRecord
    {
        ClaimNumber = $"CHB-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
        ClaimantId = userId, ClaimType = req.ClaimType, PolicyNumber = req.PolicyNumber.Trim(),
        IncidentDate = req.IncidentDate, IncidentLocation = req.IncidentLocation.Trim(),
        Description = req.Description.Trim(), EstimatedAmount = req.EstimatedAmount,
        Market = string.IsNullOrWhiteSpace(req.Market) ? "SG" : req.Market.ToUpper(),
        Currency = string.IsNullOrWhiteSpace(req.Currency) ? "SGD" : req.Currency.ToUpper(),
        Status = ClaimStatus.Submitted, CreatedAt = DateTime.UtcNow
    };
    db.Claims.Add(claim);
    db.StatusHistory.Add(new ClaimStatusHistory { ClaimId = claim.Id, Status = claim.Status, ChangedBy = userId, ChangedAt = DateTime.UtcNow, Comment = "Claim submitted" });
    await db.SaveChangesAsync();
    return Results.Created($"/api/claims/{claim.Id}", ToDto(claim, false));
}).RequireAuthorization("ClaimantOnly");

claimsApi.MapGet("/mine", async (ClaimsDbContext db, ClaimsPrincipal p) =>
{
    var id = Guid.Parse(p.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var items = await db.Claims.Where(c => c.ClaimantId == id).OrderByDescending(c => c.CreatedAt).ToListAsync();
    return Results.Ok(items.Select(c => ToDto(c, false)));
}).RequireAuthorization("ClaimantOnly");

claimsApi.MapGet("/", async (ClaimsDbContext db, string? status, string? search) =>
{
    var q = db.Claims.AsQueryable();
    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ClaimStatus>(status, true, out var parsed)) q = q.Where(c => c.Status == parsed);
    if (!string.IsNullOrWhiteSpace(search)) q = q.Where(c => c.ClaimNumber.Contains(search) || c.PolicyNumber.Contains(search));
    var items = await q.OrderByDescending(c => c.CreatedAt).ToListAsync();
    return Results.Ok(items.Select(c => ToDto(c, true)));
}).RequireAuthorization("OfficerOnly");

claimsApi.MapGet("/{id:guid}", async (Guid id, ClaimsDbContext db, ClaimsPrincipal p) =>
{
    var c = await db.Claims.Include(x => x.StatusHistory).Include(x => x.Notes).FirstOrDefaultAsync(x => x.Id == id);
    if (c is null) return Results.NotFound();
    var isOfficer = p.IsInRole("ClaimsOfficer");
    if (!isOfficer && c.ClaimantId.ToString() != p.FindFirstValue(ClaimTypes.NameIdentifier)) return Results.Forbid();
    return Results.Ok(new { claim = ToDto(c, isOfficer), history = c.StatusHistory.OrderBy(x => x.ChangedAt), notes = isOfficer ? c.Notes : null });
});

claimsApi.MapPost("/{id:guid}/assign", async (Guid id, ClaimsDbContext db, ClaimsPrincipal p) =>
{
    var c = await db.Claims.FindAsync(id);
    if (c is null) return Results.NotFound();
    if (c.Status is ClaimStatus.Rejected or ClaimStatus.Settled) return Results.BadRequest(new { message = "Closed claims cannot be assigned." });
    var officerId = Guid.Parse(p.FindFirstValue(ClaimTypes.NameIdentifier)!);
    c.AssignedOfficerId = officerId;
    if (c.Status == ClaimStatus.Submitted) c.Status = ClaimStatus.UnderReview;
    AddHistory(db, c, officerId, "Claim assigned to officer");
    await db.SaveChangesAsync();
    return Results.Ok(ToDto(c, true));
}).RequireAuthorization("OfficerOnly");

claimsApi.MapPost("/{id:guid}/request-information", async (Guid id, ActionRequest req, ClaimsDbContext db, ClaimsPrincipal p) =>
{
    if (string.IsNullOrWhiteSpace(req.Reason)) return Results.BadRequest(new { message = "Information request reason is required." });
    var c = await db.Claims.FindAsync(id); if (c is null) return Results.NotFound();
    if (c.Status is ClaimStatus.Rejected or ClaimStatus.Settled) return Results.BadRequest(new { message = "Claim is closed." });
    c.Status = ClaimStatus.InformationRequested;
    AddHistory(db, c, Guid.Parse(p.FindFirstValue(ClaimTypes.NameIdentifier)!), req.Reason);
    await db.SaveChangesAsync(); return Results.Ok(ToDto(c, true));
}).RequireAuthorization("OfficerOnly");

claimsApi.MapPost("/{id:guid}/provide-information", async (Guid id, ActionRequest req, ClaimsDbContext db, ClaimsPrincipal p) =>
{
    var c = await db.Claims.FindAsync(id); if (c is null) return Results.NotFound();
    if (c.ClaimantId.ToString() != p.FindFirstValue(ClaimTypes.NameIdentifier)) return Results.Forbid();
    if (c.Status != ClaimStatus.InformationRequested) return Results.BadRequest(new { message = "Information has not been requested." });
    c.AdditionalInformation = req.Reason;
    c.Status = ClaimStatus.UnderReview;
    AddHistory(db, c, c.ClaimantId, "Claimant provided additional information");
    await db.SaveChangesAsync(); return Results.Ok(ToDto(c, false));
}).RequireAuthorization("ClaimantOnly");

claimsApi.MapPost("/{id:guid}/approve", async (Guid id, AmountRequest req, ClaimsDbContext db, ClaimsPrincipal p) =>
{
    if (req.Amount <= 0) return Results.BadRequest(new { message = "Approved amount must be positive." });
    var c = await db.Claims.FindAsync(id); if (c is null) return Results.NotFound();
    if (c.Status is not (ClaimStatus.UnderReview or ClaimStatus.InformationRequested)) return Results.BadRequest(new { message = "Only claims under review can be approved." });
    c.ApprovedAmount = req.Amount; c.Status = ClaimStatus.Approved;
    AddHistory(db, c, Guid.Parse(p.FindFirstValue(ClaimTypes.NameIdentifier)!), "Claim approved");
    await db.SaveChangesAsync(); return Results.Ok(ToDto(c, true));
}).RequireAuthorization("OfficerOnly");

claimsApi.MapPost("/{id:guid}/reject", async (Guid id, ActionRequest req, ClaimsDbContext db, ClaimsPrincipal p) =>
{
    if (string.IsNullOrWhiteSpace(req.Reason)) return Results.BadRequest(new { message = "Rejection reason is required." });
    var c = await db.Claims.FindAsync(id); if (c is null) return Results.NotFound();
    if (c.Status is ClaimStatus.Rejected or ClaimStatus.Settled) return Results.BadRequest(new { message = "Claim is already closed." });
    c.Status = ClaimStatus.Rejected;
    AddHistory(db, c, Guid.Parse(p.FindFirstValue(ClaimTypes.NameIdentifier)!), req.Reason);
    await db.SaveChangesAsync(); return Results.Ok(ToDto(c, true));
}).RequireAuthorization("OfficerOnly");

claimsApi.MapPost("/{id:guid}/settle", async (Guid id, AmountRequest req, ClaimsDbContext db, ClaimsPrincipal p) =>
{
    if (req.Amount <= 0) return Results.BadRequest(new { message = "Settlement amount must be positive." });
    var c = await db.Claims.FindAsync(id); if (c is null) return Results.NotFound();
    if (c.Status != ClaimStatus.Approved) return Results.BadRequest(new { message = "Only approved claims can be settled." });
    c.SettlementAmount = req.Amount; c.SettledAt = DateTime.UtcNow; c.Status = ClaimStatus.Settled;
    AddHistory(db, c, Guid.Parse(p.FindFirstValue(ClaimTypes.NameIdentifier)!), "Claim settled");
    await db.SaveChangesAsync(); return Results.Ok(ToDto(c, true));
}).RequireAuthorization("OfficerOnly");

app.MapGet("/api/dashboard/officer", async (ClaimsDbContext db) =>
{
    var all = await db.Claims.ToListAsync();
    var open = all.Where(c => c.Status is not (ClaimStatus.Rejected or ClaimStatus.Settled)).ToList();
    return Results.Ok(new {
        totalClaims = all.Count, submitted = all.Count(c => c.Status == ClaimStatus.Submitted),
        underReview = all.Count(c => c.Status == ClaimStatus.UnderReview),
        informationRequested = all.Count(c => c.Status == ClaimStatus.InformationRequested),
        approved = all.Count(c => c.Status == ClaimStatus.Approved),
        rejected = all.Count(c => c.Status == ClaimStatus.Rejected),
        settled = all.Count(c => c.Status == ClaimStatus.Settled),
        outstandingLiability = open.Sum(c => c.ApprovedAmount ?? c.EstimatedAmount),
        claims = all.OrderByDescending(c => c.CreatedAt).Take(10).Select(c => ToDto(c, true))
    });
}).RequireAuthorization("OfficerOnly");

app.Run();

static void AddHistory(ClaimsDbContext db, ClaimRecord c, Guid actor, string comment) =>
    db.StatusHistory.Add(new ClaimStatusHistory { ClaimId = c.Id, Status = c.Status, ChangedBy = actor, ChangedAt = DateTime.UtcNow, Comment = comment });

static object ToDto(ClaimRecord c, bool officer) => new {
    c.Id, c.ClaimNumber, c.ClaimType, c.PolicyNumber, c.IncidentDate, c.IncidentLocation,
    c.Description, c.EstimatedAmount, c.ApprovedAmount, c.SettlementAmount, c.Market,
    c.Currency, status = c.Status.ToString(), c.CreatedAt, c.SettledAt, c.AdditionalInformation,
    assignedOfficerId = officer ? c.AssignedOfficerId : null
};

public partial class Program { }
