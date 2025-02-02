using Birchsoft.Azure.Function.Identity.Authentication;
using Birchsoft.Azure.Function.Identity.Constants;
using Birchsoft.Azure.Function.Identity.Models;
using Birchsoft.Azure.Function.Identity.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Options;

namespace Birchsoft.Azure.Function.Identity.Middlewares
{
    internal class PreValidateTokenMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ErrorMessageConfig _errorMessageConfig;

        public PreValidateTokenMiddleware(IOptions<ErrorMessageConfig> errorMessageConfig)
        {
            _errorMessageConfig = errorMessageConfig.Value;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var showDetailedError = _errorMessageConfig.ShowDetailedErrorMessages;

            try
            {
                var identitySettings = new IdentitySettings();
                identitySettings.Initialize(context);
                if (!identitySettings.SkipFunctionAuthorization)
                {
                    var requestData = await context.GetHttpRequestDataAsync();

                    if (requestData!.Headers.TryGetValues("Authorization", out var token))
                    {
                        var strToken = token.FirstOrDefault();

                        if (!string.IsNullOrWhiteSpace(strToken) && strToken.StartsWith("Bearer "))
                        {
                            var jwToken = strToken.Replace("Bearer ", string.Empty);
                            context.Items[Consts.DetailedErrorMessage] = _errorMessageConfig.ShowDetailedErrorMessages;
                            identitySettings.SetJWToken(jwToken).SetJWTokenRoles(jwToken);

                            if (identitySettings.JWTRoles.Length > 0 || identitySettings.AttributeRoles.Length > 0)
                            {
                                var rolesValidator = new RolesValidator();

                                if (!rolesValidator.Validate(identitySettings))
                                {
                                    await Helper.ReturnErrorResponse(context, "Role validation failed: No valid roles were found in the token.", showDetailedError);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            await Helper.ReturnErrorResponse(context, "Invalid token.", showDetailedError);
                            return;
                        }
                    }
                    else
                    {
                        await Helper.ReturnErrorResponse(context, "Token is missing.", showDetailedError);
                        return;
                    }
                }

                context.Items[Consts.IdentitySettings] = identitySettings;
            }
            catch (Exception ex)
            {
                await Helper.ReturnErrorResponse(context, ex.Message, showDetailedError);
                return;
            }

            await next(context);
        }
    }
}
