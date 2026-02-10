using System;
using FTg.Common.Observables;
using Game.Core.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Client.UI.Hud
{
    public sealed class StorageHudItem : HudItemBase
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI capacityText;
        [SerializeField] private Image fillBar;

        private IResourceContainer _container = default!;
        private IDisposable _subChanged;

        private int _lastTotal = int.MinValue;
        private int _lastCap = int.MinValue;

        public void Bind(IResourceContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));

            UpdateUi(force: true);

            _subChanged?.Dispose();
            _subChanged = _container.Changed.Subscribe(_ => UpdateUi(force: false));
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
            int total = _container.Total;
            int cap = _container.Capacity;

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
