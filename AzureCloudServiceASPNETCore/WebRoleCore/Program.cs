using System.Net;

namespace WebRoleCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.ConfigureKestrel(options =>
            {
                var address = Environment.GetEnvironmentVariable("ASPNETCORE_ADDRESS");
                var port = Environment.GetEnvironmentVariable("ASPNETCORE_PORT");
                if (!string.IsNullOrWhiteSpace(address)
                    && !string.IsNullOrWhiteSpace(port)
                    && int.TryParse(port, out var portNumber))
                {
                    options.Listen(IPAddress.Parse(address), portNumber);
                }
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();
            var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStarted.Register(() =>
            {
                Console.WriteLine("WebRoleCore started.");
            });
            lifetime.ApplicationStopped.Register(() =>
            {
                Console.WriteLine("WebRoleCore stopped.");
            });

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
