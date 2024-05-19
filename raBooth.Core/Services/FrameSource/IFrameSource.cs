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
        event EventHandler<FrameAcquiredEventArgs> FrameAcquired;
    }

    public record FrameAcquiredEventArgs(Mat Frame)
    {
    }
}