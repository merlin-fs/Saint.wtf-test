using System;
using System.Collections.Generic;
using System.Text;
using FTg.Common.Observables;
using Game.Client.Common;
using Game.Core.Common;
using Game.Core.Economy;
using Game.Core.Production;
using Game.Core.Transfers;
using UnityEngine;

namespace Game.Client.App.Debugging
{
    /// <summary>
    /// Система для логування внутрішніх подій GameComposition (трансферів, контейнерів, будівель).
    /// auto-gen gpt
    /// </summary>
    public sealed class GameCompositionDebug : ITickSystem, IDisposable
    {
        public sealed class Settings
        {
            // Transfers
            public bool LogTransferStarted = true;
            public bool LogTransferFinished = true;

            // Progress може бути дуже шумним
            public bool LogTransferProgress = false;
            public float TransferProgressStep = 0.25f; // 25% кроками (якщо LogTransferProgress=true)

            // Containers
            public bool LogContainerChanged = true;
            public bool LogContainerSummaryOnChange = true;
            public float MinSecondsBetweenContainerLogs = 0.0f; // 0 = без тротлінгу

            // Buildings
            public bool LogBuildingStateChanges = true;
            public bool LogBuildingProducingProgress = false;
            public float BuildingProgressStep = 0.25f;

            // Загальне
            public string Prefix = "[DBG]";
        }

        private readonly Settings _s;
        private readonly GameComposition _comp;
        private readonly IResourceCatalog _catalog;
        private readonly ITransferStream _stream;

        // subscriptions
        private readonly CompositeDisposable _disposables = new ();
        private readonly List<IDisposable> _containerSubs = new();

        // naming
        private readonly Dictionary<IResourceContainer, string> _containerNames;
        private readonly Dictionary<int, string> _buildingNames = new();

        // throttling
        private readonly Dictionary<IResourceContainer, float> _lastContainerLogTime = new();

        // building last state
        private readonly Dictionary<int, BuildingStatus> _lastPhase = new();
        private readonly Dictionary<int, StopReason> _lastStop = new();
        private readonly Dictionary<int, int> _lastProdStep = new();

        // transfer progress throttling
        private readonly Dictionary<int, int> _lastTransferStep = new();

        public GameCompositionDebug(GameComposition composition, Settings settings = null)
        {
            _comp = composition ?? throw new ArgumentNullException(nameof(composition));
            _s = settings ?? new Settings();

            _catalog = composition.Catalog;
            _stream = composition.Scheduler; // TransferScheduler implements ITransferStream

            _containerNames = new Dictionary<IResourceContainer, string>(ReferenceEqualityComparer<IResourceContainer>.Instance);

            RegisterBuildings(composition.Buildings);
            SubscribeTransfers();
            SubscribeContainers();
        }

        public void Dispose()
        {
            _disposables.Dispose();
            foreach (var t in _containerSubs)
                t.Dispose();
            _containerSubs.Clear();
        }

        public void Tick(float dt)
        {
            if (!_s.LogBuildingStateChanges && !_s.LogBuildingProducingProgress)
                return;

            foreach (var b in _comp.Buildings)
            {
                var bid = b.Id.Value;

                // phase/stop reason change
                if (_s.LogBuildingStateChanges)
                {
                    _lastPhase.TryGetValue(bid, out var prevPhase);
                    _lastStop.TryGetValue(bid, out var prevStop);

                    if (prevPhase != b.Status || prevStop != b.StopReason)
                    {
                        _lastPhase[bid] = b.Status;
                        _lastStop[bid] = b.StopReason;

                        Debug.Log($"{_s.Prefix} {BuildingName(b)} state => {b.Status} stop={b.StopReason}");
                    }
                }

                // producing progress step logging
                if (!_s.LogBuildingProducingProgress || b.Status != BuildingStatus.Producing) continue;
                var step = StepOf(b.ProductionProgress, _s.BuildingProgressStep);
                if (_lastProdStep.TryGetValue(bid, out var prev) && prev == step) continue;
                _lastProdStep[bid] = step;
                Debug.Log($"{_s.Prefix} {BuildingName(b)} producing {b.ProductionProgress:P0}");
            }
        }

        // ---------------- subscriptions ----------------

        private void SubscribeTransfers()
        {
            _stream.Started.Subscribe(OnTransferStarted).AddTo(_disposables);
            _stream.Finished.Subscribe(OnTransferFinished).AddTo(_disposables);
            if (_s.LogTransferProgress)
                _stream.Progress.Subscribe(OnTransferProgress).AddTo(_disposables);
        }

        private void SubscribeContainers()
        {
            if (!_s.LogContainerChanged)
                return;

            foreach (var b in _comp.Buildings)
            {
                SubscribeContainer(b.InputStorage);
                SubscribeContainer(b.OutputStorage);
                SubscribeContainer(b.InPort);
                SubscribeContainer(b.OutPort);
            }

            // Якщо додаси Player.Inventory у GameComposition — тут можна підписати і його.
            // SubscribeContainer(_comp.Player.Inventory);
        }

        private void SubscribeContainer(IResourceContainer c)
        {
            // Changed: IObservable<Unit>
            var sub = c.Changed.Subscribe(_ => OnContainerChanged(c));
            _containerSubs.Add(sub);
        }

        // ---------------- handlers ----------------

        private void OnTransferStarted(TransferStarted e)
        {
            if (!_s.LogTransferStarted)
                return;

            var res = ResName(e.Resource);
            var src = ContainerName(e.Source);
            var dst = ContainerName(e.Destination);

            Debug.Log($"{_s.Prefix} [T#{e.Id.Value}] START {res} {src} -> {dst} dur={e.DurationSeconds:0.###} tag={e.Tag ?? "null"}");
        }

        private void OnTransferProgress(TransferProgress e)
        {
            // Step throttling (25% / etc)
            int step = StepOf(e.Progress, _s.TransferProgressStep);

            if (_lastTransferStep.TryGetValue(e.Id.Value, out var prev) && prev == step)
                return;

            _lastTransferStep[e.Id.Value] = step;

            Debug.Log($"{_s.Prefix} [T#{e.Id.Value}] PROGRESS {e.Progress:P0}");
        }

        private void OnTransferFinished(TransferFinished e)
        {
            if (_s.LogTransferProgress)
                _lastTransferStep.Remove(e.Id.Value);

            if (!_s.LogTransferFinished)
                return;

            Debug.Log($"{_s.Prefix} [T#{e.Id.Value}] FINISH status={e.Status}");
        }

        private void OnContainerChanged(IResourceContainer c)
        {
            if (!_s.LogContainerChanged)
                return;

            // throttle
            float now = Time.time;
            if (_s.MinSecondsBetweenContainerLogs > 0f)
            {
                _lastContainerLogTime.TryGetValue(c, out float last);
                if (now - last < _s.MinSecondsBetweenContainerLogs)
                    return;
                _lastContainerLogTime[c] = now;
            }

            if (_s.LogContainerSummaryOnChange)
                Debug.Log($"{_s.Prefix} {DumpContainer(c)}");
            else
                Debug.Log($"{_s.Prefix} {ContainerName(c)} changed");
        }

        // ---------------- naming & formatting ----------------

        private void RegisterBuildings(IReadOnlyList<BuildingModel> buildings)
        {
            for (int i = 0; i < buildings.Count; i++)
            {
                var b = buildings[i];
                _buildingNames[b.Id.Value] = $"B{b.Id.Value}({RecipeName(b.Recipe)})";

                // Container names (use StorageModel.DebugName if available)
                RegisterContainer(b.InputStorage,  $"B{b.Id.Value}.Input");
                RegisterContainer(b.OutputStorage, $"B{b.Id.Value}.Output");
                RegisterContainer(b.InPort,        $"B{b.Id.Value}.InPort");
                RegisterContainer(b.OutPort,       $"B{b.Id.Value}.OutPort");

                // init last states so first tick won't necessarily spam (optional)
                _lastPhase[b.Id.Value] = b.Status;
                _lastStop[b.Id.Value] = b.StopReason;
                _lastProdStep[b.Id.Value] = -1;
            }
        }

        private void RegisterContainer(IResourceContainer c, string fallbackName)
        {
            var name = fallbackName;

            // If it's our StorageModel, prefer DebugName
            if (c is StorageModel sm && !string.IsNullOrEmpty(sm.DebugName))
                name = sm.DebugName;

            _containerNames[c] = name;
        }

        private string BuildingName(BuildingModel b)
        {
            if (_buildingNames.TryGetValue(b.Id.Value, out var n))
                return n;

            return $"B{b.Id.Value}";
        }

        private string ContainerName(IResourceContainer c)
        {
            if (_containerNames.TryGetValue(c, out var n))
                return n;

            if (c is StorageModel sm && !string.IsNullOrEmpty(sm.DebugName))
                return sm.DebugName;

            return c.GetType().Name;
        }

        private string ResName(ResourceId id)
        {
            try
            {
                var def = _catalog.GetDef(id);
                return $"{def.Name}({def.Key})";
            }
            catch
            {
                return $"Res#{id.Value}";
            }
        }

        private string RecipeName(Recipe r)
        {
            // коротко: inputs -> output
            var sb = new StringBuilder();

            bool any = false;
            r.ForEachInput((rid, amt) =>
            {
                if (any) sb.Append('+');
                any = true;
                sb.Append(ResName(rid));
                if (amt != 1) sb.Append('x').Append(amt);
            });

            if (!any) sb.Append("∅");
            sb.Append("->").Append(ResName(r.Output));
            return sb.ToString();
        }

        private string DumpContainer(IResourceContainer c)
        {
            var sb = new StringBuilder(128);
            sb.Append(ContainerName(c))
              .Append(" total=").Append(c.Total)
              .Append(" free=").Append(c.FreeSpace)
              .Append(" cap=").Append(c.Capacity)
              .Append(" [");

            bool first = true;
            for (int i = 0; i < _catalog.Count; i++)
            {
                var id = _catalog.FromIndex(i);
                int n = c.Count(id);
                if (n <= 0) continue;

                if (!first) sb.Append(", ");
                first = false;

                sb.Append(ResName(id)).Append('=').Append(n);
            }

            sb.Append(']');
            return sb.ToString();
        }

        private static int StepOf(float progress01, float step)
        {
            if (step <= 0f) step = 0.25f;
            progress01 = Mathf.Clamp01(progress01);
            return (int)Mathf.Floor(progress01 / step);
        }
    }
}
