using System.Numerics;
using GameFramework.Layers;
using Veldrid;

namespace GameFramework;

public class GameInput
{
    private readonly object _sync = new();

    public GameApplication Application { get; }

    private InputSnapshot? _snapshot;

    public InputSnapshot InputSnapshot
    {
        get
        {
            if (_snapshot == null)
            {
                throw new InvalidOperationException("Cannot get input snapshot before the update loop starts!");
            }

            return _snapshot;
        }

        internal set => _snapshot = value;
    }

    public Vector2 MousePosition => InputSnapshot.MousePosition;

    private readonly HashSet<Key> _downKeys = new();
    private readonly HashSet<MouseButton> _downMouseButtons = new();

    public IReadOnlySet<Key> DownKeys => _downKeys;
   
    public IReadOnlySet<MouseButton> DownMouseButtons => _downMouseButtons;

    public GameInput(GameApplication application)
    {
        Application = application;

        Application.Resources.Window.KeyDown += WindowOnKeyDown;
        Application.Resources.Window.KeyUp += WindowOnKeyUp;
        Application.Resources.Window.MouseDown += WindowOnMouseDown;
        Application.Resources.Window.MouseUp += WindowOnMouseUp;
    }

    private void WindowOnMouseDown(MouseEvent obj)
    {
        lock (_sync)
        {
            _downMouseButtons.Add(obj.MouseButton);
        }
    }

    private void WindowOnMouseUp(MouseEvent obj)
    {
        lock (_sync)
        {
            _downMouseButtons.Remove(obj.MouseButton);
        }
    }

    private void WindowOnKeyDown(KeyEvent obj)
    {
        lock (_sync)
        {
            _downKeys.Add(obj.Key);
        }
    }

    private void WindowOnKeyUp(KeyEvent obj)
    {
        lock (_sync)
        {
            _downKeys.Remove(obj.Key);
        }
    }

    public bool IsKeyDown(Key key)
    {
        lock (_sync)
        {
            return DownKeys.Contains(key);
        }
    }

    public bool IsMouseDown(MouseButton button)
    {
        lock (_sync)
        {
            return DownMouseButtons.Contains(button);
        }
    }

    public bool KeyPressed(Key key)
    {
        lock (_sync)
        {
            return InputSnapshot.KeyEvents.Any(x => x.Key == key && x.Down);
        }
    }

    public bool KeyReleased(Key key)
    {
        lock (_sync)
        {
            return InputSnapshot.KeyEvents.Any(x => x.Key == key && !x.Down);
        }
    }

    public bool MousePressed(MouseButton button)
    {
        lock (_sync)
        {
            return InputSnapshot.MouseEvents.Any(x => x.MouseButton == button && x.Down);
        }
    }

    public bool MouseReleased(MouseButton button)
    {
        lock (_sync)
        {
            return InputSnapshot.MouseEvents.Any(x => x.MouseButton == button && !x.Down);
        }
    }

    public Vector2 MouseDelta => Application.Resources.Window.MouseDelta;

    public float ScrollDelta => InputSnapshot.WheelDelta;
}