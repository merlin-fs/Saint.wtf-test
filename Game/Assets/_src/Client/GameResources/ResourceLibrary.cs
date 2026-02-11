
using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Common;
using UnityEngine;

namespace Game.Client.Resources
{
    /// <summary>
    /// Бібліотека ресурсів. Зберігає інформацію про всі ресурси, які є в грі,
    /// та їх візуальне представлення (матеріал, колір для UI, масштаб).
    /// </summary>
    [CreateAssetMenu(menuName = "Game/ResourceLibrary")]
    public sealed class ResourceLibrary : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public ResourceDef resourceDef;         // Resource
            public Material material;               // або Color, або prefab
            public Color uiColor;
            public Vector3 scale = Vector3.one;     
        }

        [SerializeField] private Entry[] entries = Array.Empty<Entry>();

        public IEnumerable<ResourceDef> All => entries.Select(e => e.resourceDef).ToArray();
        
        public bool TryGet(ResourceId id, out Entry entry)
        {
            if (id.Value >= 1 && id.Value < entries.Length+1)
            {
                entry = entries[id.Value-1];
                return true;
            }
            entry = null;
            return false;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Оновлюємо ID на основі індексу в масиві
            for (var i = 0; i < entries.Length; i++)
            {
                entries[i].resourceDef.Id = new ResourceId(i+1);
            }
        }
#endif        
    }
}