using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace raBooth.Core.Services.FrameSource
{
    public interface IFrameSource : IDisposable
    {
        void Start();
        void Stop();
        void CaptureStillImage();
        event EventHandler<FrameAcquiredEventArgs> LiveViewFrameAcquired;
        event EventHandler<FrameAcquiredEventArgs> StillImageCaptured;
    }

    public record FrameAcquiredEventArgs(Mat Frame)
    {
    }
}