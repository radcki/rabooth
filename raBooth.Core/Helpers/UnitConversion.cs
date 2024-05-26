using OpenCvSharp;

namespace raBooth.Core.Helpers;

public static class UnitConversion
{
    public static decimal MillimetersToInches(decimal millimeters)
    {
        return millimeters / 25.4M;
    }

    public static int MillimetersToPixels(float sizeInMm, int dpi)
    {
        return (int)(MillimetersToInches((decimal)sizeInMm) * dpi);
    }
    public static Size SizeInMmToPixelSize(Size2f sizeInMm, int dpi)
    {
        return new(MillimetersToPixels(sizeInMm.Width, dpi), MillimetersToPixels(sizeInMm.Height, dpi));
    }
}