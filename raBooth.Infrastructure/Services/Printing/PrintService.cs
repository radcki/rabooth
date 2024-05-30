using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace raBooth.Infrastructure.Services.Printing
{
    public class PrintServiceConfiguration
    {
        public static readonly string SectionName = "PrintingConfiguration";
        public string PrinterName { get; init; } = "Canon SELPHY CP1500";
        public int PaperKind { get; init; } = 43;
        public int MarginTop { get; init; } = 17;
        public int MarginBottom { get; init; } = 23;
        public int MarginLeft { get; init; } = 12;
        public int MarginRight { get; init; } = 13;
    }
    public class PrintService
    {
        private readonly PrintServiceConfiguration _configuration;
        public PrintService(PrintServiceConfiguration configuration)
        {
            _configuration = configuration;

        }

        private PageSettings LoadPrintPageSettings()
        {
            if (GetPrintersList().All(x => x != _configuration.PrinterName))
            {
                throw new Exception($"Printer {_configuration.PrinterName} not found");
            }
            var pageSettings = GetPrinterPageInfo(_configuration.PrinterName);
            var paper = GetPrinterPaperSizes(pageSettings).First(x => x.RawKind == _configuration.PaperKind);
            pageSettings.PaperSize = paper;
            return pageSettings;
        }

        private IEnumerable<string> GetPrintersList()
        {
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                yield return printer;
            }
        }

        private IEnumerable<PaperSize> GetPrinterPaperSizes(PageSettings pageSettings)
        {
            foreach (PaperSize paperSize in pageSettings.PrinterSettings.PaperSizes)
            {
                yield return paperSize;
            }
        }

        public static PageSettings GetPrinterPageInfo(String printerName)
        {
            PrinterSettings settings;

            if (string.IsNullOrEmpty(printerName))
            {
                foreach (var printer in PrinterSettings.InstalledPrinters)
                {
                    settings = new PrinterSettings();

                    settings.PrinterName = printer.ToString();

                    if (settings.IsDefaultPrinter)
                        return settings.DefaultPageSettings;
                }

                return null;
            }

            settings = new PrinterSettings();

            settings.PrinterName = printerName;
            
            return settings.DefaultPageSettings;
        }
        //public void PrintImage(Image img)
        public void PrintImage(Mat image)
        {
            var bitmap = BitmapFromSource(image.ToBitmapSource());

            var pageSettings = LoadPrintPageSettings();
            pageSettings.Margins = new Margins(_configuration.MarginLeft, _configuration.MarginRight, _configuration.MarginTop, _configuration.MarginBottom);
            pageSettings.Landscape = image.Cols > image.Rows;

            var printDocument = new PrintDocument();
            printDocument.DefaultPageSettings = pageSettings;
            printDocument.PrinterSettings = pageSettings.PrinterSettings;
            

            printDocument.PrintPage += OnPrintPage;
            printDocument.Print();

            void OnPrintPage(object sender, PrintPageEventArgs e)
            {
                e.Graphics.DrawImage(bitmap, e.MarginBounds);
            }

            //var pd = new PrintDialog();
            //pd.PrintTicket.PageBorderless = PageBorderless.None;

            //var img = new Image() { Source = image.ToBitmapSource() };

            //img.Source = image.ToBitmapSource();
            //img.Margin = new Thickness(0);
            //img.VerticalAlignment = VerticalAlignment.Top;
            //img.HorizontalAlignment = HorizontalAlignment.Left;
            //pd.PrintVisual(img, "booth");


        }
        private Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }
    }
}
