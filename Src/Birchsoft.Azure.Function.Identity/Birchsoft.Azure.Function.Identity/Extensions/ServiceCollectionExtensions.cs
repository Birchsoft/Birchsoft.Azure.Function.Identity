using Birchsoft.Azure.Function.Identity.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Birchsoft.Azure.Function.Identity.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAzFunctionAuthTokenConfig(this IServiceCollection services, IConfiguration configuration, string? authSettingsJsonName = null)
        {
            services.Configure<AzureMEID>(configuration.GetSection(nameof(AzureMEID)));

            return services;
        }
    }
}
