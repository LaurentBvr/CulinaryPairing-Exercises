using Microsoft.AspNetCore.Identity;

namespace CulinaryPairing.Domain.Users;

public class CulinaryUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? FamilyId { get; set; }
    public string FullName => $"{FirstName} {LastName}";
}