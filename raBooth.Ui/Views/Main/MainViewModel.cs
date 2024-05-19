using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using raBooth.Core.Services.FrameSource;

namespace raBooth.Ui.Views.Main
{
    public class MainViewModel : ObservableObject
    {
        private readonly IFrameSource _frameSource;

        public MainViewModel(IFrameSource frameSource)
        {
            _frameSource = frameSource;
            frameSource.FrameAcquired += OnFrameAcquired;
            frameSource.Start();
            Images.Add(Image1);
            Images.Add(Image2);
            Images.Add(Image3);
        }

        private void OnFrameAcquired(object? sender, FrameAcquiredEventArgs e)
        {
            var currentImage = Images.FirstOrDefault(x => x.IsCaptured == false);
            if (currentImage != null)
            {
                currentImage.Frame = e.Frame;
            }
        }

        public IRelayCommand CaptureCommand => new RelayCommand(ExecuteCaptureCommand);
        public IRelayCommand ResetCommand => new RelayCommand(ExecuteResetCommand);

        private void ExecuteResetCommand()
        {
            foreach (var image in Images)
            {
                image.Clear();
            }
        }

        public IRelayCommand StopCommand => new RelayCommand(ExecuteStopCommand);

        private void ExecuteStopCommand()
        {
            _frameSource.Stop();
        }

        private void ExecuteCaptureCommand()
        {
            var currentImage = Images.FirstOrDefault(x => x.IsCaptured == false);
            if (currentImage != null)
            {
                currentImage.IsCaptured = true;
            }
        }

        public CollageImage Image1 { get; set; } = new();
        public CollageImage Image2 { get; set; } = new();
        public CollageImage Image3 { get; set; } = new();
        private List<CollageImage> Images { get; set; } = new();
    }

    public class CollageImage : ObservableObject
    {
        private Mat _frame;
        private BitmapSource _preview;

        public bool IsCaptured { get; set; }

        public Mat Frame
        {
            get => _frame;
            set
            {
                SetProperty(ref _frame, value.Clone());
                App.Current?.Dispatcher?.Invoke(() => { Preview = _frame.ToBitmapSource(); });
            }
        }

        public BitmapSource Preview
        {
            get => _preview;
            set => SetProperty(ref _preview, value);
        }

        public void Clear()
        {
            App.Current?.Dispatcher?.Invoke(() =>
                                            {
                                                IsCaptured = false;
                                                Preview = default;
                                            });
        }
    }
}