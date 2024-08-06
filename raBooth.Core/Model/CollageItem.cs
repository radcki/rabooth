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
    private Rect? SourceImageCrop { get; set; }
    private readonly object _sourceImageLock = new();

    public Mat GetImage()
    {
        lock (_sourceImageLock)
        {
            var image = SourceImageCrop != null ? SourceImage.Clone(SourceImageCrop.Value) : SourceImage.Clone();
            ImageProcessing.ResizeToCover(image, Size);
            ImageProcessing.CropToSizeFromCenter(image, Size);

            return image;
        }
    }

    public void Capture(Mat image, Rect crop)
    {
        lock (_sourceImageLock)
        {
            CaptureUtcDate = DateTime.UtcNow;
            _isCaptured = true;
            SourceImageCrop = crop;
            image.CopyTo(SourceImage);
        }
    }

    public Mat GetSourceImage()
    {
        lock (_sourceImageLock)
        {
            var mat = new Mat();
            if (SourceImageCrop != null)
            {
                mat = SourceImage.Clone(SourceImageCrop.Value);
            }
            else
            {
                SourceImage.CopyTo(mat);
            }

            return mat;
        }
    }

    public void UpdateSourceImage(Mat image, Rect facesCrop)
    {
        if (!_isCaptured)
        {
            lock (_sourceImageLock)
            {
                SourceImageCrop = facesCrop;
                image.CopyTo(SourceImage);
            }
        }
    }

    public void ClearSourceImage()
    {
        lock (_sourceImageLock)
        {
            _isCaptured = false;
            SourceImageCrop = null;
            SourceImage = new Mat();
        }
    }
}