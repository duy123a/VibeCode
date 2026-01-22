using VibeCode.Shared.Entities.Interfaces;

namespace VibeCode.Shared.Entities
{
    public abstract class BaseEntity : IAuditable, ISoftDeletable
    {
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public string? CreatedBy { get; set; }

        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public string? UpdatedBy { get; set; }

        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}
