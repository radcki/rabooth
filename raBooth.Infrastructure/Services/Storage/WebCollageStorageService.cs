using raBooth.Core.Model;
using raBooth.Core.Services.Storage;
using raBooth.Core.Services.Web;
using System.Diagnostics;
using raBooth.Core.Helpers;

namespace raBooth.Infrastructure.Services.Storage
{
    public class WebCollageStorageService(IWebHostApiClient httpClient) : ICollageStorageService
    {
        public async Task StoreCollage(CollageLayout collage, IProgress<StoreCollageProgress> progress, CancellationToken cancellationToken)
        {
            var executionTime = Stopwatch.StartNew();
            var image = collage.GetView();
            var imageData = ImageProcessing.EncodeToJpg(image);
            var collageItems = collage.GetCollageItems().ToList();
            var requestToMake = 1 + collageItems.Count;
            var requestsDone = 0;
            cancellationToken.ThrowIfCancellationRequested();
            progress.Report(new StoreCollageProgress(requestsDone, requestToMake, executionTime.Elapsed));
            var collageCreationResult = await httpClient.CreateCollage(new CreateCollage.Command(collage.LastItemCaptureUtcTime, imageData), cancellationToken);
            requestsDone++;
            progress.Report(new StoreCollageProgress(requestsDone, requestToMake, executionTime.Elapsed));
            foreach (var collageItem in collageItems)
            {
                var sourceItemImage = collageItem.GetSourceImage();
                var sourceItemImageData = ImageProcessing.EncodeToJpg(sourceItemImage);
                cancellationToken.ThrowIfCancellationRequested();
                await httpClient.AddSourceCollagePhoto(new AddSourceCollagePhoto.Command(collageCreationResult.CollageId, collageItem.CaptureUtcDate.Value, sourceItemImageData), cancellationToken);
                requestsDone++;
                progress.Report(new StoreCollageProgress(requestsDone, requestToMake, executionTime.Elapsed));
            }
        }

    }
}
