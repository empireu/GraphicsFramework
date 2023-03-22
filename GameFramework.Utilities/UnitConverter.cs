namespace GameFramework.Utilities;

public sealed class UnitConverter
{
    public static readonly UnitConverter Bytes = new("bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB", "RB", "QB");

    public static string ConvertBytes(double value, int decimalPlaces = 2) => Bytes.Convert(value, decimalPlaces);
    public static string ConvertBytesRate(double value, int decimalPlaces = 2) => $"{Bytes.Convert(value, decimalPlaces)}/s";

    
    private readonly string[] _units;

    public UnitConverter(params string[] units)
    {
        if (units.Length == 0)
        {
            throw new ArgumentException("Cannot use empty unit array.");
        }

        _units = units.ToArray();
    }

    public string Convert(double value, int decimalPlaces = 2)
    {
        var unitIndex = value > 0 
            ? (int)Math.Floor(Math.Log10(value) / 3) 
            : 0;

        unitIndex = Math.Min(unitIndex, _units.Length - 1);

        return $"{Math.Round(value / Math.Pow(1000, unitIndex), decimalPlaces, MidpointRounding.ToEven)} {_units[unitIndex]}";
    }
}