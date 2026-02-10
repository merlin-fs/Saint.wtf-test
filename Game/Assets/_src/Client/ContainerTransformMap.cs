using System.Collections.Generic;
using Game.Client.Common;
using Game.Core.Economy;
using UnityEngine;

namespace Game.Client
{
    /// <summary>
    /// Maps Core containers (IResourceContainer instances) to world points (Transform).
    /// </summary>
    public sealed class ContainerTransformMap
    {
        private readonly Dictionary<IResourceContainer, Transform> _map = new(ReferenceEqualityComparer<IResourceContainer>.Instance);

        public void Register(IResourceContainer container, Transform point)
        {
            _map[container] = point;
        }

        public bool TryGet(IResourceContainer container, out Transform point) => _map.TryGetValue(container, out point);

        public Vector3 GetPosition(IResourceContainer container)
        {
            return !_map.TryGetValue(container, out var t) 
                ? throw new KeyNotFoundException($"No Transform registered for container: {container.GetType().Name}") 
                : t.position;
        }
    }
}