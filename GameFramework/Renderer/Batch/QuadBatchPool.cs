using System.Collections.Concurrent;
using GameFramework.Utilities;
using Microsoft.Extensions.ObjectPool;

namespace GameFramework.Renderer.Batch;

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

    public int InUse => _rented.Count;

    public QuadBatchPool(GameApplication application, Action<QuadBatch>? setup = null)
    {
        _pool = new DefaultObjectPool<QuadBatch>(new Policy(application, setup));  
    }

    private readonly ObjectPool<QuadBatch> _pool;

    public QuadBatch Get()
    {
        var batch = _pool.Get();

        Assert.IsTrue(_rented.TryAdd(batch, _rented.Count));

        return batch;
    }

    public RentedItem<QuadBatch> GetProvider()
    {
        var batch = Get();
        return new RentedItem<QuadBatch>(batch, Return);
    }

    public void Return(QuadBatch batch)
    {
        if (!_rented.TryRemove(batch, out var id))
        {
            throw new InvalidOperationException("Cannot return batch that was not rented from this pool!");
        }

        _pool.Return(batch);
    }
}