using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPool
{
    public interface IPoolManager
    {
        IReadOnlyList<object> ReleaseBuffer { get; }
        void StartReleaseBuffer();
        void Release(IPoolableBehaviour behaviour);
    }
    public class PoolManager<T> : IPoolManager, IEnumerable<T> where T : class, IPoolableBehaviour
    {
        private List<T> behaviours;
        private HashSet<int> freeIndices;
        private GameObject prefab;

        private bool shouldReleaseByBuffer;
        private List<T> releaseBuffer;

        IReadOnlyList<object> IPoolManager.ReleaseBuffer => releaseBuffer;
        public IReadOnlyList<T> ReleaseBuffer => releaseBuffer;

        public int Count => behaviours.Count;

        public PoolManager(GameObject prefab)
        {
            Debug.Assert(prefab.GetComponent<T>() != null, $"Pool behaviour must have specified component {typeof(T).Name}!");
            this.prefab = prefab;

            behaviours = new List<T>();
            freeIndices = new HashSet<int>();

            releaseBuffer = new List<T>();
            shouldReleaseByBuffer = false;
        }

        public T Create(object data)
        {
            if(freeIndices.Count == 0)
            {
                SetCapacity(Count + 1);
            }

            var index = freeIndices.First();
            freeIndices.Remove(index);

            behaviours[index].InitializeWith(data);

            behaviours[index].GameObject.SetActive(true);
            behaviours[index].PoolActive = true;

            return behaviours[index];
        }

        public void SetCapacity(int capacity)
        {
            if(behaviours.Count > capacity)
            {
                return;
            }

            for(int i = behaviours.Count; i < capacity; i++)
            {
                var newObj = GameObject.Instantiate(prefab);
                var newBeh = newObj.GetComponent<T>();

                newBeh.Pool = this;
                newBeh.PoolIndex = i;
                newBeh.PoolActive = false;
                newBeh.GameObject.SetActive(false);

                freeIndices.Add(i);

                behaviours.Add(newBeh);
            }
        }

        void IPoolManager.Release(IPoolableBehaviour behaviour)
            => Release(behaviour as T);
        public void Release(T behaviour)
        {
            if(behaviour == null || behaviour.Pool != this)
                throw new InvalidOperationException("Cannot release a behaviour to a pool it is not in.");

            if (shouldReleaseByBuffer)
            {
                releaseBuffer.Add(behaviour);
            }
            else
            {
                freeIndices.Add(behaviour.PoolIndex);
            }

            behaviour.GameObject.SetActive(false);
            behaviour.PoolActive = false;
        }

        public void ReleaseAll()
        {
            foreach(var b : behaviours)
            {
                b.GameObject.SetActive(false);
                b.PoolActive = false;
            }
            
            behaviours.Clear();
            freeIndices.Clear();
            releaseBuffer.Clear();
        }

        public void StartReleaseBuffer()
        {
            shouldReleaseByBuffer = true;
        }

        public void ReleaseFromBuffer()
        {
            shouldReleaseByBuffer = false;

            foreach(var item in releaseBuffer)
            {
                Release(item);
            }

            releaseBuffer.Clear();
        }

        public IEnumerator<T> GetEnumerator()
            => behaviours.Where(t => t.PoolActive).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => behaviours.Where(t => t.PoolActive).GetEnumerator();
    }
}
