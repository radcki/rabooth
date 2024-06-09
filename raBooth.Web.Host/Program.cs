using raBooth.Web.Host.Infrastructure;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using raBooth.Web.Core.DataAccess;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace raBooth.Web.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            {
                var hostAssembly = Assembly.Load("raBooth.Web.Host");
                var applicationAssembly = Assembly.Load("raBooth.Web.Core");
                var builder = WebApplication.CreateBuilder(args);
                builder.Configuration.AddEnvironmentVariables(prefix: "raBooth_");


                var mysqlConnectionString = builder.Configuration.GetConnectionString("MySql");
                builder.Services.AddDbContext<IDatabaseContext, DatabaseContext>(options =>
                                                                                 {
                                                                                     options.UseMySql(mysqlConnectionString,
                                                                                                      MariaDbServerVersion.LatestSupportedServerVersion,
                                                                                                      builder => { builder.MigrationsAssembly("raBooth.Web.Host"); });
                                                                                 }, ServiceLifetime.Transient);

                builder.Services.AddRazorPages();
                builder.Services.AddControllers()
                       .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; });


                builder.Services.Configure<FilesystemPhotoStorageConfiguration>(builder.Configuration.GetSection(FilesystemPhotoStorageConfiguration.SectionName));
                builder.Services.AddSingleton(provider => provider.GetRequiredService<IOptions<FilesystemPhotoStorageConfiguration>>().Value);

                builder.Services.Configure<ConfigApiKeyValidatorConfiguration>(builder.Configuration.GetSection(ConfigApiKeyValidatorConfiguration.SectionName));
                builder.Services.AddSingleton(provider => provider.GetRequiredService<IOptions<ConfigApiKeyValidatorConfiguration>>().Value);

                builder.Services.AddSingleton<ApiKeyAuthorizationFilter>();
                builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(applicationAssembly, hostAssembly));
                builder.Services.AddTransient<IFormFileEnvelopeMapper, FormFileEnvelopeMapper>();
                builder.Services.AddTransient<IPhotoStorage, FilesystemPhotoStorage>();
                builder.Services.AddTransient<IApiKeyValidator, ConfigApiKeyValidator>();

                var app = builder.Build();

                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Error");
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseStaticFiles();

                app.UseRouting();

                app.MapControllers();
                app.MapRazorPages();

                app.UseAuthorization();


                app.Run();
            }
        }
    }
}