using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using VibeCode.IdentityServer.Constants;
using VibeCode.Shared.Entities;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace VibeCode.IdentityServer.Controllers;

public class AuthorizationController : Controller
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;

    public AuthorizationController(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Get user from Identity cookie
        var authResult = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!authResult.Succeeded || authResult.Principal == null)
        {
            // Redirect to login if not login
            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                });
        }

        var userPrincipal = authResult.Principal;

        var userId = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return NotFound();
        var userEntity = await _userManager.FindByIdAsync(userId);

        // Create claims for OpenIddict token
        var claims = new List<Claim>
            {
                new Claim(OpenIddictConstants.Claims.Subject, userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)!),
                new Claim(OpenIddictConstants.Claims.Name, userPrincipal.Identity?.Name ?? string.Empty),
                new Claim(OpenIddictConstants.Claims.Email, userPrincipal.FindFirstValue(ClaimTypes.Email) ?? string.Empty),
                new Claim(AppConstants.PreferredLanguage, userPrincipal.FindFirstValue(AppConstants.PreferredLanguage) ?? SupportedCultures.Default),
                new Claim(AppConstants.DisplayName, userEntity?.DisplayName ?? string.Empty),
                new Claim(AppConstants.ProfileImage, userEntity?.ProfileImg ?? string.Empty),
            };

        // Get roles from Identity user

        if (!string.IsNullOrEmpty(userId))
        {

            if (userEntity != null)
            {
                var roles = await _userManager.GetRolesAsync(userEntity);
                foreach (var role in roles ?? Enumerable.Empty<string>())
                    claims.Add(new Claim(OpenIddictConstants.Claims.Role, role));
            }
        }

        // Create identity + principal
        var identity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        // Assign scope by request
        principal.SetScopes(request.GetScopes());
        principal.SetResources("auth");

        // Must set destination for token
        foreach (var claim in principal.Claims)
        {
            claim.SetDestinations(GetDestinations(claim, principal));
        }

        var signInResult = SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        return signInResult;
    }

    [HttpPost("~/connect/token"), Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        ClaimsPrincipal principal;

        if (request.IsClientCredentialsGrantType())
        {
            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            identity.AddClaim(OpenIddictConstants.Claims.Subject, request.ClientId ?? throw new InvalidOperationException());
            principal = new ClaimsPrincipal(identity);
            principal.SetScopes(request.GetScopes());
        }
        else if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            var authResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (!authResult.Succeeded || authResult.Principal == null)
                throw new InvalidOperationException("Cannot retrieve principal from token.");

            principal = authResult.Principal;
        }
        else
        {
            throw new InvalidOperationException("The specified grant type is not supported.");
        }

        var signInResult = SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        return signInResult;
    }

    [HttpGet("~/connect/logout")]
    public IActionResult Logout() => View();

    [ActionName(nameof(Logout)), HttpPost("~/connect/logout"), ValidateAntiForgeryToken]
    public async Task<IActionResult> LogoutPost()
    {
        await _signInManager.SignOutAsync();

        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties
            {
                RedirectUri = "/"
            });
    }

    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo")]
    public async Task<IActionResult> Userinfo()
    {
        var authResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var principal = authResult.Principal;

        if (principal == null)
            return Unauthorized();

        var roles = principal.FindAll(OpenIddictConstants.Claims.Role).Select(c => c.Value).ToArray();

        var userId = principal.FindFirstValue(OpenIddictConstants.Claims.Subject);
        if (string.IsNullOrEmpty(userId)) return NotFound();
        var userEntity = await _userManager.FindByIdAsync(userId);

        return Ok(new
        {
            sub = principal.FindFirstValue(OpenIddictConstants.Claims.Subject),
            name = principal.FindFirstValue(OpenIddictConstants.Claims.Name),
            email = principal.FindFirstValue(OpenIddictConstants.Claims.Email),
            role = roles,
            display_name = userEntity?.DisplayName ?? string.Empty,
            profile_image = userEntity?.ProfileImg ?? string.Empty,
        });
    }

    private static IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
    {
        // Note: by default, claims are NOT automatically included in the access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, in identity tokens or in both.
        switch (claim.Type)
        {
            case Claims.Name:
                yield return Destinations.AccessToken;

                if (principal.HasScope(Scopes.Profile))
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Email:
                yield return Destinations.AccessToken;

                if (principal.HasScope(Scopes.Email))
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Role:
                yield return Destinations.AccessToken;

                if (principal.HasScope(Scopes.Roles))
                    yield return Destinations.IdentityToken;

                yield break;

            // Never include the security stamp in the access and identity tokens, as it's a secret value.
            case "AspNet.Identity.SecurityStamp": yield break;

            default:
                yield return Destinations.AccessToken;
                yield return Destinations.IdentityToken;
                yield break;
        }
    }
}
