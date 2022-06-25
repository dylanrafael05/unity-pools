using System;
using UnityEngine;

namespace UnityPool
{
    public interface IPoolableBehaviour
    {
        void InitializeWith(object data);

        IPoolManager Pool { get; set; }
        int PoolIndex { get; set; }
        bool PoolActive { get; set; }

        GameObject GameObject { get; }

        void ReleaseThis();
    }
    public abstract class PoolableBehaviour : MonoBehaviour, IPoolableBehaviour
    {
        public IPoolManager Pool { get; private set; }
        public int PoolIndex { get; private set; }
        public bool PoolActive { get; private set; }

        IPoolManager IPoolableBehaviour.Pool { get => Pool; set => Pool = value; }
        int IPoolableBehaviour.PoolIndex { get => PoolIndex; set => PoolIndex = value; }
        bool IPoolableBehaviour.PoolActive { get => PoolActive; set => PoolActive = value; }
        GameObject IPoolableBehaviour.GameObject { get => gameObject; }

        public void ReleaseThis()
        {
            Pool.Release(this);
        }

        public abstract void InitializeWith(object data);
    }
    public abstract class PoolableBehaviour<T> : PoolableBehaviour
    {
        public sealed override void InitializeWith(object data)
            => InitializeWith(data is T t ? t : throw new InvalidOperationException($"Cannot initialize {GetType()} with {data.GetType()}."));
        public abstract void InitializeWith(T data);
    }
}
