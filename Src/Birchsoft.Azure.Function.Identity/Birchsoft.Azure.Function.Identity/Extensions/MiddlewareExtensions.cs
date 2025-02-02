using Birchsoft.Azure.Function.Identity.Middlewares;
using Birchsoft.Azure.Function.Identity.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Birchsoft.Azure.Function.Identity.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IFunctionsWorkerApplicationBuilder UseAzFunctionIdentityMiddleware(this IFunctionsWorkerApplicationBuilder worker, bool detailedErrorMessage = false)
        {
            worker.Services.Configure<ErrorMessageConfig>(options =>
            {
                options.ShowDetailedErrorMessages = detailedErrorMessage;
            });

            worker.UseWhen<PreValidateTokenMiddleware>((context) =>
            {
                return context.FunctionDefinition.InputBindings.Values
                          .First(a => a.Type.EndsWith("Trigger")).Type == "httpTrigger";
            });
            worker.UseWhen<ValidateTokenMiddleware>((context) =>
            {
                return context.FunctionDefinition.InputBindings.Values
                          .First(a => a.Type.EndsWith("Trigger")).Type == "httpTrigger";
            });

            return worker;
        }
    }
}
