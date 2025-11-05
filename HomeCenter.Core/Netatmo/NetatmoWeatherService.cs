using HomeCenter.Core.Interface;
using HomeCenter.Core.Netatmo.RestApiDto;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

namespace HomeCenter.Core.Netatmo;

public class NetatmoWeatherService(
    IHttpClientFactory httpClientFactory,
    IAccessTokenProvider accessTokenProvider,
    IOptions<NetatmoOptions> options,
    IMemoryCache memoryCache
) : IWeatherService
{
    #region Constants
    
    private const int DefaultCacheDurationSeconds = 90;
    
    #endregion
    
    #region Fields

    private readonly NetatmoOptions _netatmoOptions = options.Value;

    #endregion

    #region IWeatherService implementation

    public async Task<Weather> GetWeather()
    {
        // build cache key using device id so multiple devices won't conflict
        
        var cacheKey = $"NetatmoWeather_{_netatmoOptions.DeviceId}";
        if (memoryCache.TryGetValue<Weather>(cacheKey, out var cachedWeather) && cachedWeather != null)
        {
            return cachedWeather;
        }

        var accessToken = accessTokenProvider.AccessToken;

        var indoorAir = GetIndoorAir(accessToken).ConfigureAwait(false);
        var outdoorAir = GetOutdoorAir(accessToken).ConfigureAwait(false);
        var wind = GetOutdoorWind(accessToken).ConfigureAwait(false);

        var weather = new Weather
        {
            IndoorAir = await indoorAir,
            OutdoorAir = await outdoorAir,
            Wind = await wind
        };

        var cacheDurationSeconds = _netatmoOptions.WeatherCacheDurationSeconds ?? DefaultCacheDurationSeconds;
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheDurationSeconds));

        memoryCache.Set(cacheKey, weather, cacheEntryOptions);

        return weather;
    }

    #endregion

    #region Helpers

    private async Task<Air> GetIndoorAir(string accessToken)
    {
        var air = await GetMeasure
        (
            accessToken,
            null,
            ["temperature", "humidity", "co2", "pressure", "noise"],
            (values, measuredOn) => new Air
            {
                MeasuredOn = measuredOn,
                Temperature = values?[0],
                Humidity = (int?)values?[1],
                Co2 = (int?)values?[2],
                Pressure = values?[3],
                Noise = (int?)values?[4]
            }
        ).ConfigureAwait(false);

        return air;
    }

    private async Task<Air> GetOutdoorAir(string accessToken)
    {
        var air = await GetMeasure
        (
            accessToken,
            _netatmoOptions.OutdoorTemperatureModuleId,
            ["temperature", "humidity"],
            (values, measuredOn) => new Air
            {
                MeasuredOn = measuredOn,
                Temperature = values?[0],
                Humidity = (int?)values?[1]
            }
        ).ConfigureAwait(false);

        return air;
    }

    private async Task<Wind> GetOutdoorWind(string accessToken)
    {
        var wind = await GetMeasure
            (
                accessToken,
                _netatmoOptions.OutdoorAnemometerModuleId,
                ["windstrength", "windangle", "guststrength", "gustangle"],
                (values, measuredOn) => new Wind
                {
                    MeasuredOn = measuredOn,
                    WindStrength = (int?)values?[0],
                    WindAngle = (int?)values?[1],
                    GustStrength = (int?)values?[2],
                    GustAngle = (int?)values?[3]
                }
            )
            .ConfigureAwait(false);

        return wind;
    }

    private async Task<T> GetMeasure<T>(string accessToken, string? moduleId, IEnumerable<string> types,
        Func<IList<double>?, DateTime?, T> mapper) where T : Measure
    {
        using var httpClient = GetHttpClient(accessToken);

        var url = GetUrlForGetMeasure(moduleId, types);

        var response = await httpClient
            .GetAsync(url)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var jsonSerializerOptions = new JsonSerializerOptions();
        jsonSerializerOptions.Converters.Add(new GetMeasureResponseBodyConverter());

        var getMeasureResponse = await response.Content
            .ReadFromJsonAsync<GetMeasureResponse>(jsonSerializerOptions)
            .ConfigureAwait(false);

        var measure = MapGetMeasureResponseToMeasure(getMeasureResponse, mapper);

        return measure;
    }

    private T MapGetMeasureResponseToMeasure<T>
    (
        GetMeasureResponse? getMeasureResponse,
        Func<IList<double>?, DateTime?, T> mapper
    ) where T : Measure
    {
        if (getMeasureResponse == null || getMeasureResponse.Body.Count == 0)
        {
            return mapper(null, null);
        }

        var latestTimeStampWithValues = getMeasureResponse.Body.MaxBy(kvp => kvp.Key);
        return mapper(latestTimeStampWithValues.Value, UnixTimeStampToDateTime(latestTimeStampWithValues.Key));
    }

    private HttpClient GetHttpClient(string accessToken)
    {
        var httpClient = httpClientFactory.CreateClient();

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return httpClient;
    }

    private string GetUrlForGetMeasure(string? moduleId, IEnumerable<string> types,
        IDictionary<string, string>? queryParameters = null)
    {
        var defaultQueryParameters = new Dictionary<string, string>
        {
            ["date_begin"] = DateTimeOffset.UtcNow.AddMinutes(-30).ToUnixTimeSeconds().ToString(),
            ["scale"] = "5min",
            ["optimize"] = "false",
            ["real_time"] = "true"
        };

        if (moduleId != null)
        {
            defaultQueryParameters["module_id"] = moduleId;
        }

        defaultQueryParameters["type"] = string.Join(',', types);

        var finalQueryParameters = defaultQueryParameters
            .Union(queryParameters ?? new Dictionary<string, string>())
            .ToDictionary(s => s.Key, s => s.Value);

        return GetUrlForAction
        (
            "getmeasure",
            finalQueryParameters
        );
    }

    private string GetUrlForAction(string action, IDictionary<string, string> queryParameters)
    {
        var builder = new UriBuilder(_netatmoOptions.ApiUrl);

        builder.Port = -1;
        builder.Path += action;

        var query = HttpUtility.ParseQueryString(builder.Query);

        query["device_id"] = _netatmoOptions.DeviceId;

        foreach (var queryParameter in queryParameters)
        {
            query[queryParameter.Key] = queryParameter.Value;
        }

        builder.Query = query.ToString();
        var url = builder.ToString();

        return url;
    }

    private static DateTime? UnixTimeStampToDateTime(long? unixTimeStamp)
    {
        return unixTimeStamp.HasValue
            ? new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimeStamp.Value)
            : null;
    }

    #endregion
}