using System.Reflection;

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

                builder.Services.AddRazorPages();
                builder.Services.AddControllers()
                       .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; });

                builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(applicationAssembly, hostAssembly));

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