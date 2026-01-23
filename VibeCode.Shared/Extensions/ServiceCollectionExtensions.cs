using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using VibeCode.Shared.Constants;

namespace VibeCode.Shared.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppLocalization(this IServiceCollection services)
        {
            var cultures = SupportedCultures.All.Select(c => new CultureInfo(c)).ToList();

            services.AddLocalization();

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(SupportedCultures.Default);
                options.SupportedCultures = cultures;
                options.SupportedUICultures = cultures;

                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new QueryStringRequestCultureProvider(),   // ?culture=vi
                    new CookieRequestCultureProvider
                    {
                        CookieName = CookieRequestCultureProvider.DefaultCookieName
                    },
                    new AcceptLanguageHeaderRequestCultureProvider() // Accept-Language: vi-VN
                };
            });

            return services;
        }
    }
}
