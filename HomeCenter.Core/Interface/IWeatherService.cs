namespace HomeCenter.Core.Interface;

public interface IWeatherService
{
    Task<Weather> GetWeather();
}
