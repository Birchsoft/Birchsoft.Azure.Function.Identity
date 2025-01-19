using Birchsoft.Azure.Function.Identity.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionIdentityExample
{
    public class FunctionExample
    {
        private readonly ILogger<FunctionExample> _logger;

        public FunctionExample(ILogger<FunctionExample> logger)
        {
            _logger = logger;
        }

        [SkipFunctionAuthorization]
        [Function("FunctionNotAuth")]
        public IActionResult RunNotAuth([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request FunctionNotAuth.");
            return new OkObjectResult("Welcome to Azure FunctionNotAuth!");
        }

        [Function("FunctionAuth")]
        public IActionResult RunAuth([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request FunctionAuth.");
            return new OkObjectResult("Welcome to Azure FunctionAuth!");
        }
    }
}
