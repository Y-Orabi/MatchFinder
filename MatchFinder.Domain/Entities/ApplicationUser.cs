using Microsoft.AspNetCore.Identity;

namespace MatchFinder.Domain.Entities;

// Inherits from IdentityUser<Guid> which automatically gives you:
// - Id (Guid)
// - Email (string)
// - UserName (string)
// - PasswordHash (string)
// plus other security fields.
public class ApplicationUser : IdentityUser<Guid>
{
    // Your custom fields:
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public SkillLevel SkillLevel { get; set; } = SkillLevel.Beginner;
    public bool IsApprovedHost { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Match> HostedMatches { get; set; } = new List<Match>();
    public ICollection<MatchPlayer> MatchPlayers { get; set; } = new List<MatchPlayer>();
}