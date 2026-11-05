namespace ChubbClaims.Api.Models;

public enum ClaimStatus { Submitted, UnderReview, InformationRequested, Approved, Rejected, Settled }

public class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Claimant";
}

public class ClaimRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ClaimNumber { get; set; } = "";
    public Guid ClaimantId { get; set; }
    public Guid? AssignedOfficerId { get; set; }
    public string ClaimType { get; set; } = "Motor";
    public string PolicyNumber { get; set; } = "";
    public DateTime IncidentDate { get; set; }
    public string IncidentLocation { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal EstimatedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public decimal? SettlementAmount { get; set; }
    public string Market { get; set; } = "SG";
    public string Currency { get; set; } = "SGD";
    public ClaimStatus Status { get; set; }
    public string? AdditionalInformation { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SettledAt { get; set; }
    public List<ClaimStatusHistory> StatusHistory { get; set; } = [];
    public List<ClaimNote> Notes { get; set; } = [];
}

public class ClaimStatusHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClaimId { get; set; }
    public ClaimStatus Status { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string Comment { get; set; } = "";
}

public class ClaimNote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClaimId { get; set; }
    public Guid AuthorId { get; set; }
    public string Text { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
