using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using raBooth.Core.Model;
using raBooth.Core.Services.FrameSource;
using raBooth.Ui.Configuration;
using raBooth.Ui.Model;
using raBooth.Ui.UserControls.LayoutSelection;

namespace raBooth.Ui.Views.Main
{
    public class MainViewModel : ObservableObject
    {
        private readonly IFrameSource _frameSource;
        private readonly ILayoutGenerationService _gridLayoutGenerationService;

        private readonly LayoutsConfiguration _layoutsConfiguration;


        private BitmapSource? _preview;
        private LayoutSelectionViewModel _layoutSelectionViewModel;
        private SelectableCollageLayout? _layout;
        private bool _collagePreviewVisible;
        private bool _layoutSelectionVisible;

        public MainViewModel(IFrameSource frameSource, ILayoutGenerationService gridLayoutGenerationService, LayoutsConfiguration layoutsConfiguration)
        {
            _frameSource = frameSource;
            _gridLayoutGenerationService = gridLayoutGenerationService;
            _layoutsConfiguration = layoutsConfiguration;
            LayoutSelectionViewModel = App.Services.GetRequiredService<LayoutSelectionViewModel>();
            _ = PrepareLayouts();

            LayoutSelectionViewModel.LayoutSelected += OnLayoutSelected;
            frameSource.FrameAcquired += OnFrameAcquired;
            frameSource.Start();
            UpdateComponentsVisibility();
        }

        private void OnLayoutSelected(object? sender, CollageLayoutSelectedEventArgs e)
        {
            Layout = e.Layout;
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


        private void OnFrameAcquired(object? sender, FrameAcquiredEventArgs e)
        {
            if (Layout == default)
            {
                Preview = default;
                return;
            }
            Layout.CollageLayout.UpdateNextUncapturedItemSourceImage(e.Frame);
            var previewMat = Layout.CollageLayout.GetViewWithNextUncapturedItemPreview();

            App.Current.Dispatcher.Invoke(() =>
                                          {
                                              Preview = previewMat.ToBitmapSource();
                                          });

        }

        public IRelayCommand CaptureCommand => new RelayCommand(ExecuteCaptureCommand, () => Layout != default);
        public IRelayCommand ResetCommand => new RelayCommand(ExecuteResetCommand, () => Layout != default);
        public IRelayCommand SaveCommand => new RelayCommand(ExecuteSaveCommand, () => Layout != default);
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

        public bool LayoutSelectionVisible
        {
            get => _layoutSelectionVisible;
            set => SetProperty(ref _layoutSelectionVisible, value);
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

        private void ExecuteSaveCommand()
        {
            if (Layout == default)
            {
                return;
            }
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            Layout.CollageLayout.GetViewWithNextUncapturedItemPreview().SaveImage(Path.Combine(desktop, "booth.png"));
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

        private void ExecuteResetCommand()
        {
            if (Layout == default)
            {
                return;
            }
            Layout.CollageLayout.UndoLastItemCapture();
        }

        public IRelayCommand StopCommand => new RelayCommand(ExecuteStopCommand);
        public IRelayCommand CancelCommand => new AsyncRelayCommand(ExecuteCancelCommand);

        private async Task ExecuteCancelCommand()
        {
            if (Layout == default)
            {
                return;
            }
            Layout.CollageLayout.Clear();
            Layout = default;
        }

        private void ExecuteStopCommand()
        {
            _frameSource.Stop();
        }

        private void ExecuteCaptureCommand()
        {
            if (Layout == default)
            {
                return;
            }
            Layout.CollageLayout.CaptureNextItem();
        }
    }
}