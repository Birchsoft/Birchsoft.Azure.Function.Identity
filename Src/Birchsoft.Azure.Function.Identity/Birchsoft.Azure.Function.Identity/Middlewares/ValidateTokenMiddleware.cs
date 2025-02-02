using Birchsoft.Azure.Function.Identity.Authentication;
using Birchsoft.Azure.Function.Identity.Constants;
using Birchsoft.Azure.Function.Identity.Models;
using Birchsoft.Azure.Function.Identity.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Options;

namespace Birchsoft.Azure.Function.Identity.Middlewares
{
    internal class ValidateTokenMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly AzureMEID _azureMEID;

        public ValidateTokenMiddleware(IOptions<AzureMEID> azureMEID)
        {
            _azureMEID = azureMEID.Value;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var requestData = await context.GetHttpRequestDataAsync();
            var showDetailedError = false;
            if (context.Items.TryGetValue(Consts.DetailedErrorMessage, out var detailedError))
            {
                showDetailedError = (bool)detailedError;
            }

            var identitySettings = new IdentitySettings();
            if (context.Items.TryGetValue(Consts.IdentitySettings, out var settings))
            {
                identitySettings = (IdentitySettings)settings;
            }

            if (!identitySettings.SkipFunctionAuthorization)
            {
                var azureData = _azureMEID;

                if (!string.IsNullOrWhiteSpace(identitySettings.JWToken))
                {
                    try
                    {
                        var tokenValidator = new TokenValidator();
                        var claimsPrincipal = await tokenValidator.Validate(identitySettings, azureData);

                        if (claimsPrincipal == null)
                        {
                            await Helper.ReturnErrorResponse(context, "Unable to retrieve claims from the token.", showDetailedError);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        await Helper.ReturnErrorResponse(context, ex.Message, showDetailedError);
                        return;
                    }
                }
                else
                {
                    await Helper.ReturnErrorResponse(context, "Empty token.", showDetailedError);
                    return;
                }
            }

            await next(context);
        }
    }
}