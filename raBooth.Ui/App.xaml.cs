using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using raBooth.Core.Model;
using raBooth.Core.Services.FrameSource;
using raBooth.Infrastructure.Services.FrameSource;
using raBooth.Ui.Configuration;
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
            services.AddSingleton(provider => provider.GetRequiredService<IOptions<WebcamFrameSourceConfiguration>>().Value);
            services.Configure<LayoutsConfiguration>(configuration.GetSection(nameof(LayoutsConfiguration)));
            services.AddSingleton(provider => provider.GetRequiredService<IOptions<LayoutsConfiguration>>().Value);

            services.AddTransient<ILayoutItemsGenerationService, GridLayoutItemsGenerationService>();
            services.AddSingleton<IFrameSource, WebcamFrameSource>();
            services.AddTransient<MainWindow>();
            services.AddSingleton<MainViewModel>();
        }
    }

}
