using HomeCenter.Core.Interface;
using HomeCenter.Core.Netatmo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HomeCenter.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AppController(
    ILogger<WeatherController> logger,
    IOAuthClient oAuthClient,
    IOptions<NetatmoOptions> netatmoOptions
)
    : ControllerBase
{
    private readonly ILogger<WeatherController> _logger = logger;
    private readonly IOAuthClient _oAuthClient = oAuthClient;
    private readonly NetatmoOptions _netatmoOptions = netatmoOptions.Value;

    [HttpGet("ping", Name = "Ping")]
    public string Ping()
    {
        return "Pong";
    }

    [HttpGet("version", Name = "Version")]
    public string? Version()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString();
        return version;
    }
}