using Microsoft.AspNetCore.Identity;
using VibeCode.Shared.Entities.Interfaces;

namespace VibeCode.Shared.Entities
{
    public class AppRole : IdentityRole, IAuditable
    {
        // Audit
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public string? UpdatedBy { get; set; }
    }
}
