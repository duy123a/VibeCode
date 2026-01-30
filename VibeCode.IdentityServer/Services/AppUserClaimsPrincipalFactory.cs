using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using VibeCode.Shared.Constants;
using VibeCode.Shared.Entities;

namespace VibeCode.IdentityServer.Services
{
    public class AppUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser, AppRole>
    {
        public AppUserClaimsPrincipalFactory(
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // Add custom claim
            identity.AddClaim(new Claim(AppConstants.DisplayName, user.DisplayName ?? string.Empty));

            return identity;
        }
    }
}
