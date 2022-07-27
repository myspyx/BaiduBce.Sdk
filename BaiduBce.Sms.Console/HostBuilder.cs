using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BaiduBce.Sms.Console;

public static class HostBuilder
{
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var hostBuilder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, builder) => { builder.SetBasePath(Directory.GetCurrentDirectory()); })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IAuthService, AuthService>();
                services.AddSingleton<ISmsClient, SmsClient>();
                services.AddHttpClient<SmsClient>();
            });

        return hostBuilder;
    }
}