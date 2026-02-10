using System.Collections.Generic;
using Game.Client.Resources;
using UnityEngine;

namespace Game.Client.Common
{
    public sealed class ResourceBlockPool
    {
        private readonly Queue<ResourceBlockView> _pool = new();
        private readonly ResourceBlockView _prefab;
        private readonly Transform _root;

        public ResourceBlockPool(ResourceBlockView prefab, Transform root)
        {
            _prefab = prefab;
            _root = root;
        }

        public ResourceBlockView Rent()
        {
            if (_pool.Count > 0)
            {
                var v = _pool.Dequeue();
                return v;
            }

            var newObj = Object.Instantiate(_prefab, _root);
            newObj.gameObject.SetActive(false);
            return newObj;
        }

        public void Return(ResourceBlockView view)
        {
            view.gameObject.SetActive(false);
            view.transform.SetParent(_root, false);
            _pool.Enqueue(view);
        }
    }
}