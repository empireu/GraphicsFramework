using System.Runtime.CompilerServices;

namespace GameFramework.Sdf;

/// <summary>
///     An spiral pattern grid iterator.
/// </summary>
public ref struct SpiralIterator
{
    private int _direction;

    /// <summary>
    ///     Gets the current ring in the spiral.
    /// </summary>
    public int Level { get; private set; }

    /// <summary>
    ///     Gets the current horizontal position in the spiral.
    /// </summary>
    public int X { get; private set; }

    /// <summary>
    ///     Gets the current vertical position in the spiral.
    /// </summary>
    public int Y { get; private set; }

    public SpiralIterator()
    {
        Level = 1;
    }

    /// <summary>
    ///     Advances the position in the spiral pattern.
    /// </summary>
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