using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace raBooth.Ui.Services.QrCode
{
    public class QrCodeService
    {
        public Bitmap GetQrCodeBitmapForUrl(string url)
        {
            var codeGenerator = new QRCodeGenerator();
            var codeInfo = codeGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(codeInfo);
            var bitmap = qrCode.GetGraphic(60);

            return bitmap;
        }
    }
}
