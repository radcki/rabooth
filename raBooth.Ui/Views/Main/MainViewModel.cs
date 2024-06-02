using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using raBooth.Core.Helpers;
using raBooth.Core.Model;
using raBooth.Core.Services.FrameSource;
using raBooth.Core.Services.Printing;
using raBooth.Core.Services.Storage;
using raBooth.Infrastructure.Services.Printing;
using raBooth.Ui.Configuration;
using raBooth.Ui.Model;
using raBooth.Ui.UserControls.LayoutSelection;

namespace raBooth.Ui.Views.Main
{
    public class MainViewModel : ObservableObject
    {
        private readonly IFrameSource _frameSource;
        private readonly PrintService _printService;
        private readonly ILayoutGenerationService _gridLayoutGenerationService;
        private readonly LayoutsConfiguration _layoutsConfiguration;
        private readonly ICollageStorageService _collageStorageService;


        private BitmapSource? _preview;
        private LayoutSelectionViewModel _layoutSelectionViewModel;
        private SelectableCollageLayout? _layout;
        private bool _collagePreviewVisible;
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
        private bool _printButtonVisible;
        private bool _recaptureButtonVisible;

        public MainViewModel(IFrameSource frameSource, ILayoutGenerationService gridLayoutGenerationService, LayoutsConfiguration layoutsConfiguration, PrintService printService, ICollageStorageService collageStorageService)
        {
            _frameSource = frameSource;
            _gridLayoutGenerationService = gridLayoutGenerationService;
            _layoutsConfiguration = layoutsConfiguration;
            _printService = printService;
            _collageStorageService = collageStorageService;
            LayoutSelectionViewModel = App.Services.GetRequiredService<LayoutSelectionViewModel>();
            _ = PrepareLayouts();

            LayoutSelectionViewModel.LayoutSelected += OnLayoutSelected;
            frameSource.LiveViewFrameAcquired += OnLiveViewFrameAcquired;
            frameSource.Start();
            UpdateComponentsVisibility();
            ConfigureCaptureTimer();
            ConfigureCancelTimer();
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

            _captureTimer = new CountdownTimer(TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(100));

            _captureTimer.OnCountdownTick += (_, args) => CaptureCountdownSecondsRemaining = 1 + (int)args.RemainingTime.Seconds;
            _captureTimer.OnElapsed += async (_, _) =>
                                       {
                                           var image = await _frameSource.CaptureStillImage();
                                           _layout?.CollageLayout.CaptureNextItem(image);
                                       };
        }
        private void ConfigureCancelTimer()
        {

            _cancelTimer = new CountdownTimer(TimeSpan.FromSeconds(15), TimeSpan.FromMilliseconds(100));

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
            if (Layout == default)
            {
                Preview = default;
                return;
            }

            Layout.CollageLayout.UpdateNextUncapturedItemSourceImage(e.Frame);
            var previewMat = Layout.CollageLayout.GetViewWithNextUncapturedItemPreview();

            App.Current.Dispatcher.Invoke(() => { Preview = previewMat.ToBitmapSource(); });
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

        public bool PrintButtonVisible
        {
            get => _printButtonVisible;
            set => SetProperty(ref _printButtonVisible, value);
        }

        public bool RecaptureButtonVisible
        {
            get => _recaptureButtonVisible;
            set => SetProperty(ref _recaptureButtonVisible, value);
        }

        public bool CollagePreviewVisible
        {
            get => _collagePreviewVisible;
            set => SetProperty(ref _collagePreviewVisible, value);
        }

        public LayoutSelectionViewModel LayoutSelectionViewModel
        {
            get => _layoutSelectionViewModel;
            set => SetProperty(ref _layoutSelectionViewModel, value);
        }


        private void UpdateComponentsVisibility()
        {
            if (Layout == default)
            {
                LayoutSelectionVisible = true;
                CollagePreviewVisible = false;
            }
            else
            {
                LayoutSelectionVisible = false;
                CollagePreviewVisible = true;
            }
        }

        public BitmapSource? Preview
        {
            get => _preview;
            set => SetProperty(ref _preview, value);
        }


        public IRelayCommand StopCommand => new AsyncRelayCommand(ExecuteStopCommand);
        public IRelayCommand PrintCommand => new AsyncRelayCommand(ExecutePrintCommand);
        public IRelayCommand CancelCommand => new AsyncRelayCommand(ExecuteCancelCommand);
        public IRelayCommand CaptureCommand => new AsyncRelayCommand(ExecuteCaptureCommand);
        public IRelayCommand ResetCommand => new AsyncRelayCommand(ExecuteResetCommand);
        public IRelayCommand SaveCommand => new AsyncRelayCommand(ExecuteSaveCommand);
        public IRelayCommand RecaptureCommand => new AsyncRelayCommand(ExecuteRecaptureCommand);

        private async Task ExecuteRecaptureCommand()
        {
            if (Layout == default)
            {
                return;
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

            RecaptureButtonVisible = false;
            PrintButtonVisible = false;
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
                RecaptureButtonVisible = false;
                PrintButtonVisible = false;
                _collageCaptureCancellationTokenSource = new CancellationTokenSource();

                var cancellationToken = _collageCaptureCancellationTokenSource.Token;
                var collage = Layout.CollageLayout;
                while (collage.HasUncapturedItems() && !cancellationToken.IsCancellationRequested)
                {
                    CaptureCountdownSecondsRemainingVisible = true;
                    await _captureTimer.Start(cancellationToken);
                }

                RecaptureButtonVisible = true;
                PrintButtonVisible = true;
                StartCancellationCountdown();
                await ExecuteSaveCommand();
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
            await _collageStorageService.StoreCollage(Layout.CollageLayout, progress, _cancelCancellationTokenSource.Token);
        }
    }
}