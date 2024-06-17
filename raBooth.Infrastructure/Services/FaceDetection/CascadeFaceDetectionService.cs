using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Face;
using raBooth.Core.Helpers;
using raBooth.Core.Services.FaceDetection;

namespace raBooth.Infrastructure.Services.FaceDetection
{

    public class CascadeFaceDetectionService : IFaceDetectionService
    {

        private readonly CascadeClassifier _faceClassifier;
        private readonly CascadeClassifier _smileClassifier;
        private readonly CascadeClassifier _eyeClassifier;

        public CascadeFaceDetectionService()
        {
            _eyeClassifier = new CascadeClassifier(".\\Services\\FaceDetection\\haarcascade_eye_tree_eyeglasses.xml");
            _smileClassifier = new CascadeClassifier(".\\Services\\FaceDetection\\haarcascade_smile.xml");
            _faceClassifier = new CascadeClassifier(".\\Services\\FaceDetection\\haarcascade_frontalface_default.xml");
        }
        public IEnumerable<DetectedFace> DetectFaces(Mat image)
        {

            using var src = image.Clone();
            var expectedWidth = 300;
            var faceScaling = 1 / (src.Cols / (double)expectedWidth);
            var grey = new Mat();
            Cv2.CvtColor(src, grey, ColorConversionCodes.BGR2GRAY);
            var small = new Mat();
            Cv2.Resize(grey, small, new Size(0, 0), faceScaling, faceScaling);
            var sw = Stopwatch.StartNew();
            var faces = _faceClassifier.DetectMultiScale(small, 1.1, 8);
            var t1 = sw.Elapsed.TotalMilliseconds;
            sw.Restart();
            foreach (var face in faces)
            {
                var faceArea = ImageProcessing.ScaleRect(face, 1 / faceScaling);

                yield return new DetectedFace(faceArea);

            }
        }
        public DetectedEyes? DetectEyes(Mat image, DetectedFace detectedFace)
        {

            using var src = image[detectedFace.FaceArea].Clone();
            var expectedWidth = 300;
            var faceScaling = 1 / (src.Cols / (double)expectedWidth);
            var grey = new Mat();
            Cv2.CvtColor(src, grey, ColorConversionCodes.BGR2GRAY);
            var small = new Mat();
            Cv2.Resize(grey, small, new Size(0, 0), faceScaling, faceScaling);

            var eyes = _eyeClassifier.DetectMultiScale(small, 1.1, 3);

            var twoLargestEyes = eyes.OrderByDescending(x => x.Width * x.Height).Take(2).ToList();
            if (twoLargestEyes.Count != 2)
            {
                return null;
            }

            var leftEye = ImageProcessing.MoveRect(ImageProcessing.ScaleRect(twoLargestEyes.MinBy(x => x.X), 1 / faceScaling), detectedFace.FaceArea.Location);
            var rightEye = ImageProcessing.MoveRect(ImageProcessing.ScaleRect(twoLargestEyes.MaxBy(x => x.X), 1 / faceScaling), detectedFace.FaceArea.Location);


            return new DetectedEyes(new DetectedEye(leftEye), new DetectedEye(rightEye));
        }

    }
}
