namespace raBooth.Web.Host.Infrastructure;

public class ConfigApiKeyValidatorConfiguration
{
    public static string SectionName = "ApiKeysConfiguration";
    public List<string> ApiKeys { get; set; }
}

public class ConfigApiKeyValidator(ConfigApiKeyValidatorConfiguration configuration) : IApiKeyValidator
{
    public bool IsValid(string apiKey)
    {
        return configuration.ApiKeys.Contains(apiKey);
    }
}
