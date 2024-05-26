using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using OpenCvSharp;
using raBooth.Core.Helpers;

namespace raBooth.Core.UnitTests
{
    public class ImageProcessingTests
    {
        [Fact]
        public void ResizeToCover_ShouldResizeToExpectedSize_WhenEnlarge()
        {
            //Arrange
            var image = new Mat(new Size(100, 100), MatType.CV_8UC3, new Scalar(255, 255, 255));
            var sizeToCover = new Size(100, 500);
            var expectedSize = new Size(500, 500);

            //Act
            ImageProcessing.ResizeToCover(image, sizeToCover);

            //Assert
            image.Size().Should().Be(expectedSize);
        } 
        
        [Fact]
        public void ResizeToCover_ShouldResizeToExpectedSize_WhenEnlarge2()
        {
            //Arrange
            var image = new Mat(new Size(100, 100), MatType.CV_8UC3, new Scalar(255, 255, 255));
            var sizeToCover = new Size(50, 500);
            var expectedSize = new Size(500, 500);

            //Act
            ImageProcessing.ResizeToCover(image, sizeToCover);

            //Assert
            image.Size().Should().Be(expectedSize);
        }

        [Fact]
        public void ResizeToCover_ShouldResizeToExpectedSize_WhenShrinking()
        {
            //Arrange
            var image = new Mat(new Size(500, 200), MatType.CV_8UC3, new Scalar(255, 255, 255));
            var sizeToCover = new Size(100, 100);
            var expectedSize = new Size(250, 100);

            //Act
            ImageProcessing.ResizeToCover(image, sizeToCover);

            //Assert
            image.Size().Should().Be(expectedSize);
        }
        [Fact]
        public void ResizeToCover_ShouldNotScale_WhenSizeMatches()
        {
            //Arrange
            var image = new Mat(new Size(123, 123), MatType.CV_8UC3, new Scalar(255, 255, 255));
            var sizeToCover = new Size(123, 123);
            var expectedSize = new Size(123, 123);

            //Act
            ImageProcessing.ResizeToCover(image, sizeToCover);

            //Assert
            image.Size().Should().Be(expectedSize);
        }

        [Fact]
        public void CropToSizeFromCenter_ShouldCropToCorrectSize_WhenImageIsLarger()
        {
            //Arrange
            var image = new Mat(new Size(500, 200), MatType.CV_8UC3, new Scalar(255, 255, 255));
            var cropSize = new Size(100, 100);
            var expectedSize = new Size(100, 100);

            //Act
            ImageProcessing.CropToSizeFromCenter(image, cropSize);

            //Assert
            image.Size().Should().Be(expectedSize);
        }

        [Fact]
        public void CropToSizeFromCenter_ShouldCropToCorrectSize_WhenImageIsPartiallyLarger()
        {
            //Arrange
            var image = new Mat(new Size(500, 90), MatType.CV_8UC3, new Scalar(255, 255, 255));
            var cropSize = new Size(100, 100);
            var expectedSize = new Size(100, 90);

            //Act
            ImageProcessing.CropToSizeFromCenter(image, cropSize);

            //Assert
            image.Size().Should().Be(expectedSize);
        }

        [Fact]
        public void CropToSizeFromCenter_ShouldNotCrop_WhenImageIsSmaller()
        {
            //Arrange
            var image = new Mat(new Size(50, 50), MatType.CV_8UC3, new Scalar(255, 255, 255));
            var cropSize = new Size(100, 100);
            var expectedSize = image.Size();

            //Act
            ImageProcessing.CropToSizeFromCenter(image, cropSize);

            //Assert
            image.Size().Should().Be(expectedSize);
        }
    }
}
