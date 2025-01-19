using Birchsoft.Azure.Function.Identity.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FunctionIdentityExample
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, configBuilder) =>
                {
                    configBuilder.SetBasePath(hostContext.HostingEnvironment.ContentRootPath)
                          .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                          .AddAzFunctionIdentityJsonConfig(hostContext)
                          .AddEnvironmentVariables();
                })
                .ConfigureFunctionsWebApplication(worker =>
                {
                    worker.UseAzFunctionIdentityMiddleware(false);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;

                    services.AddLogging();
                    services.AddAzFunctionIdentityConfig(configuration);
                })
                .Build();

            await host.RunAsync();
        }
    }
}