using raBooth.Core.Model;
using raBooth.Core.Services.Storage;
using raBooth.Core.Services.Web;
using System.Diagnostics;
using raBooth.Core.Helpers;
using System.Configuration;
using System.IO;
using Microsoft.Extensions.Logging;

namespace raBooth.Infrastructure.Services.Storage
{
    public class CollageStorageService(IWebHostApiClient httpClient, CollageStorageConfiguration configuration, ILogger<CollageStorageService> logger) : ICollageStorageService
    {
        public async Task StoreCollage(CollageLayout collage, IProgress<StoreCollageProgress> progress, CancellationToken cancellationToken)
        {
            var collageId = Guid.NewGuid();
            var executionTime = Stopwatch.StartNew();
            var image = collage.GetView();
            var imageData = ImageProcessing.EncodeToJpg(image);
            var collageItems = collage.GetCollageItems().ToList();
            var requestToMake = 1 + collageItems.Count;
            var requestsDone = 0;
            string pageUrl = null;
            cancellationToken.ThrowIfCancellationRequested();
            progress.Report(new StoreCollageProgress(requestsDone, requestToMake, executionTime.Elapsed, null));
            if (configuration.LocalStorageEnabled)
            {
                await StoreImageLocally(imageData, collage);
            }

            if (configuration.CloudStorageEnabled)
            {
                try
                {
                    var collageCreationResult = await httpClient.CreateCollage(new CreateCollage.Command(collageId, collage.LastItemCaptureUtcTime, imageData), cancellationToken);
                    pageUrl = collageCreationResult.PageUrl;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error during saving collage in cloud");
                }
            }

            requestsDone++;
            progress.Report(new StoreCollageProgress(requestsDone, requestToMake, executionTime.Elapsed, pageUrl));
            foreach (var collageItem in collageItems)
            {
                var sourceItemImage = collageItem.GetSourceImage();
                var sourceItemImageData = ImageProcessing.EncodeToJpg(sourceItemImage);
                cancellationToken.ThrowIfCancellationRequested();
                if (configuration.LocalStorageEnabled)
                {
                    await StoreImageLocally(sourceItemImageData, collage);
                }

                if (configuration.CloudStorageEnabled)
                {
                    try
                    {
                        await httpClient.AddSourceCollagePhoto(new AddSourceCollagePhoto.Command(collageId, collageItem.CaptureUtcDate.Value, sourceItemImageData), cancellationToken);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error during saving collage image in cloud");
                    }
                }

                requestsDone++;
                progress.Report(new StoreCollageProgress(requestsDone, requestToMake, executionTime.Elapsed, pageUrl));
            }
        }

        private async Task StoreImageLocally(byte[] data, CollageLayout collage)
        {
            try
            {
                var filePath = GetImagePath(collage);
                var fileDirectory = Path.GetDirectoryName(filePath);
                Directory.CreateDirectory(fileDirectory);
                await File.WriteAllBytesAsync(filePath, data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        private string GetImagePath(CollageLayout collage)
        {
            var directory = Path.Combine(configuration.LocalStorageDirectory, $"{collage.LastItemCaptureUtcTime.ToString("yyyy-MM-dd_HHmmss")}");
            var index = 0;
            if (Directory.Exists(directory))
            {
                index = Directory.GetFiles(directory, "*.jpg").Length;
            }

            var filename = $"{index:D2}.jpg";
            return Path.Combine(directory, filename);
        }
    }
}