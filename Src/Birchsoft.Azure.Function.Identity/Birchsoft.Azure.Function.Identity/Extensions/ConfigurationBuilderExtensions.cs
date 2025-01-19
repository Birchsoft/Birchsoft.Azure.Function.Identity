using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Birchsoft.Azure.Function.Identity.Extensions
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddAzFunctionIdentityJsonConfig(this IConfigurationBuilder builder, HostBuilderContext context, string? authSettingsJsonName = null)
        {
            if (string.IsNullOrWhiteSpace(authSettingsJsonName))
            {
                authSettingsJsonName = "auth.settings.json";
            }

            builder.SetBasePath(context.HostingEnvironment.ContentRootPath)
                .AddJsonFile(authSettingsJsonName, optional: false, reloadOnChange: true);

            return builder;
        }
    }
}
