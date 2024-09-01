using FluentAssertions;
using OpenCvSharp;
using raBooth.Core.Helpers;
using System.Runtime.InteropServices;

namespace raBooth.Core.UnitTests
{
    public class UnitConversionTests
    {
        [Fact]
        public void MillimetersToPixels_Should_ReturnCorrectValue()
        {
            //Arrange
            var dpi = 300;
            var sizeInMm = 100;
            var expectedResult = 1181;

            //Act
            var conversionResult = UnitConversion.MillimetersToPixels(sizeInMm, dpi);

            //Assert
            conversionResult.Should().Be(expectedResult);
        }
        [Fact]
        public void SizeInMmToPixelSize_Should_ReturnCorrectValue()
        {
            //Arrange
            var dpi = 300;
            var sizeInMm = new Size2f(100, 148);
            var expectedResult = new Size(1181, 1748);

            //Act
            var conversionResult = UnitConversion.SizeInMmToPixelSize(sizeInMm, dpi);

            //Assert
            conversionResult.Height.Should().Be(expectedResult.Height);
            conversionResult.Width.Should().Be(expectedResult.Width);

        }

        [Fact]
        public void a()
        {
            var paths = FindExePath("cupti64_2022.3.0.dll").ToList();

        }
        public static IEnumerable<string> FindExePath(string exe)
        {
            exe = Environment.ExpandEnvironmentVariables(exe);

            foreach (string test in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';'))
            {
                string path = test.Trim();
                if (!string.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, exe)))
                    yield return Path.GetFullPath(path);
            }
        }
    }
}