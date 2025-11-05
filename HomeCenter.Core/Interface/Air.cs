namespace HomeCenter.Core.Interface;

public class Air : Measure
{
    /// <summary>
    /// Air temperature in degrees Celsius
    /// </summary>
    public double? Temperature { get; init; }

    /// <summary>
    /// Relative air humidity (percentage)
    /// </summary>
    public int? Humidity { get; init; }

    /// <summary>
    /// CO2 saturation in ppm (1 / 1 000 000)
    /// </summary>
    public int? Co2 { get; init; }

    /// <summary>
    ///  Air pressure in bars
    /// </summary>
    public double? Pressure { get; init; }

    /// <summary>
    /// Noise in db-s
    /// </summary>
    public int? Noise { get; init; }
}
