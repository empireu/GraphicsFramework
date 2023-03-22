using System.Collections.Concurrent;
using GameFramework.Utilities;
using Microsoft.Extensions.ObjectPool;

namespace GameFramework.Renderer.Batch;

/// <summary>
///     The quad batches are expensive to construct. The <see cref="QuadBatchPool"/> pools batches that are reused to reduce resource consumption.
/// </summary>
public sealed class QuadBatchPool
{
    private class Policy : IPooledObjectPolicy<QuadBatch>
    {
        private readonly GameApplication _application;
        private readonly Action<QuadBatch>? _setup;

        public Policy(GameApplication application, Action<QuadBatch>? setup = null)
        {
            _application = application;
            _setup = setup;
        }

        public QuadBatch Create()
        {
            var result = new QuadBatch(_application);
            _setup?.Invoke(result);

            return result;
        }

        public bool Return(QuadBatch obj)
        {
            obj.Clear();

            return true;
        }
    }

    private readonly ConcurrentDictionary<QuadBatch, int> _rented = new();

    /// <summary>
    ///     Gets the number of batches in use.
    /// </summary>
    public int InUse => _rented.Count;

    public QuadBatchPool(GameApplication application, Action<QuadBatch>? setup = null)
    {
        _pool = new DefaultObjectPool<QuadBatch>(new Policy(application, setup));  
    }

    private readonly ObjectPool<QuadBatch> _pool;

    /// <summary>
    ///     Gets a batch from this pool. If none are available, a new instance is created.
    /// </summary>
    /// <returns>A quad batch that will be returned at a later point.</returns>
    public QuadBatch Get()
    {
        var batch = _pool.Get();

        Assert.IsTrue(_rented.TryAdd(batch, _rented.Count));

        return batch;
    }

    /// <returns>A <see cref="RentedItem{T}"/> provider around <see cref="Get"/></returns>
    public RentedItem<QuadBatch> GetProvider()
    {
        var batch = Get();

        return new RentedItem<QuadBatch>(batch, Return);
    }

    /// <summary>
    ///     Returns a batch to this pool. It must be a batch that was obtained as a result of calling <see cref="Get"/> or <see cref="GetProvider"/>.
    /// </summary>
    /// <param name="batch">The batch to return, obtained from this pool.</param>
    /// <exception cref="InvalidOperationException">Thrown if the returned batch did not originate from this pool.</exception>
    public void Return(QuadBatch batch)
    {
        if (!_rented.TryRemove(batch, out _))
        {
            throw new InvalidOperationException("Cannot return batch that was not rented from this pool!");
        }

        _pool.Return(batch);
    }
}