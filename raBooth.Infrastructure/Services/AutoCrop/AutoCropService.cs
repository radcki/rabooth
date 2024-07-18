using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using OpenCvSharp;
using raBooth.Core.Services.FaceDetection;
using raBooth.Ui.Infrastructure;

namespace raBooth.Infrastructure.Services.AutoCrop
{
    public class AutoCropConfiguration
    {
        public const string SectionName = nameof(AutoCropConfiguration);
        public bool Enabled { get; set; } = false;
        public int Width { get; init; } = 3200;
        public int Height { get; init; } = 1800;
        public int CropAdjustStep { get; init; } = 10;
        public double CropTargetUpdateFrequency { get; init; } = 0.5d;
    }

    public class AutoCropService
    {
        private readonly AutoCropConfiguration _configuration;
        private readonly BackgroundFaceDetector _faceDetector;
        private Rect? _currentCrop;
        private Rect? _destinationCrop;
        private readonly System.Timers.Timer _destinationUpdateTimer;
        private readonly System.Timers.Timer _currentCropUpdateTimer;
        private Mat? _frame = null;
        private List<DetectedFace> _detectedFaces = [];

        public AutoCropService(AutoCropConfiguration configuration, BackgroundFaceDetector faceDetector)
        {
            _configuration = configuration;
            _faceDetector = faceDetector;
            if (_configuration.Enabled)
            {
                _destinationUpdateTimer = new System.Timers.Timer(TimeSpan.FromSeconds(_configuration.CropTargetUpdateFrequency));
                _destinationUpdateTimer.Elapsed += DestinationUpdateTimer_OnElapsed;
                _destinationUpdateTimer.Start();
                _currentCropUpdateTimer = new System.Timers.Timer(TimeSpan.FromSeconds(1 / 60D));
                _currentCropUpdateTimer.Elapsed += CurrentCropUpdate_OnElapsed;
                _currentCropUpdateTimer.Start();
                _faceDetector.VisibleFacesChanged += OnVisibleFacesChanged;
            }
        }

        private void CurrentCropUpdate_OnElapsed(object? sender, ElapsedEventArgs e)
        {
            UpdateCurrentCrop();
        }

        private void OnVisibleFacesChanged(object? sender, VisibleFacesChangedEventArgs e)
        {
            _frame = e.Frame;
            _detectedFaces = e.VisibleFaces.ToList();
        }

        private void DestinationUpdateTimer_OnElapsed(object? sender, ElapsedEventArgs e)
        {
            if (_frame != null)
            {
                UpdateDestinationCrop(_frame, _detectedFaces);
            }
        }

        private void UpdateDestinationCrop(Mat frame, List<DetectedFace> detectedFaces)
        {
            detectedFaces = detectedFaces.Where(x => x != default).ToList();
            //var width = frame.Width;
            var width = Math.Min(_configuration.Width, frame.Width);
            var height = Math.Min(_configuration.Height, frame.Height);
            Rect cropRect = new Rect(0, 0, frame.Cols, frame.Rows);
            if (detectedFaces.Count == 0)
            {
                cropRect = GetRectFromCenter(frame.Width / 2, frame.Height / 2, width, height);
            }
            else
            {
                var minFacesX = detectedFaces.Min(x => x.FaceArea.Left);
                var maxFacesX = detectedFaces.Max(x => x.FaceArea.Right);
                var minFacesY = detectedFaces.Min(x => x.FaceArea.Top);
                var maxFacesY = detectedFaces.Max(x => x.FaceArea.Bottom);
                var facesXDistance = (maxFacesX - minFacesX) * 1.4;
                var facesYDistance = (maxFacesY - minFacesY) * 1.4;
                //var facesCenter = new Point(frame.Width / 2, +(maxFacesY + minFacesY) / 2);
                var facesCenter = new Point((maxFacesX + minFacesX) / 2, +(maxFacesY + minFacesY) / 2);
                if (facesXDistance > _configuration.Width)
                {
                    width = (int)facesXDistance;
                }

                if (facesYDistance > _configuration.Height)
                {
                    height = (int)facesYDistance;
                }

                if (width > frame.Width)
                {
                    width = frame.Width;
                    facesCenter = new Point(frame.Width / 2, facesCenter.Y);
                }

                if (height > frame.Height)
                {
                    height = frame.Height;
                    facesCenter = new Point(facesCenter.X, frame.Height / 2);
                }

                cropRect = GetRectFromCenter(facesCenter.X, facesCenter.Y, width, height);
            }

            cropRect = LimitToBounds(cropRect, frame);
            _destinationCrop = cropRect;
        }

        private void UpdateCurrentCrop()
        {
            if (_destinationCrop == null)
            {
                return;
            }

            _currentCrop ??= _destinationCrop;

            var step = _configuration.CropAdjustStep;
            var current = _currentCrop.Value;
            var destination = _destinationCrop.Value;
            var xDiff = destination.X - current.X;
            var yDiff = destination.Y - current.Y;
            var wDiff = destination.Width - current.Width;
            var hDiff = destination.Height - current.Height;
            var xOffset = xDiff > step ? step : xDiff < -step ? -step : 0;
            var yOffset = yDiff > step ? step : yDiff < -step ? -step : 0;
            var wOffset = wDiff > step ? step : wDiff < -step ? -step : 0;
            var hOffset = hDiff > step ? step : hDiff < -step ? -step : 0;
            var crop = new Rect(current.X + xOffset, current.Y + yOffset, current.Width + wOffset, current.Height + hOffset);
            _currentCrop = _frame != null ? LimitToBounds(crop, _frame) : crop;
        }

        public Rect GetCurrentCrop(Mat frame)
        {
            if (!_configuration.Enabled)
            {
                return new Rect(0, 0, frame.Cols, frame.Rows);
            }

            if (_destinationCrop == null)
            {
                UpdateDestinationCrop(frame, []);
            }

            if (_currentCrop == null)
            {
                UpdateCurrentCrop();
            }

            return _currentCrop.Value;
        }

        public Mat GetFrameCroppedToFaces(Mat frame)
        {
            if (!_configuration.Enabled)
            {
                return frame.Clone();
            }
            if (_destinationCrop == null)
            {
                UpdateDestinationCrop(frame, []);
            }

            if (_currentCrop == null)
            {
                UpdateCurrentCrop();
            }

            return frame[_currentCrop.Value];
        }

        private Rect GetRectFromCenter(int x, int y, int width, int height)
        {
            return new Rect(x - (width / 2), y - (height / 2), width, height);
        }

        private Rect LimitToBounds(Rect rect, Mat image)
        {
            if (rect.Width > image.Width)
            {
                throw new Exception($"Rect width ({rect.Width}) is larger than mat width ({image.Width})");
            }

            if (rect.Height > image.Height)
            {
                throw new Exception($"Rect height ({rect.Height}) is larger than mat height ({image.Height})");
            }

            var yTopOutOfBoundsPixels = Math.Abs(Math.Min(0, rect.Top));
            var yBottomOutOfBoundsPixels = Math.Abs(Math.Min(0, image.Rows - rect.Bottom));

            var xLeftOutOfBoundsPixels = Math.Abs(Math.Min(0, rect.Left));
            var xRighOutOfBoundsPixels = Math.Abs(Math.Min(0, image.Cols - rect.Right));

            var bounded = new Rect(rect.X + xLeftOutOfBoundsPixels - xRighOutOfBoundsPixels, rect.Y + yTopOutOfBoundsPixels - yBottomOutOfBoundsPixels, rect.Width, rect.Height);
            return bounded;
        }
    }
}