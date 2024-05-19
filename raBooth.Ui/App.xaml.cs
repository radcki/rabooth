using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using raBooth.Core.Services.FrameSource;
using raBooth.Infrastructure.Services.FrameSource;
using raBooth.Ui.Views.Main;

namespace raBooth.Ui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; set; }


        protected override void OnStartup(StartupEventArgs e)
        {
            var builder = new ConfigurationBuilder()
                         .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection, configuration);

            Services = serviceCollection.BuildServiceProvider();

        }

        private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<WebcamFrameSourceConfiguration>(configuration.GetSection(nameof(WebcamFrameSourceConfiguration)));

            services.AddTransient<WebcamFrameSourceConfiguration>();
            services.AddTransient(typeof(MainWindow));
            services.AddSingleton<IFrameSource, WebcamFrameSource>();
            services.AddSingleton<MainViewModel>();
        }
    }

}
