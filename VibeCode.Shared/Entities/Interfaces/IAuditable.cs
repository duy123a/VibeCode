namespace VibeCode.Shared.Entities.Interfaces;

public interface IAuditable
{
    DateTimeOffset CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTimeOffset UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
}
