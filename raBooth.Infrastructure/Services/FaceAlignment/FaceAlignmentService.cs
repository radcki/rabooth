using OpenCvSharp;
using raBooth.Core.Services.FaceDetection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace raBooth.Infrastructure.Services.FaceAlignment
{
    public class FaceAlignmentService
    {
        private readonly IFaceDetectionService _faceDetectionService;

        public FaceAlignmentService(IFaceDetectionService faceDetectionService)
        {
            _faceDetectionService = faceDetectionService;
        }

        private IEnumerable<Point> TransformPoints(IEnumerable<Point> points, Mat rotMat)
        {
            Mat transformedPoints = new Mat();
            Cv2.Transform(InputArray.Create(points), transformedPoints, rotMat);
            transformedPoints.GetArray(out Point[] results);
            return results;
        }

        public Mat GetAlignedFace(Mat image, DetectedFace detectedFace)
        {
            var eyes = _faceDetectionService.DetectEyes(image, detectedFace);
            if (eyes != null)
            {
                var rightEyeCenter = eyes.RightEye.GetCenter();
                var leftEyeCenter = eyes.LeftEye.GetCenter();
                var centerBetweenEyes = new Point((rightEyeCenter.X + leftEyeCenter.X) / 2, (rightEyeCenter.Y + leftEyeCenter.Y) / 2);


                var radians = Math.Atan2(rightEyeCenter.Y - leftEyeCenter.Y, rightEyeCenter.X - leftEyeCenter.X);

                var angle = (radians * 180) / Math.PI;
                var rotMat = Cv2.GetRotationMatrix2D(centerBetweenEyes, angle, 1.0);
                List<Point> untransformedCorner = [detectedFace.FaceArea.TopLeft, detectedFace.FaceArea.BottomRight];
                var transformedCorners = TransformPoints(untransformedCorner, rotMat).ToArray();
                var transformedEyes = TransformPoints(new[] { leftEyeCenter, centerBetweenEyes, rightEyeCenter }, rotMat).ToArray();
                var width = Math.Abs(transformedCorners[1].X - transformedCorners[0].X);
                var height = Math.Abs(transformedCorners[1].Y - transformedCorners[0].Y);
                var x = transformedEyes[1].X - width / 2;
                var y = transformedEyes[1].Y - height / 3;
                var cropRect = new Rect(x, y, width, height);


                var rotated = image.WarpAffine(rotMat, image.Size());


                var faceCrop = rotated[cropRect];

                return faceCrop;
            }

            return null;
        }
    }
}