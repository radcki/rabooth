namespace raBooth.Infrastructure.Services.Storage;

public class CollageStorageConfiguration
{
    public static string SectionName = "CollageStorageConfiguration";
    public bool CloudStorageEnabled { get; set; } = false;
    public bool LocalStorageEnabled { get; set; } = false;
    public string LocalStorageDirectory { get; init; } = @".\Storage\";
}