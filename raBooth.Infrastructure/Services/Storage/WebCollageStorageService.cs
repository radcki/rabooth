using raBooth.Core.Model;
using raBooth.Core.Services.Storage;
using raBooth.Core.Services.Web;
using System.Diagnostics;

namespace raBooth.Infrastructure.Services.Storage
{
    public class WebCollageStorageService(IWebHostApiClient httpClient) : ICollageStorageService
    {
        public async Task StoreCollage(CollageLayout collage, IProgress<StoreCollageProgress> progress, CancellationToken cancellationToken)
        {
            var executionTime = Stopwatch.StartNew();
            var image = collage.GetView();
            var sourceImages = collage.GetSourceItemImages().ToList();
            var requestToMake = 1 + sourceImages.Count;
            var requestsDone = 0;

            progress.Report(new StoreCollageProgress(requestsDone, requestToMake, executionTime.Elapsed));
            var collageCreationResult = await httpClient.CreateCollage(new CreateCollage.Command());
            requestsDone++;
            progress.Report(new StoreCollageProgress(requestsDone, requestToMake, executionTime.Elapsed));
            foreach (var sourceItemImage in collage.GetSourceItemImages())
            {
                await httpClient.AddSourceCollagePhoto(new AddSourceCollagePhoto.Command(collageCreationResult.CollageId));
                requestsDone++;
                progress.Report(new StoreCollageProgress(requestsDone, requestToMake, executionTime.Elapsed));
            }
        }


    }
}
