using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SensorBatteryChecker.Services.Battery;
using static System.DateTime;

namespace SensorBatteryChecker.Functions;

public class HttpBatteryChecker
{
    private readonly ILogger<HttpBatteryChecker> _logger;
    private readonly IBatteryService _batteryService;

    public HttpBatteryChecker(
        ILogger<HttpBatteryChecker> logger,
        IBatteryService batteryService)
    {
        _logger = logger;
        _batteryService = batteryService;
    }

    [FunctionName("HttpBatteryChecker")]
    [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
    [OpenApiSecurity(
        "function_key", 
        SecuritySchemeType.ApiKey, 
        Name = "code", 
        In = OpenApiSecurityLocationType.Query)]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.OK, 
        contentType: "text/plain", 
        bodyType: typeof(string), 
        Description = "The OK response")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
    {
        _logger.LogInformation("Checking battery status from HTTP request");

        await _batteryService.CheckBatteryAsync();

        _logger.LogInformation($"Check finished {Now} from HTTP request");

        return new OkObjectResult("{\"message\": \"OK\"}");
    }
}