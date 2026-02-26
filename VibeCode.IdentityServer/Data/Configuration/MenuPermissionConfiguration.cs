using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeCode.Shared.Entities.Auth;

namespace VibeCode.IdentityServer.Data.Configuration
{
    public class MenuPermissionConfiguration : IEntityTypeConfiguration<MenuPermission>
    {
        public void Configure(EntityTypeBuilder<MenuPermission> builder)
        {
            builder.HasKey(x => new { x.MenuId, x.PermissionId });

            builder.HasOne(x => x.Menu)
                .WithMany(x => x.MenuPermissions)
                .HasForeignKey(x => x.MenuId);

            builder.HasOne(x => x.Permission)
                .WithMany(x => x.MenuPermissions)
                .HasForeignKey(x => x.PermissionId);
        }
    }
}
