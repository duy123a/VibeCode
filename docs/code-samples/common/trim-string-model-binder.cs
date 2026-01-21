public class TrimStringModelBinder : IModelBinder
{
    private readonly IModelBinder FallbackBinder;

    public TrimStringModelBinder(IModelBinder fallbackBinder)
    {
        FallbackBinder = fallbackBinder ?? throw new ArgumentNullException(nameof(fallbackBinder));
    }

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult.FirstValue is string str &&
          !string.IsNullOrEmpty(str))
        {
            bindingContext.Result = ModelBindingResult.Success(str.Trim());
            return Task.CompletedTask;
        }
        return FallbackBinder.BindModelAsync(bindingContext);
    }
}
