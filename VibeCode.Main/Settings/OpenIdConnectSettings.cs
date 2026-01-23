namespace VibeCode.Main.Settings
{
    public class OpenIdConnectSettings
    {
        public string ResponseType { get; set; } = "code";
        public bool UsePkce { get; set; } = true;
        public bool SaveTokens { get; set; } = true;
        public string CallbackPath { get; set; } = "/signin-oidc";
        public string SignedOutCallbackPath { get; set; } = "/signout-callback-oidc";
        public bool RequireHttpsMetadata { get; set; } = false;
        public bool GetClaimsFromUserInfoEndpoint { get; set; } = false;
        public string[] Scopes { get; set; } = new[] { "openid", "profile", "email", "roles" };
        public string FailureRedirectPath { get; set; } = "/";
    }
}
