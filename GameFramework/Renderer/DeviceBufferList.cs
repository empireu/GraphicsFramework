using System.Diagnostics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace GameFramework.Renderer;

public unsafe class DeviceBufferList<TData> : IDisposable where TData : unmanaged
{
    private readonly GraphicsDevice _device;
    private readonly DeviceBufferPool _pool;
    private readonly BufferUsage _usage;

    private DataBuffer? _latest;

    public DeviceBufferList(GraphicsDevice device, DeviceBufferPool pool, int maxItemsPerBuffer, BufferUsage usage)
    {
        _device = device;
        _pool = pool;
        _usage = usage;
        MaxItemsPerBuffer = maxItemsPerBuffer;

        Clear();

        Debug.Assert(_latest != null);
        Debug.Assert(_buffers.Count == 1 && _buffers[0] == _latest);
    }

    public int MaxItemsPerBuffer { get; }

    private readonly List<DataBuffer> _buffers = new();

    public IReadOnlyList<DataBuffer> Buffers => _buffers;

    private DataBuffer AddBuffer()
    {
        var instanceBuffer = _pool.Get((uint)(sizeof(TData) * MaxItemsPerBuffer), _usage);

        var result = new DataBuffer(_device, instanceBuffer);

        result.PrepareWriting();

        _buffers.Add(result);

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DataBuffer GetWriteBuffer()
    {
        if (_latest == null)
        {
            throw new InvalidOperationException();
        }

        if (_latest.InstanceCount == MaxItemsPerBuffer)
        {
            _latest = AddBuffer();
        }

        _latest.PrepareWriting();

        return _latest;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ref TData instance)
    {
        var buffer = GetWriteBuffer();
        var view = buffer.InstanceDataView;
        view[buffer.InstanceCount++] = instance;
    }

    private void ReturnAll()
    {
        foreach (var dataBuffer in _buffers)
        {
            if (dataBuffer.IsMapped)
            {
                dataBuffer.UnMap();
            }

            _pool.Return(dataBuffer.InstanceDataBuffer);
        }

        _buffers.Clear();
    }

    public void Clear()
    {
        ReturnAll();

        _latest = AddBuffer();
    }

    public sealed class DataBuffer
    {
        private readonly GraphicsDevice _device;

        public bool IsMapped { get; private set; }

        public DataBuffer(GraphicsDevice device, DeviceBuffer instanceDataBuffer)
        {
            _device = device;
            InstanceDataBuffer = instanceDataBuffer;
        }

        public void PrepareDraw()
        {
            if (IsMapped)
            {
                UnMap();
            }
        }

        public void PrepareWriting()
        {
            if (!IsMapped)
            {
                Map();
            }
        }

        public DeviceBuffer InstanceDataBuffer { get; }

        public MappedResourceView<TData> InstanceDataView { get; private set; }

        public int InstanceCount { get; set; }

        public void Map()
        {
            if (IsMapped)
            {
                throw new InvalidOperationException("Already mapped.");
            }

            InstanceDataView = _device.Map<TData>(InstanceDataBuffer, MapMode.Write);
            IsMapped = true;
        }

        public void UnMap()
        {
            if (!IsMapped)
            {
                throw new InvalidOperationException("Not mapped.");
            }

            _device.Unmap(InstanceDataBuffer);
            InstanceDataView = default;
            IsMapped = false;
        }
    }

    public void Dispose()
    {
        ReturnAll();

        _latest = null;

        GC.SuppressFinalize(this);
    }

    ~DeviceBufferList()
    {
        Debug.Fail("Destroying device buffer list.");
    }
}