namespace HomeCenter.Core.Netatmo;

public class NetatmoOptions
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string ApiUrl { get; init; }
    public required string TokenEndpoint { get; init; }
    public required string TokenScope { get; init; }
    public required string DeviceId { get; init; }
    public required string OutdoorTemperatureModuleId { get; init; }
    public required string OutdoorAnemometerModuleId { get; init; }
    public int? WeatherCacheDurationSeconds { get; init; }
}
