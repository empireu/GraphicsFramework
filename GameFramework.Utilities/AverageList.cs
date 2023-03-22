namespace GameFramework.Utilities;

/// <summary>
///     Utility class for averaging a small set of values. This results in fewer numerical errors than a continuous average.
/// </summary>
public sealed class AverageList
{
    private readonly int _count;
    private readonly List<float> _samples = new();

    public AverageList(int count = 10)
    {
        _count = count;
    }

    public void AddSample(float sample)
    {
        _samples.Add(sample);

        if (_samples.Count > _count)
        {
            _samples.RemoveAt(0);
        }
    }

    public float Average => _samples.Sum() / _samples.Count;
}