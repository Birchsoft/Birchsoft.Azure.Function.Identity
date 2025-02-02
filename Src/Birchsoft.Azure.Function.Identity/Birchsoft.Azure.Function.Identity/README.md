## Birchsoft.Azure.Function.Identity

---
## **Important:**

The NuGet package supports only Azure Functions using the isolated worker model and validates JWT issued by Microsoft Entra ID.

---

The `Birchsoft.Azure.Function.Identity` package simplifies Azure HTTP Trigger Function Authentication by utilizing JSON Web Tokens (JWT).

### Configuration
To set up your function correctly in Azure, follow these steps:

**1. Create a JSON configuration file and name it `identity.settings.json`:**

```json
{
    "AzureMEID": {
        "TenantId": "xxxxxxxx-8eba-45ca-a7af-xxxxxxxxxxxx",
        "Audience": "xxxxxxxx-2612-4413-b5b2-xxxxxxxxxxxx",
        "ObjectId": "xxxxxxxx-9f62-4343-aaf6-xxxxxxxxxxxx"
    }
}
```
The `Audience` is the `Application (client) ID` from Azure App Registrations.

You can choose any name for this JSON configuration file, but make sure to specify it as a parameter in the `AddAzFunctionIdentityJsonConfig()` method.

**2. Configure the Program.cs file:**

```cs
class Program
{
    public static async Task Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureAppConfiguration((hostContext, configBuilder) =>
            {
                configBuilder.SetBasePath(hostContext.HostingEnvironment.ContentRootPath)
                        .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                        .AddAzFunctionIdentityJsonConfig(hostContext)
                        .AddEnvironmentVariables();
            })
            .ConfigureFunctionsWebApplication(worker =>
            {
                worker.UseAzFunctionIdentityMiddleware(false);
            })
            .ConfigureServices((hostContext, services) =>
            {
                var configuration = hostContext.Configuration;

                services.AddLogging();
                services.AddAzFunctionIdentityConfig(configuration);
            })
            .Build();

        await host.RunAsync();
    }
}
```

**3. Skip Azure function authorization**

You can also bypass Azure Function (endpoint) authorization by adding the attribute `[SkipFunctionAuthorization]`.

```cs
[SkipFunctionAuthorization]
[Function("FunctionNotAuth")]
public IActionResult RunNotAuth([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
{
    _logger.LogInformation("C# HTTP trigger function processed a request FunctionNotAuth.");
    return new OkObjectResult("Welcome to Azure FunctionNotAuth!");
}
```

**4. Authorization based on roles included in the token.**

To authorize functions based on the roles specified in the token, add an attribute, such as `[AppRoles("Admin", "User")]`.

---
## **Important:**

If the token contains roles and the function does not include the `AppRoles` attribute with the corresponding role, authentication will fail.

---
```cs
[AppRoles("Admin", "User")]
[Function("FunctionAuthRoles")]
public IActionResult RunAuthRoles([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
{
    _logger.LogInformation("C# HTTP trigger function processed a request FunctionAuthRoles.");
    return new OkObjectResult("Welcome to Azure FunctionAuthRoles!");
}
```


