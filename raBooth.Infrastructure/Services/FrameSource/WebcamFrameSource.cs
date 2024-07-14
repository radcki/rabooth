using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using raBooth.Core.Services.FrameSource;

namespace raBooth.Infrastructure.Services.FrameSource
{
    public class WebcamFrameSourceConfiguration
    {
        public int DeviceId { get; init; }
        public int FrameWidth { get; init; } = 1280;
        public int FrameHeight { get; init; } = 720;
        public int CropTop { get; init; } = 0;
        public int CropBottom { get; init; } = 0;
        public int CropLeft { get; init; } = 0;
        public int CropRight { get; init; } = 0;
    }

    public class WebcamFrameSource : IFrameSource
    {
        private readonly WebcamFrameSourceConfiguration _configuration;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning = false;
        private VideoCapture? _videoCapture;
        private Mat? _latestFrame;

        public WebcamFrameSource(WebcamFrameSourceConfiguration configuration)
        {
            _configuration = configuration;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <inheritdoc />
        public void Start()
        {
            if (_isRunning) return;
            _ = Task.Run(() =>
                         {
                             _cancellationTokenSource = new CancellationTokenSource();
                             _isRunning = true;
                             _videoCapture = new VideoCapture(_configuration.DeviceId);
                             _videoCapture.Set(VideoCaptureProperties.FrameWidth, _configuration.FrameWidth);
                             _videoCapture.Set(VideoCaptureProperties.FrameHeight, _configuration.FrameHeight);
                             try
                             {
                                 while (!_cancellationTokenSource.Token.IsCancellationRequested)
                                 {
                                     using var frame = new Mat();
                                     if (_videoCapture.Read(frame))
                                     {
                                         var croppedFrame = new Mat();
                                         long imageSize = frame.Rows * frame.Cols * 4;
                                         GC.AddMemoryPressure(imageSize);
                                         frame[_configuration.CropTop, frame.Height - _configuration.CropBottom, _configuration.CropLeft, frame.Width - _configuration.CropRight].CopyTo(croppedFrame);
 
                                         frame.CopyTo(croppedFrame);
                                         _latestFrame = croppedFrame;
                                         _ = Task.Run(() => { LiveViewFrameAcquired?.Invoke(this, new FrameAcquiredEventArgs(croppedFrame)); });
                                     }
                                 }
                             }
                             catch (Exception e)
                             {
                                 Console.WriteLine(e);
                                 throw;
                             }

                             _isRunning = false;
                             _videoCapture.Release();
                             _videoCapture.Dispose();
                         });
        }

        public void Stop()
        {
            if (!_isRunning)
            {
                return;
            }

            if (_cancellationTokenSource.Token.CanBeCanceled)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        public Task<Mat> CaptureStillImage()
        {
            return Task.FromResult(_latestFrame ?? new Mat());
        }

        public event EventHandler<FrameAcquiredEventArgs>? LiveViewFrameAcquired;


        #region IDisposable

        public void Dispose()
        {
            Stop();
        }

        #endregion
    }
}