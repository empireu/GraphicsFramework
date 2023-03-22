using System.Runtime.CompilerServices;

namespace GameFramework.Sdf;

public ref struct SpiralIterator
{
    private int _direction;

    public int Level { get; private set; }

    public int X { get; private set; }

    public int Y { get; private set; }

    public SpiralIterator()
    {
        Level = 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Next()
    {
        switch (_direction)
        {
            case 0:
            {
                ++X;

                if (X == Level)
                {
                    ++_direction;
                }

                break;
            }

            case 1:
            {
                ++Y;

                if (Y == Level)
                {
                    ++_direction;
                }

                break;
            }
            case 2:
            {
                --X;

                if (-X == Level)
                {
                    ++_direction;
                }

                break;
            }
            case 3:
            {
                --Y;

                if (-Y == Level)
                {
                    _direction = 0;
                    ++Level;
                }

                break;
            }
        }
    }
};