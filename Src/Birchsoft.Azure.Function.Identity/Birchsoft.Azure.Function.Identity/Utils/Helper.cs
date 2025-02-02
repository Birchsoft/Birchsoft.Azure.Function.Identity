using Birchsoft.Azure.Function.Identity.Attributes;
using Microsoft.Azure.Functions.Worker;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace Birchsoft.Azure.Function.Identity.Utils
{
    internal class Helper
    {
        internal static async Task ReturnErrorResponse(FunctionContext context, string throwErrorMessage, bool showDetailedError)
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
                    code = response.StatusCode
                };
                var jsonResponse = JsonSerializer.Serialize(errorResponse);

                await using var writer = new StreamWriter(response.Body);
                await writer.WriteAsync(jsonResponse);
                await writer.FlushAsync();
            }
        }

        internal static bool SkipFunctionAuthorization(FunctionContext context)
        {
            var functionDefinition = context.FunctionDefinition;
            var entryPoint = functionDefinition.EntryPoint;
            var parts = entryPoint.Split('.');
            if (parts.Length < 2) return false;

            var methodName = parts[^1];
            var className = string.Join('.', parts[..^1]);

            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(className))
                .FirstOrDefault(t => t != null);

            var methodInfo = type?.GetMethod(methodName);

            if (methodInfo != null)
            {
                var doesCustomAttributeExist = methodInfo.GetCustomAttribute<SkipFunctionAuthorization>() != null;

                if (doesCustomAttributeExist)
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool SkipRoleAuthorization(FunctionContext context, out string[] roles)
        {
            roles = [];
            var functionDefinition = context.FunctionDefinition;
            var entryPoint = functionDefinition.EntryPoint;
            var parts = entryPoint.Split('.');
            if (parts.Length < 2) return false;

            var methodName = parts[^1];
            var className = string.Join('.', parts[..^1]);

            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(className))
                .FirstOrDefault(t => t != null);

            var methodInfo = type?.GetMethod(methodName);

            if (methodInfo != null)
            {
                var attribute = methodInfo.GetCustomAttribute<AppRoles>();
                if (attribute == null) return true;

                roles = attribute.Roles;
                if (roles.Length == 0)
                {
                    throw new SecurityTokenValidationException("The AppRoles attribute must include parameters.");
                }

                return false;
            }

            return true;
        }
    }
}
