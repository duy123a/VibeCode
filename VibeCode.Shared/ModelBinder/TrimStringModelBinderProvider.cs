using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace VibeCode.Shared.ModelBinder
{
    public class TrimStringModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            if (context.Metadata.ModelType == typeof(string))
            {
                var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
                return new TrimStringModelBinder(new SimpleTypeModelBinder(context.Metadata.ModelType, loggerFactory));
            }
            return null;
        }
    }
}
