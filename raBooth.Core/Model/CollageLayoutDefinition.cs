using System.Runtime.Intrinsics.Arm;
using OpenCvSharp;
using raBooth.Core.Helpers;

namespace raBooth.Core.Model
{
    public class CollageLayoutDefinition
    {
        public float WidthInMm { get; init; }
        public float HeightInMm { get; init; }
        public Size2f SizeInMm => new (WidthInMm, HeightInMm);
        public float StrokeWidthInMm { get; init; }
        public int Dpi { get; init; } = 300;
        public Size Size => UnitConversion.SizeInMmToPixelSize(SizeInMm, Dpi);
        public int StrokeWidth => (int)UnitConversion.MillimetersToPixels(StrokeWidthInMm, Dpi);
        public List<CollageRowDefinition> Rows { get; init; } = new();


    }

    public class CollageRowDefinition
    {

        public int HeightRatio { get; init; }
        public List<CollageRowItemDefinition> Items { get; init; } = new();
    }

    public class CollageRowItemDefinition
    {

        public int WidthRatio { get; init; }
    }
}