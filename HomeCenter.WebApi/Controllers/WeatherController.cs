using HomeCenter.Core.Interface;
using Microsoft.AspNetCore.Mvc;

namespace HomeCenter.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherController(
    IWeatherService weatherService,
    IAccessTokenProvider accessTokenProvider
) : ControllerBase
{
    [HttpGet("weather", Name = "GetWeather")]
    public async Task<Weather> GetWeather(string accessToken)
    {
        accessTokenProvider.AccessToken = accessToken;
        return await weatherService.GetWeather();
    }
}