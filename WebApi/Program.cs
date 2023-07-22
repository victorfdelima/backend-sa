using Microsoft.AspNetCore.SignalR;
using WebApi.Hub;

namespace WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var hubContext = services.GetRequiredService<IHubContext<ContadorHub>>();
            hubContext.Clients.All.SendAsync("ReceiveMessage", "O servidor está pronto.");
        }

        host.Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}