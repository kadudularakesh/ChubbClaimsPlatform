using ChubbClaims.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ChubbClaims.Api.Data;

public class ClaimsDbContext(DbContextOptions<ClaimsDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<ClaimRecord> Claims => Set<ClaimRecord>();
    public DbSet<ClaimStatusHistory> StatusHistory => Set<ClaimStatusHistory>();
    public DbSet<ClaimNote> Notes => Set<ClaimNote>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<AppUser>().HasIndex(x => x.Email).IsUnique();
        b.Entity<ClaimRecord>().HasIndex(x => x.ClaimNumber).IsUnique();
        b.Entity<ClaimRecord>().Property(x => x.EstimatedAmount).HasPrecision(18, 2);
        b.Entity<ClaimRecord>().Property(x => x.ApprovedAmount).HasPrecision(18, 2);
        b.Entity<ClaimRecord>().Property(x => x.SettlementAmount).HasPrecision(18, 2);
        b.Entity<ClaimRecord>().HasMany(x => x.StatusHistory).WithOne().HasForeignKey(x => x.ClaimId);
        b.Entity<ClaimRecord>().HasMany(x => x.Notes).WithOne().HasForeignKey(x => x.ClaimId);
    }
}

public static class SeedData
{
    public static async Task EnsureSeeded(ClaimsDbContext db)
    {
        if (!db.Users.Any())
        {
            db.Users.AddRange(
                new AppUser { Name = "Demo Claimant", Email = "claimant@demo.com", Role = "Claimant", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Passw0rd!") },
                new AppUser { Name = "Demo Officer", Email = "officer@demo.com", Role = "ClaimsOfficer", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Passw0rd!") }
            );
            await db.SaveChangesAsync();
        }
    }
}
