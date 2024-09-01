using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using raBooth.Core.Services.FrameSource;

namespace raBooth.Infrastructure.Services.FrameSource
{
    public class WebcamFrameSourceConfiguration
    {
        public int DeviceId { get; init; }
        public int FrameWidth { get; init; } = 1280;
        public int FrameHeight { get; init; } = 720;
    }

    public class WebcamFrameSource : IFrameSource
    {
        private readonly WebcamFrameSourceConfiguration _configuration;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning = false;
        private VideoCapture? _videoCapture;
        private Mat? _latestFrame;
        private ILogger<WebcamFrameSource> _logger;

        public WebcamFrameSource(WebcamFrameSourceConfiguration configuration, ILogger<WebcamFrameSource> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private async Task CreateVideoCatpure()
        {
            while (_videoCapture == null || !_videoCapture.Grab())
            {
                try
                {
                    if (_videoCapture is { IsDisposed: false })
                    {
                        _logger.LogWarning($"Disposing old VideoCapture");
                        _videoCapture.Release();
                        _videoCapture.Dispose();
                    }

                    _videoCapture = new VideoCapture(_configuration.DeviceId, VideoCaptureAPIs.DSHOW);
                    _videoCapture.Set(VideoCaptureProperties.FrameWidth, _configuration.FrameWidth);
                    _videoCapture.Set(VideoCaptureProperties.FrameHeight, _configuration.FrameHeight);
                    _logger.LogInformation($"VideoCapture created");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _logger.LogError(e, $"Exception during VideoCapture creation");
                }

                await Task.Delay(1000);
            }
        }

        /// <inheritdoc />
        public void Start()
        {
            if (_isRunning) return;
            _ = Task.Run(async () =>
                         {
                             _cancellationTokenSource = new CancellationTokenSource();
                             _isRunning = true;
                             await CreateVideoCatpure();
                             try
                             {
                                 using var cameraFrame = new Mat();
                                 while (!_cancellationTokenSource.Token.IsCancellationRequested)
                                 {
                                     if (_videoCapture.Read(cameraFrame))
                                     {
                                         var latestFrame = new Mat();
                                         cameraFrame.CopyTo(latestFrame);

                                         _latestFrame = latestFrame;
                                         LiveViewFrameAcquired?.Invoke(this, new FrameAcquiredEventArgs(latestFrame));
                                     }
                                     else
                                     {
                                         _logger.LogError($"Frame not received from VideoCapture, recreating.");
                                         await CreateVideoCatpure();
                                     }
                                 }
                             }
                             catch (Exception e)
                             {
                                 Console.WriteLine(e);
                                 throw;
                             }
                             finally
                             {
                                 _isRunning = false;
                                 _videoCapture.Release();
                                 _videoCapture.Dispose();
                             }
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
            return Task.FromResult(_latestFrame.Clone() ?? new Mat());
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