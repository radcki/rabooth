using OpenCvSharp;

namespace raBooth.Core.Services.FaceDetection;

public interface IFaceDetectionService
{
    IEnumerable<DetectedFace> DetectFaces(Mat image);
}

public record DetectedFace(Rect FaceArea);

public record DetectedEyes(DetectedEye LeftEye, DetectedEye RightEye);

public record DetectedEye(Rect EyeArea)
{
    public Point GetCenter() => new(EyeArea.X + EyeArea.Width / 2, EyeArea.Y + EyeArea.Height / 2);
};