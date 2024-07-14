using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.DependencyInjection;
using raBooth.Ui.Views.Main;

namespace raBooth.Ui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WindowState _previousWindowState;
        public MainWindow()
        {
            DataContext = App.Services.GetRequiredService<MainViewModel>();
            InitializeComponent();
        }

        #region Overrides of UIElement

        /// <inheritdoc />
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                ToggleFullscreen();
            }

            base.OnKeyUp(e);
        }

        #region Overrides of Window

        /// <inheritdoc />
        protected override void OnClosing(CancelEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.Dispose();
            }
            base.OnClosing(e);
        }

        #endregion

        #endregion

        private void ToggleFullscreen()
        {
            if (WindowStyle == WindowStyle.None)
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = _previousWindowState;
                Topmost = false;
            }
            else
            {

                WindowStyle = WindowStyle.None;
                _previousWindowState = WindowState;
                WindowState = WindowState.Maximized;
                Topmost = true;
            }
        }
    }
}