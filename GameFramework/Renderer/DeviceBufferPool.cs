using System.Collections.Concurrent;
using System.Diagnostics;
using Veldrid;

namespace GameFramework.Renderer;

public sealed class DeviceBufferPool
{
    public const byte MaxSizeExponent = 30;
    public const byte MinSizeExponent = 0;

    private int _inUse;
    private long _inUseSize;

    public static readonly IReadOnlyList<BufferUsage> ValidUsages = new[]
    {
        BufferUsage.Dynamic | BufferUsage.IndexBuffer,
        BufferUsage.Dynamic | BufferUsage.VertexBuffer,
        BufferUsage.IndexBuffer,
        BufferUsage.VertexBuffer,
    };

    private readonly Dictionary<BufferUsage, Bucket[]> _pools = new();

    public int InUse => _inUse;

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

    public DeviceBuffer Get(uint size, BufferUsage usage)
    {
        var bucket = GetBucket(usage, size);
        var buffer = bucket.GetBuffer();

        Interlocked.Increment(ref _inUse);
        Interlocked.Add(ref _inUseSize, buffer.SizeInBytes);

        return buffer;
    }

    public void Return(DeviceBuffer buffer)
    {
        var bucket = GetBucket(buffer.Usage, buffer.SizeInBytes);
        bucket.Return(buffer);

        Interlocked.Decrement(ref _inUse);
        Interlocked.Add(ref _inUseSize, -buffer.SizeInBytes);
    }

    public static uint SizeFromExponent(byte exponent)
    {
        return 1u << exponent;
    }

    public static byte ComputeSizeExponent(uint size)
    {
        return (byte)Math.Ceiling(Math.Log2(size));
    }

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

        public DeviceBuffer GetBuffer()
        {
            if (!_buffers.TryTake(out var buffer))
            {
                buffer = CreateBuffer();
            }

            return buffer;
        }

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

        private DeviceBuffer CreateBuffer()
        {
            var description = new BufferDescription(SizeBytes, Usage);

            return _device.ResourceFactory.CreateBuffer(description);
        }
    }
}