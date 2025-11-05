namespace HomeCenter.Core.Interface;

public class Weather
{
    public required Air OutdoorAir { get; init; }
    public required Air IndoorAir { get; init; }
    public required Wind Wind { get; init; }
}
