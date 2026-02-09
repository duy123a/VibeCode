using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using VibeCode.IdentityServer.Settings;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace VibeCode.IdentityServer.HostedServices;

public sealed class OpenIddictSeedWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly OpenIddictClientSettings _clientOptions;

    public OpenIddictSeedWorker(
        IServiceProvider serviceProvider,
        IOptions<OpenIddictClientSettings> clientOptions)
    {
        _serviceProvider = serviceProvider;
        _clientOptions = clientOptions.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var appManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        var existing = await appManager.FindByClientIdAsync(
            _clientOptions.ClientId, cancellationToken);

        if (existing != null)
            return;

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = _clientOptions.ClientId,
            ClientSecret = _clientOptions.ClientSecret,
            ClientType = ClientTypes.Confidential,
            DisplayName = "VibeCode Main Application",
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.EndSession,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Roles
            },
            Requirements =
            {
                Requirements.Features.ProofKeyForCodeExchange
            }
        };

        descriptor.RedirectUris.Add(
            new Uri($"{_clientOptions.BaseUrl.TrimEnd('/')}/signin-oidc"));

        descriptor.PostLogoutRedirectUris.Add(
            new Uri($"{_clientOptions.BaseUrl.TrimEnd('/')}/signout-callback-oidc"));

        await appManager.CreateAsync(descriptor, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
