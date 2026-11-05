namespace ChubbClaims.Api.Models;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Name, string Role);
public record CreateClaimRequest(string ClaimType, string PolicyNumber, DateTime IncidentDate,
    string IncidentLocation, string Description, decimal EstimatedAmount, string? Market, string? Currency);
public record ActionRequest(string Reason);
public record AmountRequest(decimal Amount);
