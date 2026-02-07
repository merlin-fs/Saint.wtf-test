using System;
using System.Collections.Generic;

namespace Game.Core.Common
{
    public sealed class ResourceCatalog : IResourceCatalog
    {
        private readonly Dictionary<ResourceId, int> _idToIndex = new();
        private readonly ResourceId[] _indexToId;
        private readonly Dictionary<ResourceId, ResourceDef> _defs = new();

        public ResourceCatalog(IReadOnlyList<ResourceDef> defs)
        {
            if (defs == null) throw new ArgumentNullException(nameof(defs));

            _indexToId = new ResourceId[defs.Count];

            for (var i = 0; i < defs.Count; i++)
            {
                var def = defs[i];
                _idToIndex[def.Id] = i;
                _indexToId[i] = def.Id;
                _defs[def.Id] = def;
            }
        }

        public int Count => _indexToId.Length;

        public bool Contains(ResourceId id) => _idToIndex.ContainsKey(id);

        public ResourceDef GetDef(ResourceId id) => _defs.TryGetValue(id, out var def) 
            ? def 
            : throw new KeyNotFoundException($"Unknown ResourceId={id}");

        public int ToIndex(ResourceId id)
            => _idToIndex.TryGetValue(id, out var idx)
                ? idx
                : throw new KeyNotFoundException($"Unknown ResourceId={id}");

        public ResourceId FromIndex(int index) => _indexToId[index];
    }
}