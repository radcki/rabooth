using System.Diagnostics;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using raBooth.Core.Helpers;
using raBooth.Core.Model;
using raBooth.Core.Services.FaceDetection;
using raBooth.Core.Services.FrameSource;
using raBooth.Core.Services.Light;
using raBooth.Core.Services.Storage;
using raBooth.Infrastructure.Services.AutoCrop;
using raBooth.Infrastructure.Services.FaceAlignment;
using raBooth.Infrastructure.Services.Printing;
using raBooth.Ui.Configuration;
using raBooth.Ui.Infrastructure;
using raBooth.Ui.Model;
using raBooth.Ui.Services.QrCode;
using raBooth.Ui.UserControls.LayoutSelection;

namespace raBooth.Ui.Views.Main
{
    public class MainViewModel : ObservableObject, IDisposable
    {
        private readonly IFrameSource _frameSource;
        private readonly PrintService _printService;
        private readonly ILayoutGenerationService _gridLayoutGenerationService;
        private readonly LayoutsConfiguration _layoutsConfiguration;
        private readonly CaptureConfiguration _captureConfiguration;
        private readonly UiConfiguration _uiConfiguration;
        private readonly ICollageStorageService _collageStorageService;
        private readonly QrCodeService _qrCodeService;
        private readonly ILightManager _lightManager;
        private readonly IFaceDetectionService _faceDetectionService;
        private readonly FaceAlignmentService _faceAlignmentService;
        private readonly AutoCropService _autoCropService;

        private BitmapSource? _preview;
        private BitmapSource? _cameraPreview;
        private BitmapSource? _collagePageUrlQrCode;
        private LayoutSelectionViewModel _layoutSelectionViewModel;
        private SelectableCollageLayout? _layout;
        private bool _collageCaptureVisible;
        private bool _layoutSelectionVisible;
        private int _captureCountdownSecondsRemaining;
        private CountdownTimer _captureTimer;
        private CountdownTimer _cancelTimer;

        private CancellationTokenSource _collageCaptureCancellationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource _cancelCancellationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource _cancelStorageCancellationTokenSource = new CancellationTokenSource();
        private bool _captureCountdownSecondsRemainingVisible;
        private int _cancelCommandCountdownSecondsRemaining;
        private bool _cancelCommandCountdownVisible;
        private bool _printButtonEnabled;
        private bool _recaptureButtonEnabled;
        private bool _collagePageQrCodeVisible;
        private bool _collagePageQrCodeSpinnerVisible;
        private bool _getReadyMessageVisible;
        private bool _collagePreviewVisible;
        private int _windowWidth;
        private int _windowHeight;
        private bool _cameraPreviewVisible;
        private bool _titleVisible;
        private List<DetectedFace> _detectedFaces = [];

        public MainViewModel(IFrameSource frameSource,
                             ILayoutGenerationService gridLayoutGenerationService,
                             LayoutsConfiguration layoutsConfiguration,
                             PrintService printService,
                             ICollageStorageService collageStorageService,
                             QrCodeService qrCodeService,
                             CaptureConfiguration captureConfiguration,
                             ILightManager lightManager,
                             IFaceDetectionService faceDetectionService,
                             BackgroundFaceDetector faceDetector,
                             UiConfiguration uiConfiguration,
                             AutoCropService autoCropService)
        {
            _frameSource = frameSource;
            _gridLayoutGenerationService = gridLayoutGenerationService;
            _layoutsConfiguration = layoutsConfiguration;
            _printService = printService;
            _collageStorageService = collageStorageService;
            _qrCodeService = qrCodeService;
            _captureConfiguration = captureConfiguration;
            _lightManager = lightManager;
            _faceDetectionService = faceDetectionService;
            FaceDetector = faceDetector;
            _uiConfiguration = uiConfiguration;
            _autoCropService = autoCropService;
            _faceAlignmentService = new FaceAlignmentService(faceDetectionService);
            LayoutSelectionViewModel = App.Services.GetRequiredService<LayoutSelectionViewModel>();


            _ = PrepareLayouts();

            LayoutSelectionViewModel.LayoutSelected += OnLayoutSelected;
            frameSource.LiveViewFrameAcquired += OnLiveViewFrameAcquired;
            frameSource.Start();
            FaceDetector.VisibleFacesCountChanged += OnVisibleFacesCountChanged;
            UpdateComponentsVisibility();
            ConfigureCaptureTimer();
            ConfigureCancelTimer();
            EnableApplicationSleepMode();
        }

        private void OnVisibleFacesCountChanged(object? sender, VisibleFacesCountChangedEventArgs e)
        {
            if (e.FaceCount > 0)
            {
                _detectedFaces = FaceDetector.VisibleFaces.ToList();
                WakeUpApplication();
            }
            else
            {
                _ = DebounceSleepMode();
            }
        }

        private async Task DebounceSleepMode()
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < _uiConfiguration.EnableSleepModeTime)
            {
                if (FaceDetector.VisibleFacesCount != 0 || Layout != default || TitleVisible)
                {
                    return;
                }

                await Task.Delay(100);
            }

            await EnableApplicationSleepMode();
        }

        public void WakeUpApplication()
        {
            if (Layout == default)
            {
                LayoutSelectionVisible = true;
                CameraPreviewVisible = true;
                TitleVisible = false;
                _ = DebounceSleepMode();
            }
        }

        private async Task EnableApplicationSleepMode()
        {
            if (Layout == default)
            {
                await ExecuteCancelCommand();
                LayoutSelectionVisible = false;
                CameraPreviewVisible = true;
                TitleVisible = true;
            }
        }

        private async void OnLayoutSelected(object? sender, CollageLayoutSelectedEventArgs e)
        {
            Task.Run(async () =>
                     {
                         Layout = e.Layout;
                         await ExecuteCaptureCommand();
                     });
        }

        private void ConfigureCaptureTimer()
        {
            _captureTimer = new CountdownTimer(_captureConfiguration.CaptureCountdownLength, TimeSpan.FromMilliseconds(100));

            _captureTimer.OnCountdownTick += (_, args) =>
                                             {
                                                 var counterBefore = CaptureCountdownSecondsRemaining;
                                                 var counterAfter = 1 + (int)args.RemainingTime.Seconds;
                                                 if (counterBefore != counterAfter)
                                                 {
                                                     CaptureCountdownSecondsRemaining = counterAfter;
                                                 }
                                             };
            _captureTimer.OnElapsed += async (_, _) =>
                                       {
                                           var image = await _frameSource.CaptureStillImage();
                                           var facesCrop = _autoCropService.GetCurrentCrop(image);
                                           _layout?.CollageLayout.CaptureNextItem(image, facesCrop);
                                       };
        }

        private void ConfigureCancelTimer()
        {
            _cancelTimer = new CountdownTimer(_captureConfiguration.ExitCountdownLength, TimeSpan.FromMilliseconds(100));

            _cancelTimer.OnCountdownTick += (_, args) => CancelCommandCountdownSecondsRemaining = 1 + (int)args.RemainingTime.Seconds;
            _cancelTimer.OnElapsed += (_, _) =>
                                      {
                                          if (CancelCommand.CanExecute(null))
                                          {
                                              CancelCommand.Execute(null);
                                          }

                                          CancelCommandCountdownVisible = false;
                                      };
        }

        private Task PrepareLayouts()
        {
            return Task.Run(() =>
                            {
                                foreach (var layoutDefinition in _layoutsConfiguration.LayoutDefinitions)
                                {
                                    LayoutSelectionViewModel.AddLayout(_gridLayoutGenerationService.GenerateLayout(layoutDefinition));
                                }
                            });
        }


        private void OnLiveViewFrameAcquired(object? sender, FrameAcquiredEventArgs e)
        {
            _ = UpdateCameraPreview(e.Frame);

            if (Layout == default)
            {
                Preview = default;
                return;
            }

            if (Layout != default)
            {
                var facesCrop = _autoCropService.GetCurrentCrop(e.Frame);
                Layout?.CollageLayout.UpdateNextUncapturedItemSourceImage(e.Frame, facesCrop);
                var previewMat = Layout.CollageLayout.GetViewWithNextUncapturedItemPreview();

                App.Current?.Dispatcher.Invoke(() => { Preview = previewMat.ToBitmapSource(); });
            }
        }

        private Task UpdateCameraPreview(Mat frame)
        {
            var cameraPreviewMat = ImageProcessing.ResizeToCoverToNew(frame, new Size(WindowWidth, WindowHeight));
            App.Current?.Dispatcher.Invoke(() => { CameraPreview = cameraPreviewMat.ToBitmapSource(); });
            FaceDetector.SubmitNewFrame(frame);
            return Task.CompletedTask;
        }

        public SelectableCollageLayout? Layout
        {
            get => _layout;
            set
            {
                if (SetProperty(ref _layout, value))
                {
                    OnPropertyChanged(nameof(CaptureCommand));
                    OnPropertyChanged(nameof(ResetCommand));
                    OnPropertyChanged(nameof(SaveCommand));
                    OnPropertyChanged(nameof(CameraPreviewVisible));
                }

                UpdateComponentsVisibility();
            }
        }

        public int CaptureCountdownSecondsRemaining
        {
            get => _captureCountdownSecondsRemaining;
            set => SetProperty(ref _captureCountdownSecondsRemaining, value);
        }

        public int CancelCommandCountdownSecondsRemaining
        {
            get => _cancelCommandCountdownSecondsRemaining;
            set => SetProperty(ref _cancelCommandCountdownSecondsRemaining, value);
        }


        public bool LayoutSelectionVisible
        {
            get => _layoutSelectionVisible;
            set => SetProperty(ref _layoutSelectionVisible, value);
        }

        public bool CaptureCountdownSecondsRemainingVisible
        {
            get => _captureCountdownSecondsRemainingVisible;
            set => SetProperty(ref _captureCountdownSecondsRemainingVisible, value);
        }

        public bool CancelCommandCountdownVisible
        {
            get => _cancelCommandCountdownVisible;
            set => SetProperty(ref _cancelCommandCountdownVisible, value);
        }

        public bool PrintButtonEnabled
        {
            get => _printButtonEnabled;
            set => SetProperty(ref _printButtonEnabled, value);
        }

        public bool CollagePageQrCodeVisible
        {
            get => _collagePageQrCodeVisible;
            set => SetProperty(ref _collagePageQrCodeVisible, value);
        }

        public bool CollagePageQrCodeSpinnerVisible
        {
            get => _collagePageQrCodeSpinnerVisible;
            set => SetProperty(ref _collagePageQrCodeSpinnerVisible, value);
        }

        public bool GetReadyMessageVisible
        {
            get => _getReadyMessageVisible;
            set => SetProperty(ref _getReadyMessageVisible, value);
        }

        public bool RecaptureButtonEnabled
        {
            get => _recaptureButtonEnabled;
            set => SetProperty(ref _recaptureButtonEnabled, value);
        }

        public bool CollageCaptureVisible
        {
            get => _collageCaptureVisible;
            set => SetProperty(ref _collageCaptureVisible, value);
        }

        public bool CollagePreviewVisible
        {
            get => _collagePreviewVisible;
            set => SetProperty(ref _collagePreviewVisible, value);
        }

        public bool TitleVisible
        {
            get => _titleVisible;
            set => SetProperty(ref _titleVisible, value);
        }

        public int WindowWidth
        {
            get => _windowWidth;
            set => SetProperty(ref _windowWidth, value);
        }

        public int WindowHeight
        {
            get => _windowHeight;
            set => SetProperty(ref _windowHeight, value);
        }

        public LayoutSelectionViewModel LayoutSelectionViewModel
        {
            get => _layoutSelectionViewModel;
            set => SetProperty(ref _layoutSelectionViewModel, value);
        }

        public BitmapSource CollagePageUrlQrCode
        {
            get => _collagePageUrlQrCode;
            set => SetProperty(ref _collagePageUrlQrCode, value);
        }


        public BitmapSource? Preview
        {
            get => _preview;
            set => SetProperty(ref _preview, value);
        }

        public BitmapSource? CameraPreview
        {
            get => _cameraPreview;
            set => SetProperty(ref _cameraPreview, value);
        }

        public bool CameraPreviewVisible
        {
            get => _cameraPreviewVisible;
            set => SetProperty(ref _cameraPreviewVisible, value);
        }


        public IRelayCommand StopCommand => new AsyncRelayCommand(ExecuteStopCommand);
        public IRelayCommand PrintCommand => new AsyncRelayCommand(ExecutePrintCommand);
        public IRelayCommand CancelCommand => new AsyncRelayCommand(ExecuteCancelCommand);
        public IRelayCommand CaptureCommand => new AsyncRelayCommand(ExecuteCaptureCommand);
        public IRelayCommand ResetCommand => new AsyncRelayCommand(ExecuteResetCommand);
        public IRelayCommand SaveCommand => new AsyncRelayCommand(ExecuteSaveCommand);
        public IRelayCommand WakUpCommand => new AsyncRelayCommand(WakeUpCommandExecute);
        public IRelayCommand RecaptureCommand => new AsyncRelayCommand(ExecuteRecaptureCommand);
        public BackgroundFaceDetector FaceDetector { get; init; }

        private Task WakeUpCommandExecute()
        {
            return Task.Run(WakeUpApplication);
        }

        private void UpdateComponentsVisibility()
        {
            if (Layout == default)
            {
                LayoutSelectionVisible = true;
                CameraPreviewVisible = true;
                CollageCaptureVisible = false;
                CollagePageQrCodeSpinnerVisible = false;
                CollagePageQrCodeVisible = false;
                CollagePageUrlQrCode = default;
                _ = DebounceSleepMode();
            }
            else
            {
                LayoutSelectionVisible = false;
                CollageCaptureVisible = true;
                CameraPreviewVisible = false;
            }
        }

        private async Task ExecuteRecaptureCommand()
        {
            if (Layout == default)
            {
                return;
            }

            if (_cancelTimer.IsInProgress)
            {
                _cancelTimer.Cancel();
                CancelCommandCountdownVisible = false;
            }

            Layout.CollageLayout.Clear();

            if (_collageCaptureCancellationTokenSource.Token.CanBeCanceled)
            {
                await _collageCaptureCancellationTokenSource.CancelAsync();
            }

            _ = ExecuteCaptureCommand();
        }

        private async Task ExecuteCancelCommand()
        {
            _cancelTimer.Cancel();

            if (Layout == default)
            {
                return;
            }

            if (_collageCaptureCancellationTokenSource.Token.CanBeCanceled)
            {
                await _collageCaptureCancellationTokenSource.CancelAsync();
            }

            if (_captureTimer.IsInProgress)
            {
                _captureTimer.Cancel();
            }

            Layout.CollageLayout.Clear();
            Layout = default;

            RecaptureButtonEnabled = false;
            PrintButtonEnabled = false;
            CancelCommandCountdownVisible = false;
        }

        private async Task ExecuteStopCommand()
        {
            _frameSource.Stop();
        }

        private async Task ExecuteCaptureCommand()
        {
            if (Layout == default)
            {
                return;
            }

            try
            {
                CollagePageQrCodeSpinnerVisible = false;
                CollagePageQrCodeVisible = false;
                CollagePageUrlQrCode = default;
                RecaptureButtonEnabled = false;
                PrintButtonEnabled = false;
                GetReadyMessageVisible = true;
                CollagePreviewVisible = false;
                await _lightManager.SetLightsToHighBrightness();
                await Task.Delay(_captureConfiguration.GetReadyMessageDisplayTime);
                GetReadyMessageVisible = false;
                CollagePreviewVisible = true;


                _collageCaptureCancellationTokenSource = new CancellationTokenSource();

                var cancellationToken = _collageCaptureCancellationTokenSource.Token;
                if (Layout == default)
                {
                    return;
                }

                var collage = Layout.CollageLayout;
                while (collage.HasUncapturedItems() && !cancellationToken.IsCancellationRequested)
                {
                    CaptureCountdownSecondsRemainingVisible = true;
                    await _captureTimer.Start(cancellationToken);
                }

                RecaptureButtonEnabled = true;
                PrintButtonEnabled = true;
                StartCancellationCountdown();
                await ExecuteSaveCommand();
                //await _lightManager.SetLightsToLowBrightness();
            }
            finally
            {
                CaptureCountdownSecondsRemainingVisible = false;
            }
        }

        private void StartCancellationCountdown()
        {
            CancelCommandCountdownSecondsRemaining = _cancelTimer.Length.Seconds;
            _cancelCancellationTokenSource = new CancellationTokenSource();
            _cancelTimer.Start(_cancelCancellationTokenSource.Token);
            CancelCommandCountdownVisible = true;
        }

        private async Task ExecutePrintCommand()
        {
            _printService.PrintImage(Layout.CollageLayout.GetViewWithNextUncapturedItemPreview());
        }

        private async Task ExecuteResetCommand()
        {
            if (Layout == default)
            {
                return;
            }

            Layout.CollageLayout.UndoLastItemCapture();
        }

        private async Task ExecuteSaveCommand()
        {
            if (Layout == default)
            {
                return;
            }

            _cancelStorageCancellationTokenSource = new CancellationTokenSource();
            var progress = new Progress<StoreCollageProgress>();
            progress.ProgressChanged += OnStoreCollageProgressChanged;
            CollagePageQrCodeSpinnerVisible = true;
            _ = Task.Run(async () =>
                         {
                             await _collageStorageService.StoreCollage(Layout.CollageLayout, progress, _cancelCancellationTokenSource.Token);
                             progress.ProgressChanged -= OnStoreCollageProgressChanged;
                         });
        }

        private void OnStoreCollageProgressChanged(object? sender, StoreCollageProgress e)
        {
            if (!string.IsNullOrEmpty(e.CollagePageUrl))
            {
                App.Current?.Dispatcher.Invoke(() =>
                                               {
                                                   CollagePageUrlQrCode = _qrCodeService.GetQrCodeBitmapForUrl(e.CollagePageUrl).ToBitmapSource();
                                                   CollagePageQrCodeSpinnerVisible = false;
                                                   CollagePageQrCodeVisible = true;
                                               });
            }
        }

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            LayoutSelectionViewModel.LayoutSelected -= OnLayoutSelected;
            _frameSource.LiveViewFrameAcquired -= OnLiveViewFrameAcquired;
            _frameSource.Dispose();
            _collageCaptureCancellationTokenSource.Dispose();
            _cancelCancellationTokenSource.Dispose();
            _cancelStorageCancellationTokenSource.Dispose();
        }

        #endregion
    }
}