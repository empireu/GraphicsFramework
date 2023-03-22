namespace GameFramework.Utilities;

public static class RectanglePacker
{
    public class PackingRectangle
    {
        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public bool WasPacked { get; set; }
    };

    public sealed class RectangleHolder<T> : PackingRectangle
    {
        public T Instance { get; }

        public RectangleHolder(T instance, int width, int height)
        {
            Instance = instance;
            Width = width;
            Height = height;
        }
    }

    public static void Pack(IEnumerable<PackingRectangle> enumerable, int width)
    {
        var rectangles = enumerable.OrderBy(x => x.Height);

        var x = 0;
        var y = 0;

        var largestHeight = 0;

        foreach (var rect in rectangles)
        {
            if (x + rect.Width > width)
            {
                y += largestHeight;
                x = 0;
                largestHeight = 0;
            }

            rect.X = x;
            rect.Y = y;

            x += rect.Width;

            if (rect.Height > largestHeight)
            {
                largestHeight = rect.Height;
            }

            rect.WasPacked = true;
        }
    }

}