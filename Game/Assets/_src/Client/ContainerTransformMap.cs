using System.Collections.Generic;
using Game.Client.Common;
using Game.Core.Economy;
using UnityEngine;

namespace Game.Client
{
    /// <summary>
    /// Відповідає за зберігання відповідності між контейнерами ресурсів та їх позиціями у світі.
    /// Це потрібно для того, щоб знати, де візуалізувати ресурси, які знаходяться в певному контейнері.
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