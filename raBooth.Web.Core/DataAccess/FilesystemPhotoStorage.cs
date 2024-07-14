using raBooth.Web.Core.Entities;

namespace raBooth.Web.Core.DataAccess;

public class FilesystemPhotoStorageConfiguration
{
    public static string SectionName = nameof(FilesystemPhotoStorageConfiguration);
    public string DirectoryPath { get; init; }
}

public class FilesystemPhotoStorage(FilesystemPhotoStorageConfiguration configuration) : IPhotoStorage
{
    public async Task StoreImage(IPhoto reference, Collage collage, byte[] data)
    {
        var filePath = GetImagePath(reference, collage);
        var fileDirectory = Path.GetDirectoryName(filePath);
        Directory.CreateDirectory(fileDirectory);
        await File.WriteAllBytesAsync(filePath, data);
    }

    public async Task<byte[]> GetImage(IPhoto reference, Collage collage)
    {
        var filePath = GetImagePath(reference, collage);
        if (File.Exists(filePath))
        {
            return await File.ReadAllBytesAsync(filePath);
        }

        throw new FileNotFoundException();
    }

    private string GetImagePath(IPhoto reference, Collage collage)
    {

        var directory = Path.Combine(configuration.DirectoryPath, $"{collage.CaptureDateTime.ToString("yyyy-MM-dd_HHmmss")}_{reference.CollageId.ToString()}");
        var filename = $"{reference.PhotoId.ToString()}.jpg";
        return Path.Combine(directory, filename);
    }
}