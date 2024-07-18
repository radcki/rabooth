using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Printing;
using System.Reflection.Metadata;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Face;
using raBooth.Core.Services.FaceDetection;
using raBooth.Infrastructure.Services.FaceDetection;

namespace raBooth.FaceRecognition.Train
{
    internal class ModelTrainer
    {

        private readonly CascadeFaceDetectionService _faceDetectionService;
        private List<DatasetItem> _datasetItems = [];
        private EigenFaceRecognizer _recognizer;
        private List<DatasetLabel> _datasetLabels = [];

        private Size _datasetImageSize = new(226, 226);

        public ModelTrainer()
        {
            _faceDetectionService = new CascadeFaceDetectionService();
            _recognizer = EigenFaceRecognizer.Create();
        }

        public void LoadDataset(string datasetDirectory)
        {
            var labelDirectories = Directory.GetDirectories(datasetDirectory);
            int labelId = 0;
            ConcurrentBag<DatasetItem> datasetItems = [];
            foreach (var directory in labelDirectories)
            {
                labelId++;
                var label = Path.GetFileName(directory);
                var imagePaths = Directory.EnumerateFiles(directory, "*.jpg");
                var datasetLabel = new DatasetLabel(labelId, label);
                _datasetLabels.Add(datasetLabel);

                foreach (var imagePath in imagePaths)
                {
                    var image = Cv2.ImRead(imagePath);
                    var detectedFaces = _faceDetectionService.DetectFaces(image).ToList();


                    if (detectedFaces.Count > 1)
                    {
                        Console.WriteLine($"More than 1 face detected in file {imagePath}, skipping");
                        continue;
                    }
                    if (detectedFaces.Count == 0)
                    {
                        Console.WriteLine($"No faces detected in file {imagePath}, skipping");
                        continue;
                    }
                    var detectedFace = detectedFaces[0];



                    var faceCrop = GetDatasetImage(image, detectedFace);
                    datasetItems.Add(new DatasetItem(datasetLabel, faceCrop));
                    Console.WriteLine($"Face of {label} registered from file {imagePath}");

                }

            }
            foreach (var datasetItem in datasetItems)
            {
                Cv2.ImShow("img", datasetItem.Image);
                Cv2.WaitKey(0);
            }
            _datasetItems = datasetItems.ToList();
        }

        public void Train()
        {
            _recognizer.Train(_datasetItems.Select(x => x.Image), _datasetItems.Select(x => x.Label.Id));
        }

        public DatasetLabel Test(string filePath)
        {

            var image = Cv2.ImRead(filePath);
            var detectedFaces = _faceDetectionService.DetectFaces(image).ToList();
            var faceCrop = GetDatasetImage(image, detectedFaces.FirstOrDefault());
            _recognizer.Predict(faceCrop, out int labelId, out double confidence);
            var label = _datasetLabels.FirstOrDefault(x => x.Id == labelId);
            Console.WriteLine($"Face of {label.Name} detected with confidence {confidence}");
            return label;
        }

        private Rect RectToSquare(Rect rect)
        {
            var longSide = Math.Max(rect.Width, rect.Height);
            var x = (rect.X + (rect.Width / 2)) - (longSide / 2);
            var y = (rect.Y + (rect.Height / 2)) - (longSide / 2);
            return new Rect(x, y, longSide, longSide);
        }

        private Mat GetDatasetImage(Mat image, DetectedFace detectedFace)
        {
            var datasetImage = GetAlignedFace(image, detectedFace);
            if (datasetImage == null)
            {
                Console.WriteLine("Could not align image to eyes");
                var greyscale = image.CvtColor(ColorConversionCodes.BGR2GRAY);
                var faceCrop = greyscale[RectToSquare(detectedFace.FaceArea)];
                datasetImage = faceCrop.Resize(_datasetImageSize, interpolation: InterpolationFlags.Area);
            }

            return datasetImage;
        }
        private Rect CornersToRect(Point topLeft, Point bottomRight) => new Rect(topLeft, new Size(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y));

        private IEnumerable<Point> TransformPoints(IEnumerable<Point> points, Mat rotMat)
        {
            Mat transformedPoints = new Mat();
            Cv2.Transform(InputArray.Create(points), transformedPoints, rotMat);
            transformedPoints.GetArray(out Point[] results);
            return results;
        }

        private Mat GetAlignedFace(Mat image, DetectedFace detectedFace)
        {
            var eyes = _faceDetectionService.DetectEyes(image, detectedFace);
            var dec = image.Clone();
            if (eyes != null)
            {
                var rightEyeCenter = eyes.RightEye.GetCenter();
                var leftEyeCenter = eyes.LeftEye.GetCenter();
                var centerBetweenEyes = new Point((rightEyeCenter.X + leftEyeCenter.X) / 2, (rightEyeCenter.Y + leftEyeCenter.Y) / 2);
                //var prev = dec.Clone();
                //Cv2.Circle(prev,rightEyeCenter,5,Scalar.Blue, 10);
                //Cv2.Circle(prev,leftEyeCenter,5,Scalar.Red, 10);
                //Cv2.Circle(prev,centerBetweenEyes,5,Scalar.Green, 10);
                //Cv2.ImShow("prev", prev);
                var radians = Math.Atan2(rightEyeCenter.Y - leftEyeCenter.Y, rightEyeCenter.X - leftEyeCenter.X);

                var angle = (radians * 180) / Math.PI;
                var rotMat = Cv2.GetRotationMatrix2D(centerBetweenEyes, angle, 1.0);
                List<Point> untransformedCorner = [detectedFace.FaceArea.TopLeft, detectedFace.FaceArea.BottomRight];
                var transformedCorners = TransformPoints(untransformedCorner, rotMat).ToArray();
                var transformedEyes = TransformPoints(new[] { leftEyeCenter, centerBetweenEyes, rightEyeCenter }, rotMat).ToArray();
                var width = transformedCorners[1].X - transformedCorners[0].X;
                var height = transformedCorners[1].Y - transformedCorners[0].Y;
                var x = transformedEyes[1].X - width / 2;
                var y = transformedEyes[1].Y - height / 3;
                var cropRect = new Rect(x, y, width, height);

                //var mask = new Mat(dec.Size(), MatType.CV_8UC1, new Scalar(0));
                //mask[RectToSquare(detectedFace.FaceArea)].SetTo(new Scalar(1));

                var greyscale = dec.CvtColor(ColorConversionCodes.BGR2GRAY);
                var rotated = greyscale.WarpAffine(rotMat, image.Size());
                //var rotatedMask = mask.WarpAffine(rotMat, image.Size());

                //Cv2.ImShow("rotated", rotated);
                var crop = RectToSquare(cropRect);
                //var crop = RectToSquare(CornersToRect(transformedCorners[0], transformedCorners[1]));


                var faceCrop = rotated[crop];
                var resized = faceCrop.Resize(_datasetImageSize, interpolation: InterpolationFlags.Area);
                //Cv2.ImShow("face", dec);
                //Cv2.ImShow("rotated", resized);
                //Cv2.WaitKey(0);
                //Cv2.WaitKey(0);

                return resized;
            }
            else
            {

                //Cv2.ImShow("prev", image[detectedFace.FaceArea]);
                //Cv2.WaitKey(0);
            }

            return null;
        }
    }

    public record DatasetLabel(int Id, string Name);

    public class DatasetItem(DatasetLabel label, Mat image)
    {
        public DatasetLabel Label { get; private set; } = label;
        public Mat Image { get; private set; } = image;
    }
}
