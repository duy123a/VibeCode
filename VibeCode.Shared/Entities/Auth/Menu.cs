namespace VibeCode.Shared.Entities.Auth
{
    public enum MenuTargetApp
    {
        Main = 1,
        Identity = 2
    }

    public class Menu
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string? Url { get; set; } // Controller/Action or route
        public MenuTargetApp TargetApp { get; set; }

        public string? Icon { get; set; }

        public int? ParentId { get; set; }
        public Menu? Parent { get; set; }
        public ICollection<Menu> Children { get; set; } = new List<Menu>();

        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
