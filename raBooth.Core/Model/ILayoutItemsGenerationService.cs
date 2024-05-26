namespace raBooth.Core.Model;

public interface ILayoutItemsGenerationService
{
    IEnumerable<CollageItem> GenerateItems(CollageLayoutDefinition layoutDefinition);
}