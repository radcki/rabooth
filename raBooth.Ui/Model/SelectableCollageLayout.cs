using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using raBooth.Core.Model;

namespace raBooth.Ui.Model
{
    public class SelectableCollageLayout : ObservableObject
    {
        private BitmapSource _preview;
        private Mat _previewMat;

        public SelectableCollageLayout(CollageLayout collageLayout)
        {
            CollageLayout = collageLayout;
            PreviewMat = GeneratePreview();
        }

        public CollageLayout CollageLayout { get; private set; }

        private Mat GeneratePreview()
        {
            var mask = CollageLayout.GetItemsMask();
            var previewMat = new Mat(mask.Size(), MatType.CV_8UC3, new Scalar(255, 255, 255));
            previewMat.SetTo(new Scalar(240, 240, 240), mask);
            return previewMat;
        }

        private Mat PreviewMat
        {
            get => _previewMat;
            set
            {
                _previewMat = value;
                App.Current.Dispatcher?.Invoke(() => Preview = value.ToBitmapSource());

            }
        }

        public BitmapSource Preview
        {
            get => _preview;
            private set => SetProperty(ref _preview, value);
        }
    }
}
