public class TrimmingModelBinderProvider : IModelBinderProvider
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
