using System.IO;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using raBooth.Core.Model;
using raBooth.Core.Services.FrameSource;
using raBooth.Ui.Configuration;

namespace raBooth.Ui.Views.Main
{
    public class MainViewModel : ObservableObject
    {
        private readonly IFrameSource _frameSource;
        private readonly ILayoutItemsGenerationService _gridLayoutItemsGenerationService;

        private LayoutsConfiguration _layoutsConfiguration;
        private CollageLayout _layout { get; set; }

        private BitmapSource _preview;

        public MainViewModel(IFrameSource frameSource, ILayoutItemsGenerationService gridLayoutItemsGenerationService, LayoutsConfiguration layoutsConfiguration)
        {
            _frameSource = frameSource;
            _gridLayoutItemsGenerationService = gridLayoutItemsGenerationService;
            _layoutsConfiguration = layoutsConfiguration;
            _layout = PrepareLayout(layoutsConfiguration.LayoutDefinitions.FirstOrDefault());

            frameSource.FrameAcquired += OnFrameAcquired;
            frameSource.Start();

        }

        private CollageLayout PrepareLayout(CollageLayoutDefinition definition)
        {
            var layout = new CollageLayout(definition);
            foreach (var layoutItem in _gridLayoutItemsGenerationService.GenerateItems(definition))
            {
                layout.AddItem(layoutItem);
            }

            return layout;
        }

        private void OnFrameAcquired(object? sender, FrameAcquiredEventArgs e)
        {
            _layout.UpdateNextUncapturedItemSourceImage(e.Frame);
            var previewMat = _layout.GetViewWithNextUncapturedItemPreview();
            
            App.Current.Dispatcher.Invoke(() =>
                                          {
                                              Preview = previewMat.ToBitmapSource();
                                          });

        }

        public IRelayCommand CaptureCommand => new RelayCommand(ExecuteCaptureCommand);
        public IRelayCommand ResetCommand => new RelayCommand(ExecuteResetCommand);
        public IRelayCommand SaveCommand => new RelayCommand(ExecuteSaveCommand);

        private void ExecuteSaveCommand()
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            _layout.GetViewWithNextUncapturedItemPreview().SaveImage(Path.Combine(desktop, "booth.png"));
        }

        public BitmapSource Preview
        {
            get => _preview;
            set => SetProperty(ref _preview, value);
        }

        private void ExecuteResetCommand()
        {
            _layout.UndoLastItemCapture();
        }

        public IRelayCommand StopCommand => new RelayCommand(ExecuteStopCommand);

        private void ExecuteStopCommand()
        {
            _frameSource.Stop();
        }

        private void ExecuteCaptureCommand()
        {
            _layout.CaptureNextItem();
        }
    }
}