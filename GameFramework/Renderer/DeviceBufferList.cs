using System.Diagnostics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace GameFramework.Renderer;

/// <summary>
///     Holds many instances of <code>TData</code>. Multiple device buffers are used for storage.
/// </summary>
/// <typeparam name="TData">The data element to store.</typeparam>
public unsafe class DeviceBufferList<TData> : IDisposable where TData : unmanaged
{
    private readonly GraphicsDevice _device;
    private readonly DeviceBufferPool _pool;
    private readonly BufferUsage _usage;

    private DataBuffer? _latest;

    /// <summary>
    ///     Creates a new instance of the <code>DeviceBufferList</code> class.
    /// </summary>
    /// <param name="device">The graphics device that owns the resources.</param>
    /// <param name="pool">The buffer pool to use.</param>
    /// <param name="maxItemsPerBuffer">The number of items to store per buffer. This will control the size of the individual buffers that will be rented from the pool.</param>
    /// <param name="usage">The buffer usage, passed to the device buffers.</param>
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

    /// <summary>
    ///     The number of items stored per buffer.
    /// </summary>
    public int MaxItemsPerBuffer { get; }

    private readonly List<DataBuffer> _buffers = new();

    /// <summary>
    ///     The buffers in use. All buffers but the last one are full with instances.
    /// </summary>
    public IReadOnlyList<DataBuffer> Buffers => _buffers;

    private DataBuffer AddBuffer()
    {
        var instanceBuffer = _pool.Get((uint)(sizeof(TData) * MaxItemsPerBuffer), _usage);

        var result = new DataBuffer(_device, instanceBuffer);

        result.PrepareWriting();

        _buffers.Add(result);

        return result;
    }
    
    /// <summary>
    ///     Gets a write buffer with space for at least one instance.
    /// </summary>
    /// <returns>A data buffer, with at least one instance of free space available.</returns>
    /// <exception cref="InvalidOperationException">Thrown if an internal state was broken.</exception>
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

    /// <summary>
    ///     Adds an instance to the buffer.
    /// </summary>
    /// <param name="instance">The instance to add.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ref TData instance)
    {
        var buffer = GetWriteBuffer();
        var view = buffer.InstanceDataView;
        view[buffer.InstanceCount++] = instance;
    }

    /// <summary>
    ///     Returns all used buffers to the pool.
    /// </summary>
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

    /// <summary>
    ///     Clears all data stored in this <code>DeviceBufferList</code>.
    /// </summary>
    public void Clear()
    {
        ReturnAll();

        _latest = AddBuffer();
    }

    /// <summary>
    ///     Represents a mapped device buffer with instance count tracking.
    /// </summary>
    public sealed class DataBuffer
    {
        private readonly GraphicsDevice _device;

        /// <summary>
        ///     True if this device is mapped to a view. Otherwise, false.
        /// </summary>
        public bool IsMapped { get; private set; }

        public DataBuffer(GraphicsDevice device, DeviceBuffer instanceDataBuffer)
        {
            _device = device;
            InstanceDataBuffer = instanceDataBuffer;
        }

        /// <summary>
        ///     Prepares this buffer for drawing. Internally, this un-maps the buffer.
        /// </summary>
        public void PrepareDraw()
        {
            if (IsMapped)
            {
                UnMap();
            }
        }

        /// <summary>
        ///     Prepares this buffer for writing. Internally, this maps the buffer.
        /// </summary>
        public void PrepareWriting()
        {
            if (!IsMapped)
            {
                Map();
            }
        }

        /// <summary>
        ///     Gets the underlying device buffer.
        /// </summary>
        public DeviceBuffer InstanceDataBuffer { get; }

        /// <summary>
        ///     Gets the mapped view. Only valid if <see cref="IsMapped"/> is true.
        /// </summary>
        public MappedResourceView<TData> InstanceDataView { get; private set; }

        /// <summary>
        ///     Gets the number of instances stored in this buffer.
        /// </summary>
        public int InstanceCount { get; set; }

        /// <summary>
        ///     Maps the device buffer, making access via <see cref="InstanceDataView"/> possible.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this buffer is already mapped.</exception>
        public void Map()
        {
            if (IsMapped)
            {
                throw new InvalidOperationException("Already mapped.");
            }

            InstanceDataView = _device.Map<TData>(InstanceDataBuffer, MapMode.Write);
            IsMapped = true;
        }

        /// <summary>
        ///     Un-maps the device buffer, invalidating the <see cref="InstanceDataView"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this buffer is not mapped.</exception>
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

    /// <summary>
    ///     Returns all resources to the pool and marks this DeviceBufferList as invalid.
    /// </summary>
    public void Dispose()
    {
        ReturnAll();

        _latest = null;

        GC.SuppressFinalize(this);
    }

    ~DeviceBufferList()
    {
        // Dispose should always be called explicitly.

        Debug.Fail("Destroying device buffer list.");
    }
}