using OpenCvSharp;
using raBooth.Core.Model;
using raBooth.Core.Services.FaceDetection;

namespace raBooth.Ui.Infrastructure
{
    public class BackgroundFaceDetector
    {
        private readonly IFaceDetectionService _faceDetectionService;
        private readonly DebouncedValue<int> _visibleFacesCount;
        private readonly List<DetectedFace> _visibleFaces = [];
        private bool _isFaceDetectionInProgress;

        public BackgroundFaceDetector(IFaceDetectionService faceDetectionService)
        {
            _faceDetectionService = faceDetectionService;
            _visibleFacesCount = new DebouncedValue<int>();
            _visibleFacesCount.ValueChanged += (sender, i) => VisibleFacesCountChanged?.Invoke(this, new(i));
        }


        public void SubmitNewFrame(Mat frame)
        {
            if (!_isFaceDetectionInProgress)
            {
                _ = DetectFaces(frame);
            }
        }

        private Task DetectFaces(Mat frame)
        {
            return Task.Run(() =>
                            {
                                _isFaceDetectionInProgress = true;
                                var faces = _faceDetectionService.DetectFaces(frame).ToList();
                                _visibleFacesCount.Update(faces.Count);

                                var toAdd = faces.Where(newFace => _visibleFaces.All(oldFace => !oldFace.IsSameFace(newFace))).ToList();
                                var toRemove = _visibleFaces.Where(oldFace => faces.All(newFace => !newFace.IsSameFace(oldFace))).ToList();
                                foreach (var detectedFace in toRemove)
                                {
                                    _visibleFaces.Remove(detectedFace);
                                }

                                _visibleFaces.AddRange(toAdd);
                                if (toAdd.Count > 0 || toRemove.Count > 0)
                                {
                                    VisibleFacesChanged?.Invoke(this, new(_visibleFaces.ToList(), frame));
                                }

                                _isFaceDetectionInProgress = false;
                            });
        }

        public int VisibleFacesCount => _visibleFacesCount.Value;
        public IEnumerable<DetectedFace> VisibleFaces => _visibleFaces.AsEnumerable();
        public event EventHandler<VisibleFacesChangedEventArgs> VisibleFacesChanged;
        public event EventHandler<VisibleFacesCountChangedEventArgs> VisibleFacesCountChanged;
    }

    public record VisibleFacesCountChangedEventArgs(int FaceCount);

    public record VisibleFacesChangedEventArgs(List<DetectedFace> VisibleFaces, Mat Frame);
}