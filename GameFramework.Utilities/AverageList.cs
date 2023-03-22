namespace GameFramework.Utilities;

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