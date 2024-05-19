using System.IO;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using raBooth.Core.Model;
using raBooth.Core.Services.FrameSource;

namespace raBooth.Ui.Views.Main
{
    public class MainViewModel : ObservableObject
    {
        private readonly IFrameSource _frameSource;
        private static int frameW = 1024;
        private static int frameH = 528;
        private static int border = 20;

        private CollageLayout Layout = new(new[]
                                           {
                                               new CollageItem(new Size(frameW, frameH), new Point(0, 0)),
                                               new CollageItem(new Size(frameW/2-border/2, frameH), new Point(0, frameH + border)),
                                               new CollageItem(new Size(frameW/2-border/2, frameH), new Point(frameW/2 + border/2, frameH + border))
                                           });

        private BitmapSource _preview;

        public MainViewModel(IFrameSource frameSource)
        {
            _frameSource = frameSource;
            frameSource.FrameAcquired += OnFrameAcquired;
            frameSource.Start();
            Layout.CurrentViewUpdated += OnCurrentViewUpdated;
        }

        private void OnCurrentViewUpdated(object? sender, CurrentViewUpdatedEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
                                          {
                                              Preview = e.View.ToBitmapSource();
                                          });
        }

        private void OnFrameAcquired(object? sender, FrameAcquiredEventArgs e)
        {
            Layout.UpdateNextUncapturedItem(e.Frame);
        }

        public IRelayCommand CaptureCommand => new RelayCommand(ExecuteCaptureCommand);
        public IRelayCommand ResetCommand => new RelayCommand(ExecuteResetCommand);
        public IRelayCommand SaveCommand => new RelayCommand(ExecuteSaveCommand);

        private void ExecuteSaveCommand()
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            Layout.CapturedItemsView.SaveImage(Path.Combine(desktop,"booth.png"));
        }

        public BitmapSource Preview
        {
            get => _preview;
            set => SetProperty(ref _preview, value);
        }

        private void ExecuteResetCommand()
        {
            Layout.UndoLastItemCapture();
        }

        public IRelayCommand StopCommand => new RelayCommand(ExecuteStopCommand);

        private void ExecuteStopCommand()
        {
            _frameSource.Stop();
        }

        private void ExecuteCaptureCommand()
        {
            Layout.CaptureItem();
        }
    }
}