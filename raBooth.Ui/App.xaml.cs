using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;
using raBooth.Core.Model;
using raBooth.Core.Services.FaceDetection;
using raBooth.Core.Services.FrameSource;
using raBooth.Core.Services.Printing;
using raBooth.Core.Services.Storage;
using raBooth.Core.Services.Web;
using raBooth.Infrastructure.Services.AutoCrop;
using raBooth.Infrastructure.Services.FaceDetection;
using raBooth.Infrastructure.Services.FrameSource;
using raBooth.Infrastructure.Services.Light;
using raBooth.Infrastructure.Services.Printing;
using raBooth.Infrastructure.Services.Storage;
using raBooth.Infrastructure.Services.Web;
using raBooth.Ui.Configuration;
using raBooth.Ui.Infrastructure;
using raBooth.Ui.Services.QrCode;
using raBooth.Ui.UserControls.LayoutSelection;
using raBooth.Ui.Views.Main;
using ILightManager = raBooth.Core.Services.Light.ILightManager;

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
                         .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                         .AddUserSecrets(GetType().Assembly, true);

            var configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection, configuration);

            Services = serviceCollection.BuildServiceProvider();
            SetCulture();
        }

        private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging();


            services.Configure<WebcamFrameSourceConfiguration>(configuration.GetSection(nameof(WebcamFrameSourceConfiguration)));
            services.AddSingleton(provider => provider.GetRequiredService<IOptions<WebcamFrameSourceConfiguration>>().Value);

            services.Configure<LayoutsConfiguration>(configuration.GetSection(nameof(LayoutsConfiguration)));
            services.AddSingleton(provider => provider.GetRequiredService<IOptions<LayoutsConfiguration>>().Value);

            services.Configure<CaptureConfiguration>(configuration.GetSection(CaptureConfiguration.SectionName));
            services.AddSingleton(provider => provider.GetRequiredService<IOptions<CaptureConfiguration>>().Value);

            services.Configure<PrintServiceConfiguration>(configuration.GetSection(PrintServiceConfiguration.SectionName));
            services.AddSingleton(provider => provider.GetRequiredService<IOptions<PrintServiceConfiguration>>().Value);

            services.Configure<WebHostApiClientConfiguration>(configuration.GetSection(WebHostApiClientConfiguration.SectionName));
            services.AddSingleton(provider => provider.GetRequiredService<IOptions<WebHostApiClientConfiguration>>().Value);

            services.Configure<UiConfiguration>(configuration.GetSection(UiConfiguration.SectionName));
            services.AddSingleton(provider => provider.GetRequiredService<IOptions<UiConfiguration>>().Value);

            services.Configure<LightsConfiguration>(configuration.GetSection(LightsConfiguration.SectionName));
            services.AddSingleton(provider => provider.GetRequiredService<IOptions<LightsConfiguration>>().Value);

            services.Configure<AutoCropConfiguration>(configuration.GetSection(AutoCropConfiguration.SectionName));
            services.AddSingleton(provider => provider.GetRequiredService<IOptions<AutoCropConfiguration>>().Value);

            services.Configure<CollageStorageConfiguration>(configuration.GetSection(CollageStorageConfiguration.SectionName));
            services.AddSingleton(provider => provider.GetRequiredService<IOptions<CollageStorageConfiguration>>().Value);

            services.AddTransient<ICollageStorageService, CollageStorageService>();
            //services.AddTransient<IWebHostApiClient, FakeWebHostApiClient>();
            services.AddTransient<IWebHostApiClient, WebHostApiClient>();

            services.AddTransient<ILayoutGenerationService, GridLayoutGenerationService>();
            services.AddTransient<ILightManager, LightManager>();
            //services.AddTransient<IFaceDetectionService, OnnxFaceDetectionService>();
            services.AddTransient<IFaceDetectionService, CascadeFaceDetectionService>();
            services.AddTransient<PrintService>();
            services.AddTransient<QrCodeService>();
            services.AddTransient<MainWindow>();
            services.AddTransient<LayoutSelectionViewModel>();

            services.AddSingleton<IFrameSource, WebcamFrameSource>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<BackgroundFaceDetector>();
            services.AddSingleton<AutoCropService>();
        }

        private void SetCulture()
        {
            var uiConfiguration = Services.GetRequiredService<UiConfiguration>();
            if (!string.IsNullOrEmpty(uiConfiguration.Culture))
            {
                var culture = CultureInfo.GetCultures(CultureTypes.AllCultures).FirstOrDefault(x => x.Name == uiConfiguration.Culture) ?? CultureInfo.CurrentCulture;
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }
        }
    }

}
