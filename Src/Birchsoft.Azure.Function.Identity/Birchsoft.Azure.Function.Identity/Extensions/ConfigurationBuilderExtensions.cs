using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Birchsoft.Azure.Function.Identity.Extensions
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddAzFunctionIdentityJsonConfig(this IConfigurationBuilder builder, HostBuilderContext context, string? identitySettingsJsonName = null)
        {
            if (string.IsNullOrWhiteSpace(identitySettingsJsonName))
            {
                identitySettingsJsonName = "identity.settings.json";
            }

            builder.SetBasePath(context.HostingEnvironment.ContentRootPath)
                .AddJsonFile(identitySettingsJsonName, optional: false, reloadOnChange: true);

            return builder;
        }
    }
}
