using Birchsoft.Azure.Function.Identity.Attributes;
using Birchsoft.Azure.Function.Identity.Authentication;
using Birchsoft.Azure.Function.Identity.Constants;
using Birchsoft.Azure.Function.Identity.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Options;
using System.Net;
using System.Reflection;
using System.Text.Json;

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
            if (AuthorizeFunction(context))
            {
                var requestData = await context.GetHttpRequestDataAsync();
                var showDetailedError = false;
                if (context.Items.TryGetValue(Consts.DetailedErrorMessage, out var detailedError))
                {
                    showDetailedError = (bool)detailedError;
                }

                if (requestData!.Headers.TryGetValues("Authorization", out var token))
                {
                    var strToken = token.FirstOrDefault();
                    var azureData = _azureMEID;

                    if (!string.IsNullOrWhiteSpace(strToken) && strToken.StartsWith("Bearer "))
                    {
                        try
                        {
                            var validator = new TokenValidator();

                            var claimsPrincipal = await validator.ValidateToken(strToken.Replace("Bearer ", string.Empty), azureData);

                            if (claimsPrincipal == null)
                            {
                                await ReturnErrorResponse(context, "Unable to retrieve claims from the token.", showDetailedError);
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            await ReturnErrorResponse(context, ex.Message, showDetailedError);
                            return;
                        }
                    }
                    else
                    {
                        await ReturnErrorResponse(context, "Invalid token.", showDetailedError);
                        return;
                    }
                }
                else
                {
                    await ReturnErrorResponse(context, "Token is missing.", showDetailedError);
                    return;
                }
            }

            await next(context);
        }

        private static async Task ReturnErrorResponse(FunctionContext context, string throwErrorMessage, bool showDetailedError)
        {
            var httpResponse = await context.GetHttpRequestDataAsync();
            if (httpResponse != null)
            {
                var response = httpResponse.CreateResponse();
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Headers.Add("Content-Type", "application/json");

                var errorResponse = new
                {
                    error = showDetailedError ? throwErrorMessage : HttpStatusCode.Unauthorized.ToString(),
                    code = showDetailedError ? response.StatusCode : HttpStatusCode.Unauthorized
                };
                var jsonResponse = JsonSerializer.Serialize(errorResponse);

                await using var writer = new StreamWriter(response.Body);
                await writer.WriteAsync(jsonResponse);
                await writer.FlushAsync();
            }
        }

        private static bool AuthorizeFunction(FunctionContext context)
        {
            var functionDefinition = context.FunctionDefinition;
            var entryPoint = functionDefinition.EntryPoint;
            var parts = entryPoint.Split('.');
            if (parts.Length < 2) return true;

            var methodName = parts[^1];
            var className = string.Join('.', parts[..^1]);

            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(className))
                .FirstOrDefault(t => t != null);

            var methodInfo = type?.GetMethod(methodName);

            if (methodInfo != null)
            {
                var isDoNotAuthorize = methodInfo.GetCustomAttribute<SkipFunctionAuthorization>() != null;

                if (isDoNotAuthorize)
                {
                    return false;
                }
            }

            return true;
        }
    }
}