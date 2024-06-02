using OpenCvSharp;
using raBooth.Core.Helpers;

namespace raBooth.Core.Model;

public class CollageItem
{
    public CollageItem(Size size, Point origin)
    {
        Size = size;
        Origin = origin;
    }

    public Size Size { get; private set; }
    public Point Origin { get; private set; }
    public Rect Area => new(Origin, Size);
    public DateTime? CaptureUtcDate { get; private set; }
    private Mat SourceImage { get; set; } = new();
    private bool _isCaptured { get; set; }

    public Mat GetImage()
    {
        var image = SourceImage.Clone();
        ImageProcessing.ResizeToCover(image, Size);
        ImageProcessing.CropToSizeFromCenter(image, Size);

        return image;
    }

    public void Capture(Mat image)
    {
        CaptureUtcDate = DateTime.UtcNow;
        _isCaptured = true;
        image.CopyTo(SourceImage);
    }

    public Mat GetSourceImage()
    {
        var mat = new Mat();
        SourceImage.CopyTo(mat);
        return mat;
    }
    public void UpdateSourceImage(Mat image)
    {
        if (!_isCaptured)
        {
            image.CopyTo(SourceImage);
        }
    }

    public void ClearSourceImage()
    {
        _isCaptured = false;
        SourceImage = new Mat();
    }
}