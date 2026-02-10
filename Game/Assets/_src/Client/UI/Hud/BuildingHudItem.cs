using System;
using System.Text;
using Game.Core.Common;
using Game.Core.Production;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Client.UI.Hud
{
    public sealed class BuildingHudItem : HudItemBase
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI stateText;
        [SerializeField] private GameObject progressRoot;
        [SerializeField] private Image progressFill;

        [Header("Update policy")]
        [Tooltip("Як часто перевіряти Phase/StopReason (сек).")]
        [SerializeField] private float statePollInterval = 0.2f; // 5 Hz

        [Tooltip("Як часто оновлювати прогрес під час Producing (сек).")]
        [SerializeField] private float progressInterval = 0.05f; // 20 Hz

        [Tooltip("Крок для оновлення тексту прогресу (0.1 = 10%).")]
        [SerializeField] private float progressTextStep = 0.1f;

        private BuildingModel _building = default!;
        private IResourceCatalog _catalog = default!;

        private float _stateTimer;
        private float _progressTimer;

        private BuildingStatus _lastPhase;
        private StopReason _lastStop;
        private int _lastProgressStep = -1;

        public void Bind(BuildingModel building, IResourceCatalog catalog)
        {
            _building = building ?? throw new ArgumentNullException(nameof(building));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));

            if (titleText != null)
                titleText.text = $"OUT: {ResName(_building.Recipe.Output)}";

            _lastProgressStep = -1;
            _stateTimer = 0f;
            _progressTimer = 0f;

            UpdateState(force: true);
        }

        private void Update()
        {
            if (!IsVisible) return;

            _stateTimer += Time.deltaTime;
            if (_stateTimer >= statePollInterval)
            {
                _stateTimer = 0f;
                UpdateState(force: false);
            }

            if (_building.Status == BuildingStatus.Producing)
            {
                _progressTimer += Time.deltaTime;
                if (_progressTimer >= progressInterval)
                {
                    _progressTimer = 0f;
                    UpdateProducingProgress();
                }
            }
        }

        protected override void OnVisibilityChanged(bool visible)
        {
            if (!visible) return;
            // повернули HUD — оновимо одразу, без очікування таймерів
            _stateTimer = 0f;
            _progressTimer = 0f;
            UpdateState(force: true);
        }

        private void UpdateState(bool force)
        {
            var phase = _building.Status;
            var stop = _building.StopReason;

            if (!force && phase == _lastPhase && stop == _lastStop)
                return;

            _lastPhase = phase;
            _lastStop = stop;
            _lastProgressStep = -1;

            if (progressRoot != null)
                progressRoot.SetActive(phase == BuildingStatus.Producing);

            if (stateText != null)
                stateText.text = FormatStateLine(phase, stop);

            // швидкий апдейт fill
            if (phase == BuildingStatus.Producing)
                UpdateProducingProgress();
            else if (progressFill != null)
                progressFill.fillAmount = 0f;
        }

        private void UpdateProducingProgress()
        {
            float p = Mathf.Clamp01(_building.ProductionProgress);

            if (progressFill != null)
                progressFill.fillAmount = p;

            int step = progressTextStep <= 0f
                ? Mathf.FloorToInt(p * 10f)
                : Mathf.FloorToInt(p / progressTextStep);

            if (step == _lastProgressStep) return;
            _lastProgressStep = step;

            if (stateText != null)
                stateText.text = $"PRODUCING {p:P0}";
        }

        private string FormatStateLine(BuildingStatus phase, StopReason stop)
        {
            return phase switch
            {
                BuildingStatus.Idle => "IDLE",
                BuildingStatus.PullInputs => "IN...",
                BuildingStatus.Producing => $"PRODUCING {_building.ProductionProgress:P0}",
                BuildingStatus.PushOutput => "OUT...",
                BuildingStatus.Stopped => FormatStopped(stop),
                _ => phase.ToString()
            };
        }

        private string FormatStopped(StopReason stop)
        {
            return stop switch
            {
                StopReason.NoInput => "STOP: No input" + MissingInputsDetail(),
                StopReason.OutputFull => $"STOP: Output full ({_building.OutputStorage.Total}/{_building.OutputStorage.Capacity})",
                StopReason.TransferBlocked => "STOP: Transfer blocked",
                _ => "STOP"
            };
        }

        private string MissingInputsDetail()
        {
            var sb = new StringBuilder(64);
            var any = false;

            _building.Recipe.ForEachInput((rid, need) =>
            {
                int have = _building.InputStorage.Count(rid);
                if (have >= need) return;

                if (!any) { sb.Append(" ("); any = true; }
                else sb.Append(", ");

                sb.Append(ResName(rid)).Append(' ')
                  .Append(have).Append('/').Append(need);
            });

            if (any) sb.Append(')');
            return sb.ToString();
        }

        private string ResName(ResourceId id)
        {
            try { return _catalog.GetDef(id).Name; }
            catch { return $"Res#{id.Value}"; }
        }
    }
}
