using System;
using FTg.Common.Observables;
using Game.Client.Resources;
using Game.Core.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Client.UI.Hud
{
    public sealed class StorageHudItem : HudItemBase
    {
        [SerializeField] private ResourceLibrary resourceLibrary;
        [SerializeField] private TextMeshProUGUI capacityText;
        [SerializeField] private Image fillBar;
        [SerializeField] private Image[] resIcons;

        private IResourceContainer _container;
        private IDisposable _subChanged;

        private int _lastTotal = int.MinValue;
        private int _lastCap = int.MinValue;

        public void Bind(IResourceContainer container, IResourceTransferFilter filter = null)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));

            UpdateUi(force: true);

            _subChanged?.Dispose();
            _subChanged = _container.Changed.Subscribe(_ => UpdateUi(force: false));

            foreach (var icon in resIcons)
                icon.gameObject.SetActive(false);
            
            if (filter == null) return;
            var idx = 0;
            foreach (var resDef in resourceLibrary.All)
            {
                if (!filter.Allows(resDef.Id)) continue;
                resourceLibrary.TryGet(resDef.Id, out var entry);
                resIcons[idx].color = entry.uiColor;
                resIcons[idx].gameObject.SetActive(true);
                idx++;
            }
        }

        public override void Dispose()
        {
            _subChanged?.Dispose();
            _subChanged = null;
        }

        protected override void OnVisibilityChanged(bool visible)
        {
            // Якщо HUD показали після паузи/вимкнення — форсимо актуальні дані
            if (visible && _container != null)
                UpdateUi(force: true);
        }

        private void UpdateUi(bool force)
        {
            var total = _container.Total;
            var cap = _container.Capacity;

            if (!force && total == _lastTotal && cap == _lastCap)
                return;

            _lastTotal = total;
            _lastCap = cap;

            if (capacityText != null)
                capacityText.text = $"{total}/{cap}";

            if (fillBar != null)
                fillBar.fillAmount = cap > 0 ? Mathf.Clamp01((float)total / cap) : 1f;
        }
    }
}
