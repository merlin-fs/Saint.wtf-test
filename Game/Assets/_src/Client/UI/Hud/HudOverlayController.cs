using System.Collections.Generic;
using UnityEngine;

namespace Game.Client.UI.Hud
{
    public sealed class HudOverlayController : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private float placementHz = 20f;

        private readonly List<IHudItem> _items = new();
        private bool _enabled = true;
        private float _timer;

        public bool Enabled
        {
            get => _enabled;
            set => SetAllVisible(value);
        }

        public T Spawn<T>(T prefab, Transform parent = null) where T : MonoBehaviour, IHudItem
        {
            parent ??= transform;
            var inst = Instantiate(prefab, parent, worldPositionStays: true);
            _items.Add(inst);

            // глобальний прапорець застосовуємо одразу
            inst.SetVisible(_enabled);
            return inst;
        }

        public void Despawn(IHudItem item)
        {
            if (item == null) return;

            _items.Remove(item);
            item.Dispose();

            if (item is Component c)
                Destroy(c.gameObject);
        }

        public void SetAllVisible(bool visible)
        {
            _enabled = visible;
            foreach (var t in _items)
                t.SetVisible(visible);
        }

        public void Clear()
        {
            for (var i = _items.Count - 1; i >= 0; i--)
                Despawn(_items[i]);

            _items.Clear();
        }

        private void LateUpdate()
        {
            if (!_enabled) return;
            if (worldCamera == null) return;

            var interval = placementHz <= 0 ? 0.05f : 1f / placementHz;
            _timer += Time.deltaTime;
            if (_timer < interval) return;
            _timer = 0f;

            var ctx = new HudPlacementContext(worldCamera);

            foreach (var t in _items)
                t.UpdatePlacement(ctx);
        }

        private void OnDestroy() => Clear();
    }
}
