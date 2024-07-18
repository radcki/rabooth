using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FaceONNX;
using Microsoft.ML.OnnxRuntime;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using raBooth.Core.Helpers;
using raBooth.Core.Services.FaceDetection;
using Rect = OpenCvSharp.Rect;
using Size = OpenCvSharp.Size;

namespace raBooth.Infrastructure.Services.FaceDetection
{
    public class OnnxFaceDetectionService : IFaceDetectionService
    {

        private readonly IFaceDetector _faceDetector;
        private readonly FaceEmbedder _faceEmbedder;
        private readonly Face68LandmarksExtractor _faceLandmarksExtractor;

        public OnnxFaceDetectionService()
        {
            var options = SessionOptions.MakeSessionOptionWithCudaProvider();

            _faceLandmarksExtractor = new Face68LandmarksExtractor(options);
            _faceEmbedder = new FaceEmbedder(options);
            _faceDetector = new FaceDetector(options);
        }

        public IEnumerable<DetectedFace> DetectFaces(Mat image)
        {
            using var src = image.Clone();
            var faceScaling = 0.1;
            var small = new Mat();
            Cv2.Resize(src, small, new Size(0, 0), faceScaling, faceScaling);

            var bitmap = BitmapFromSource(small.ToBitmapSource());
            var detectionResults = _faceDetector.Forward(bitmap);
            foreach (var faceDetectionResult in detectionResults)
            {
                
                var points = _faceLandmarksExtractor.Forward(bitmap, faceDetectionResult.Box, true);
                var angle = points.RotationAngle;

                var aligned = FaceProcessingExtensions.Align(bitmap, faceDetectionResult.Box, angle);
                 var embeddings = _faceEmbedder.Forward(aligned);

                var faceBox = RectFromRectangle(faceDetectionResult.Box);

                var faceArea = ImageProcessing.ScaleRect(faceBox, 1 / faceScaling);
                yield return new DetectedFace(faceArea);
            }
        }

        /// <inheritdoc />
        public DetectedEyes? DetectEyes(Mat image, DetectedFace detectedFace)
        {
            throw new NotImplementedException();
        }

        public static BitmapSource ConvertBitmap(Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                                                                source.GetHbitmap(),
                                                                                IntPtr.Zero,
                                                                                Int32Rect.Empty,
                                                                                BitmapSizeOptions.FromEmptyOptions());
        }

        private Rect RectFromRectangle(System.Drawing.Rectangle rectangle)
        {
            return new Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        public Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

    }
}
