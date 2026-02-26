namespace VibeCode.Shared.Entities.Auth
{
    public class Permission
    {
        public int Id { get; set; }

        public string Code { get; set; } = null!; // USER_VIEW, USER_EDIT, etc.
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
        public ICollection<MenuPermission> MenuPermissions { get; set; } = new List<MenuPermission>();
    }
}
