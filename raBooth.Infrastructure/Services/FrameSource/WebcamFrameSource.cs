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
        public int DeviceId { get; init; } = 3;
    }

    public class WebcamFrameSource : IFrameSource
    {
        private readonly WebcamFrameSourceConfiguration _configuration;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning = false;
        private VideoCapture _videoCapture;

        public WebcamFrameSource(WebcamFrameSourceConfiguration configuration)
        {
            _configuration = configuration;
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
                             try
                             {
                                 while (!_cancellationTokenSource.Token.IsCancellationRequested)
                                 {
                                     var frame = new Mat();
                                     if (_videoCapture.Read(frame))
                                     {
                                         FrameAcquired?.Invoke(this, new FrameAcquiredEventArgs(frame));
                                     }
                                 }
                             }
                             catch (Exception e)
                             {
                                 Console.WriteLine(e);
                                 throw;
                             }
                             _isRunning = false;
                             _videoCapture.Dispose();
                         });
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public event EventHandler<FrameAcquiredEventArgs>? FrameAcquired;

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            Stop();
        }

        #endregion
    }
}