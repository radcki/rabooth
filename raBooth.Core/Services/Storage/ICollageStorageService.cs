using raBooth.Core.Model;

namespace raBooth.Core.Services.Storage;

public interface ICollageStorageService
{
    Task StoreCollage(CollageLayout collage, IProgress<StoreCollageProgress> progress, CancellationToken cancellationToken);
}

public record StoreCollageProgress(int Done, int Total, TimeSpan ElapsedTime);