using Microsoft.AspNetCore.Identity;
using VibeCode.Shared.Entities.Interfaces;

namespace VibeCode.Shared.Entities;

public class AppUser : IdentityUser, IAuditable
{
    public string DisplayName { get; set; } = string.Empty;
    public string ProfileImg { get; set; } = string.Empty;
    // Audit
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? UpdatedBy { get; set; }
}
