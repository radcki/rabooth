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

    private Mat SourceImage { get; set; } = new();

    public Mat GetImage()
    {
        var image = SourceImage.Clone();
        ImageProcessing.ResizeToCover(image, Size);
        ImageProcessing.CropToSizeFromCenter(image, Size);

        return image;
    }

    public Mat GetSourceImage()
    {
        var mat = new Mat();
        SourceImage.CopyTo(mat);
        return mat;
    }
    public void UpdateSourceImage(Mat image)
    {

        image.CopyTo(SourceImage);
    }

    public void ClearSourceImage()
    {
        SourceImage = new Mat();
    }
}