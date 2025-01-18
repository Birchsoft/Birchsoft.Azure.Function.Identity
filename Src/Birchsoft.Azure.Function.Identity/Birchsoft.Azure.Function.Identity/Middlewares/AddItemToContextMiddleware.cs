using Birchsoft.Azure.Function.Identity.Constants;
using Birchsoft.Azure.Function.Identity.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Options;

namespace Birchsoft.Azure.Function.Identity.Middlewares
{
    internal class AddItemToContextMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ErrorMessageConfig _errorMessageConfig;

        public AddItemToContextMiddleware(IOptions<ErrorMessageConfig> errorMessageConfig)
        {
            _errorMessageConfig = errorMessageConfig.Value;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            context.Items[Consts.DetailedErrorMessage] = _errorMessageConfig.ShowDetailedErrorMessages;

            await next(context);
        }
    }
}
