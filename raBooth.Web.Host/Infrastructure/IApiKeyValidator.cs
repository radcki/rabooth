namespace raBooth.Web.Host.Infrastructure;

public interface IApiKeyValidator
{
    bool IsValid(string apiKey);
}