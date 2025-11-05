using System.Text.Json.Serialization;

namespace HomeCenter.Core.Netatmo.RestApiDto;

internal class GetMeasureResponse
{
    [JsonPropertyName("body")]
    public required GetMeasureResponseBody Body { get; init; }

    [JsonPropertyName("status")]
    public required string status { get; init; }

    [JsonPropertyName("time_exec")]
    public required double TimeExec { get; init; }

    [JsonPropertyName("time_server")]
    public required long TimeServer { get; init; }
}
