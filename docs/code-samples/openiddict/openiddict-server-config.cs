builder.Services.AddOpenIddict()
    .AddCore(opt =>
    {
        opt.UseEntityFrameworkCore()
           .UseDbContext<AuthDbContext>();
    })
    .AddServer(opt =>
    {
        // Issuer configuration
        var issuerUrl = builder.Configuration["OpenIddict:IssuerUrl"];
        if (!string.IsNullOrEmpty(issuerUrl))
        {
            opt.SetIssuer(new Uri(issuerUrl));
        }

        // Endpoints
        opt.SetAuthorizationEndpointUris("/connect/authorize")
         .SetTokenEndpointUris("/connect/token")
         .SetRevocationEndpointUris("/connect/revoke")
         .SetUserInfoEndpointUris("/connect/userinfo")
         .SetEndSessionEndpointUris("/connect/logout");

        // Flows
        opt.AllowAuthorizationCodeFlow();

        // PKCE (Proof Key for Code Exchange)
        opt.RequireProofKeyForCodeExchange();

        // Scopes
        opt.RegisterScopes(
            OpenIddictConstants.Scopes.OpenId,
            OpenIddictConstants.Scopes.Email,
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.Roles
        );

        // Keys
        // Dev: ephemeral keys, Production: replace with certificate
        opt.AddEphemeralEncryptionKey()
           .AddEphemeralSigningKey();

        opt.DisableAccessTokenEncryption();

        // ASP.NET Core integration
        opt.UseAspNetCore()
           .EnableAuthorizationEndpointPassthrough()
           .EnableTokenEndpointPassthrough()
           .EnableUserInfoEndpointPassthrough()
           .EnableEndSessionEndpointPassthrough();
    })
    .AddValidation(opt =>
    {
        opt.UseLocalServer();
        opt.UseAspNetCore();
    });

// Seed demo clients
builder.Services.AddHostedService<DatabaseSeedWorker>();
