using System.ComponentModel.DataAnnotations.Schema;

namespace VibeCode.Shared.Entities;

public class Permission : BaseEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string? UserId { get; set; }
    public string? RoleId { get; set; }
    public int PageId { get; set; }
    public bool CanAccess { get; set; }

    public Page? Page { get; set; }
}
