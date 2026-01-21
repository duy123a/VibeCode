public class LoginViewModel
{
    [Display(ResourceType = typeof(SharedResources), Name = "Email")]
    [Required(
        ErrorMessageResourceName = "Required",
        ErrorMessageResourceType = typeof(SharedResources))]
    public string Email { get; set; } = string.Empty;

    [Display(ResourceType = typeof(SharedResources), Name = "Password")]
    [Required(
        ErrorMessageResourceName = "Required",
        ErrorMessageResourceType = typeof(SharedResources))]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(ResourceType = typeof(SharedResources), Name = "Remember_Me")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
