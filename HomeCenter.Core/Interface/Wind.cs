namespace HomeCenter.Core.Interface;

public class Wind : Measure
{
    public int? WindStrength { get; init; }
    public int? WindAngle { get; init; }
    public int? GustStrength { get; init; }
    public int? GustAngle { get; init; }
}
