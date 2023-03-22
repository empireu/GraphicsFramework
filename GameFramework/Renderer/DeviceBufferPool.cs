using System.Collections.Concurrent;
using System.Diagnostics;
using Veldrid;

namespace GameFramework.Renderer;

/// <summary>
///     This is a bucket pool of <see cref="DeviceBuffer"/>s. It goes up to 2^30 bytes.
/// </summary>
public sealed class DeviceBufferPool
{
    public const byte MaxSizeExponent = 30;
    public const byte MinSizeExponent = 0;

    private int _inUse;
    private long _inUseSize;

    /// <summary>
    ///     Only buffers with these usages are valid for pooling.
    /// </summary>
    public static readonly IReadOnlyList<BufferUsage> ValidUsages = new[]
    {
        BufferUsage.Dynamic | BufferUsage.IndexBuffer,
        BufferUsage.Dynamic | BufferUsage.VertexBuffer,
        BufferUsage.IndexBuffer,
        BufferUsage.VertexBuffer,
    };

    // We hold a bucket pool per usage:
    private readonly Dictionary<BufferUsage, Bucket[]> _pools = new();

    /// <summary>
    ///     Gets the number of buffers in use.
    /// </summary>
    public int InUse => _inUse;

    /// <summary>
    ///     Gets the total size of the memory space in use (excluding overhead), in bytes.
    /// </summary>
    public long InUseSize => _inUseSize;

    public DeviceBufferPool(GraphicsDevice device)
    {
        foreach (var bufferUsage in ValidUsages)
        {
            var buckets = Enumerable
                .Range(MinSizeExponent, MaxSizeExponent + 1)
                .Select(exponent => new Bucket(bufferUsage, device, (byte)exponent))
                .ToArray();

            _pools.Add(bufferUsage, buckets);
        }
    }

    /// <summary>
    ///     Gets the bucket for the specified size and usage.
    /// </summary>
    /// <param name="usage">The desired usage of the buffer.</param>
    /// <param name="size">The minimum size of the resulting buffer, in bytes.</param>
    /// <returns>A buffer with the specified usage and a power-of-two size.</returns>
    /// <exception cref="ArgumentException">Thrown if the requested size is larger than the pool's maximum.</exception>
    private Bucket GetBucket(BufferUsage usage, uint size)
    {
        var exponent = ComputeSizeExponent(size);

        if (exponent > MaxSizeExponent)
        {
            throw new ArgumentException($"Size {size} is larger than max size ({SizeFromExponent(MaxSizeExponent)})");
        }

        if (!_pools.TryGetValue(usage, out var buckets))
        {
            throw new ArgumentException($"Cannot use a pool buffer with a usage of {usage}.");
        }

        return buckets[exponent];
    }

    /// <summary>
    ///     Gets a device buffer from this pool.
    /// </summary>
    /// <param name="size">The minimum size of the buffer, in bytes.</param>
    /// <param name="usage">The desired buffer usage.</param>
    /// <returns>A buffer with the specified usage and a power-of-two size.</returns>
    public DeviceBuffer Get(uint size, BufferUsage usage)
    {
        var bucket = GetBucket(usage, size);
        var buffer = bucket.GetBuffer();

        Interlocked.Increment(ref _inUse);
        Interlocked.Add(ref _inUseSize, buffer.SizeInBytes);

        return buffer;
    }

    /// <summary>
    ///     Returns a buffer to this pool.
    /// </summary>
    /// <param name="buffer">The buffer to return. It must originate from this pool.</param>
    public void Return(DeviceBuffer buffer)
    {
        var bucket = GetBucket(buffer.Usage, buffer.SizeInBytes);
        bucket.Return(buffer);

        Interlocked.Decrement(ref _inUse);
        Interlocked.Add(ref _inUseSize, -buffer.SizeInBytes);
    }

    /// <summary>
    ///     Computes the size in bytes from a power of two.
    /// </summary>
    /// <param name="exponent"></param>
    /// <returns></returns>
    public static uint SizeFromExponent(byte exponent)
    {
        return 1u << exponent;
    }

    /// <summary>
    ///     Computes the power of two from the specified size.
    /// </summary>
    /// <param name="size">The size, in bytes.</param>
    /// <returns>An exponent for base two. The resulting power-of-two number is larger or equal to <code>size</code></returns>
    public static byte ComputeSizeExponent(uint size)
    {
        return (byte)Math.Ceiling(Math.Log2(size));
    }

    /// <summary>
    ///     The bucket holds the actual pool of buffers. It is created per usage and size.
    /// </summary>
    private sealed class Bucket
    {
        public BufferUsage Usage { get; }

        public byte SizeExponent { get; }

        public uint SizeBytes => SizeFromExponent(SizeExponent);

        private readonly GraphicsDevice _device;

        private readonly ConcurrentBag<DeviceBuffer> _buffers = new();

        public Bucket(BufferUsage usage, GraphicsDevice device, byte sizeExponent)
        {
            Usage = usage;
            SizeExponent = sizeExponent;
            _device = device;
        }

        /// <summary>
        ///     Gets or creates a buffer.
        /// </summary>
        /// <returns>A <see cref="DeviceBuffer"/> with the size defined in this bucket.</returns>
        public DeviceBuffer GetBuffer()
        {
            if (!_buffers.TryTake(out var buffer))
            {
                buffer = CreateBuffer();
            }

            return buffer;
        }

        /// <summary>
        ///     Returns a <see cref="DeviceBuffer"/> to the bucket.
        /// </summary>
        /// <param name="buffer">The buffer to return. It must originate from this bucket.</param>
        /// <exception cref="ArgumentException">Thrown if the buffer properties do not match what is imposed by this bucket.</exception>
        public void Return(DeviceBuffer buffer)
        {
            if (buffer.Usage != Usage)
            {
                throw new ArgumentException($"Cannot add a buffer with usage {buffer.Usage} to a bucket with {Usage}");
            }

            if (buffer.SizeInBytes != SizeBytes)
            {
                throw new ArgumentException(
                    $"Cannot add a buffer with a size of {buffer.SizeInBytes} bytes to a bucket with {SizeBytes} bytes.");
            }

            _buffers.Add(buffer);
        }

        /// <summary>
        ///     Creates a fresh buffer with the properties of this pool.
        /// </summary>
        /// <returns></returns>
        private DeviceBuffer CreateBuffer()
        {
            var description = new BufferDescription(SizeBytes, Usage);

            return _device.ResourceFactory.CreateBuffer(description);
        }
    }
}