namespace raBooth.Core.Model;

public interface ILayoutGenerationService
{
    CollageLayout GenerateLayout(CollageLayoutDefinition layoutDefinition);
}