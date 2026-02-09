namespace VibeCode.Main.Settings
{
    public class CookieSettings
    {
        public string LoginPath { get; set; } = "/Account/Login";
        public string LogoutPath { get; set; } = "/Account/Logout";
        public string AccessDeniedPath { get; set; } = "/Account/AccessDenied";
        public int DefaultExpireSeconds { get; set; } = 3600;
        public bool SlidingExpiration { get; set; } = true;
        public int RememberMeExpireDays { get; set; } = 14;
        public string CookiePath { get; set; } = "/";
    }
}
