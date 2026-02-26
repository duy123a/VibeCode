using Microsoft.AspNetCore.Identity;
using VibeCode.Shared.Entities.Auth;
using VibeCode.Shared.Entities.Interfaces;

namespace VibeCode.Shared.Entities;

public class AppUser : IdentityUser, IAuditable
{
    public string DisplayName { get; set; } = string.Empty;
    public string ProfileImg { get; set; } = string.Empty;

    // Navigation
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();

    // Audit
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? UpdatedBy { get; set; }
}
