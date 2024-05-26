using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace raBooth.Core.Helpers
{
    public static class ImageProcessing
    {
        public static void ResizeToCover(Mat image, Size size)
        {
            if (image.Rows == 0 || image.Cols == 0)
            {
                var emptyImage = new Mat(size, MatType.CV_8UC3, new Scalar(255,255,255));
                emptyImage.CopyTo(image);
                return;
            }
            var wScalingRatio = (float)size.Width / image.Cols;
            var hScalingRatio = (float)size.Height / image.Rows;

            var noScalingRequired = Math.Abs(wScalingRatio - 1) < 0.001f && Math.Abs(hScalingRatio - 1) < 0.001f;
            if (noScalingRequired)
            {
                return;
            }

            var scalingRatio = Math.Max(wScalingRatio, hScalingRatio);

            var enlargeRequired = scalingRatio > 0;
            var interpolation = enlargeRequired ? InterpolationFlags.Cubic : InterpolationFlags.Area;
            Cv2.Resize(image, image, new(), scalingRatio, scalingRatio, interpolation);
        }

        public static void CropToSizeFromCenter(Mat image, Size size)
        {
            if (size.Width >= image.Cols && size.Height >= image.Rows)
            {
                return;
            }

            var centerX = image.Cols / 2;
            var centerY = image.Rows / 2;

            var rowStart = Math.Clamp(centerY - size.Height / 2, 0, image.Rows);
            var rowEnd = Math.Clamp(rowStart + size.Height, 0, image.Rows);
            var colStart = Math.Clamp(centerX - size.Width / 2, 0, image.Cols);
            var colEnd = Math.Clamp(colStart + size.Width, 0, image.Cols);

            image[rowStart, rowEnd, colStart, colEnd].CopyTo(image);
        }
    }
}
