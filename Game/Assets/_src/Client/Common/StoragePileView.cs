using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Common;
using Game.Core.Economy;
using FTg.Common.Observables;
using Game.Client.Resources;
using Game.Client.UI.Hud;
using UnityEngine;

namespace Game.Client.Common
{
    public sealed class StoragePileView : MonoBehaviour, IWorldBoundsProvider
    {
        [Header("Binding")] [SerializeField] private Transform pileRoot;
        [SerializeField] private BoxCollider boundsBox; // межі викладки (локальні)

        [SerializeField] private ResourceLibrary visualCatalog;

        [Header("Prefab/Pool")] [SerializeField]
        private ResourceBlockView blockPrefab;

        [SerializeField] private Transform poolRoot;

        [Header("Block layout params")] [SerializeField]
        private float blockFootprint = 0.25f; // "розмір" блоку по X/Z

        [SerializeField] private float blockHeight = 0.22f; // крок по Y
        [SerializeField] private float padding = 0.05f;

        [Tooltip("Групувати ресурси смугами по X всередині bounds.")] [SerializeField]
        private bool groupByResource = true;

        private IResourceCatalog _catalog;
        private IResourceContainer _container;
        private CompositeDisposable _disposables;
        private ResourceBlockPool _pool;

        private readonly Dictionary<ResourceId, List<ResourceBlockView>> _blocksByRes = new();
        private readonly Dictionary<ResourceId, int> _lastCounts = new();
        private readonly List<ResourceId> _order = new();

        public void Bind(IResourceCatalog catalog, IResourceContainer container)
        {
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _container = container ?? throw new ArgumentNullException(nameof(container));

            if (pileRoot == null) pileRoot = transform;
            if (boundsBox == null) boundsBox = GetComponentInChildren<BoxCollider>();
            _pool = new ResourceBlockPool(blockPrefab, poolRoot != null ? poolRoot : transform);
            container.Changed.Subscribe(_ => Refresh()).AddTo(_disposables);

            Refresh();
        }

        private void OnDestroy() => _disposables?.Dispose();

        public bool TryGetBounds(out Bounds b)
        {
            if (boundsBox == null)
            {
                b = default;
                return false;
            }
            b = boundsBox.bounds; // world bounds
            return true;
        }            
        
        private void Refresh()
        {
            if (!UpdateResourceCounts()) return;
            SyncVisualBlocks();
            LayoutInBounds();
        }

        private bool UpdateResourceCounts()
        {
            var anyChanged = false;
            _order.Clear();

            for (var i = 0; i < _catalog.Count; i++)
            {
                var id = _catalog.FromIndex(i);
                _order.Add(id);

                var currentCount = _container.Count(id);
                _lastCounts.TryGetValue(id, out var previousCount);

                if (previousCount == currentCount) continue;
                _lastCounts[id] = currentCount;
                anyChanged = true;
            }

            return anyChanged;
        }

        private void SyncVisualBlocks()
        {
            foreach (var res in _order)
            {
                var desiredCount = _container.Count(res);
                var list = GetOrCreateBlockList(res);

                RemoveExcessBlocks(list, desiredCount);
                AddMissingBlocks(list, res, desiredCount);
            }
        }

        private void LayoutInBounds()
        {
            var config = CreateLayoutConfig();
            var activeResources = CollectActiveResources();

            if (!ShouldUseGroupedLayout(activeResources, config.Columns))
            {
                LayoutSequential(config);
                return;
            }

            LayoutGrouped(config, activeResources);
        }

        #region compute columns
        private List<ResourceBlockView> GetOrCreateBlockList(ResourceId res)
        {
            if (_blocksByRes.TryGetValue(res, out var list)) return list;
            
            list = new List<ResourceBlockView>();
            _blocksByRes[res] = list;
            return list;
        }

        private void RemoveExcessBlocks(List<ResourceBlockView> list, int desiredCount)
        {
            while (list.Count > desiredCount)
            {
                var index = list.Count - 1;
                _pool.Return(list[index]);
                list.RemoveAt(index);
            }
        }

        private void AddMissingBlocks(List<ResourceBlockView> list, ResourceId res, int desiredCount)
        {
            while (list.Count < desiredCount)
            {
                var block = _pool.Rent();
                block.transform.SetParent(pileRoot, false);

                if (visualCatalog != null && visualCatalog.TryGet(res, out var visual))
                    block.Apply(visual);

                list.Add(block);
            }
        }

        private LayoutConfig CreateLayoutConfig()
        {
            var size = boundsBox.size;
            var center = boundsBox.center;

            var usableX = Mathf.Max(0.01f, size.x - padding * 2f);
            var usableZ = Mathf.Max(0.01f, size.z - padding * 2f);

            return new LayoutConfig
            {
                Columns = Mathf.Max(1, Mathf.FloorToInt(usableX / blockFootprint)),
                Rows = Mathf.Max(1, Mathf.FloorToInt(usableZ / blockFootprint)),
                StartX = center.x - usableX * 0.5f + blockFootprint * 0.5f,
                StartZ = center.z - usableZ * 0.5f + blockFootprint * 0.5f,
                StartY = center.y - size.y * 0.5f + blockHeight * 0.5f
            };
        }

        private bool ShouldUseGroupedLayout(List<ResourceId> activeResources, int availableColumns)
        {
            return groupByResource 
                   && activeResources.Count > 1 
                   && availableColumns >= activeResources.Count;
        }

        private void LayoutGrouped(LayoutConfig config, List<ResourceId> activeResources)
        {
            var columnsPerResource = AllocateColumns(activeResources, config.Columns);
            var currentColumn = 0;

            for (var i = 0; i < activeResources.Count; i++)
            {
                var resource = activeResources[i];
                var stripeColumns = columnsPerResource[i];

                if (_blocksByRes.TryGetValue(resource, out var blocks) && blocks.Count > 0)
                {
                    LayoutResourceStripe(blocks, config, currentColumn, stripeColumns);
                }

                currentColumn += stripeColumns;
            }
        }

        private void LayoutResourceStripe(List<ResourceBlockView> blocks, LayoutConfig config, 
            int startColumn, int stripeColumns)
        {
            var stripeLayerSize = stripeColumns * config.Rows;

            for (var i = 0; i < blocks.Count; i++)
            {
                var position = CalculateBlockPosition(i, stripeLayerSize, startColumn, stripeColumns, config);
                blocks[i].transform.localPosition = position;
                blocks[i].gameObject.SetActive(true);
            }
        }

        private Vector3 CalculateBlockPosition(int blockIndex, int layerSize, int columnOffset, 
            int stripeWidth, LayoutConfig config)
        {
            var indexInLayer = blockIndex % layerSize;
            var layer = blockIndex / layerSize;

            var localColumn = indexInLayer % stripeWidth;
            var row = indexInLayer / stripeWidth;
            var globalColumn = columnOffset + localColumn;

            return new Vector3(
                config.StartX + globalColumn * blockFootprint,
                config.StartY + layer * blockHeight,
                config.StartZ + row * blockFootprint
            );
        }

        private void LayoutSequential(LayoutConfig config)
        {
            var globalIndex = 0;

            foreach (var res in _order)
            {
                if (!_blocksByRes.TryGetValue(res, out var list) || list.Count == 0)
                    continue;

                for (var i = 0; i < list.Count; i++)
                {
                    var position = CalculateSequentialPosition(globalIndex + i, config);
                    list[i].transform.localPosition = position;
                    list[i].gameObject.SetActive(true);
                }

                globalIndex += list.Count;
            }
        }

        private Vector3 CalculateSequentialPosition(int index, LayoutConfig config)
        {
            var indexInLayer = index % config.LayerSize;
            var layer = index / config.LayerSize;

            var column = indexInLayer % config.Columns;
            var row = indexInLayer / config.Columns;

            return new Vector3(
                config.StartX + column * blockFootprint,
                config.StartY + layer * blockHeight,
                config.StartZ + row * blockFootprint
            );
        }

        private List<ResourceId> CollectActiveResources()
        {
            var active = new List<ResourceId>(4);

            foreach (var res in _order)
            {
                if (_blocksByRes.TryGetValue(res, out var list) && list.Count > 0)
                    active.Add(res);
            }

            return active;
        }

        private int[] AllocateColumns(List<ResourceId> resources, int totalColumns)
        {
            var counts = GetResourceCounts(resources);
            var totalWeight = counts.Sum(count => Mathf.Max(1, count));
            
            var allocation = InitializeAllocation(resources.Count);
            var targetAllocation = CalculateTargetAllocation(counts, totalWeight, totalColumns);
            
            ApplyFloorAllocation(allocation, targetAllocation);
            AdjustToExactTotal(allocation, targetAllocation, totalColumns);

            return allocation;
        }

        private int[] GetResourceCounts(List<ResourceId> resources)
        {
            var counts = new int[resources.Count];
            for (var i = 0; i < resources.Count; i++)
            {
                counts[i] = _blocksByRes.TryGetValue(resources[i], out var list) ? list.Count : 0;
            }
            return counts;
        }

        private int[] InitializeAllocation(int resourceCount)
        {
            var allocation = new int[resourceCount];
            for (var i = 0; i < resourceCount; i++)
            {
                allocation[i] = 1; // minimum 1 column per resource
            }
            return allocation;
        }

        private float[] CalculateTargetAllocation(int[] counts, int totalWeight, int totalColumns)
        {
            var targets = new float[counts.Length];
            for (var i = 0; i < counts.Length; i++)
            {
                var weight = Mathf.Max(1, counts[i]);
                targets[i] = (float)weight / totalWeight * totalColumns;
            }
            return targets;
        }

        private void ApplyFloorAllocation(int[] allocation, float[] targets)
        {
            for (var i = 0; i < allocation.Length; i++)
            {
                var additional = Mathf.Max(0, Mathf.FloorToInt(targets[i]) - 1);
                allocation[i] += additional;
            }
        }

        private void AdjustToExactTotal(int[] allocation, float[] targets, int totalColumns)
        {
            var currentTotal = allocation.Sum();
            var fractionalParts = CalculateFractionalParts(targets);

            while (currentTotal < totalColumns)
            {
                var index = FindLargestFractional(fractionalParts);
                allocation[index]++;
                fractionalParts[index] = 0;
                currentTotal++;
            }

            while (currentTotal > totalColumns)
            {
                var index = FindLargestAllocation(allocation);
                if (index < 0) break;
                allocation[index]--;
                currentTotal--;
            }
        }

        private float[] CalculateFractionalParts(float[] targets)
        {
            var fractionals = new float[targets.Length];
            for (var i = 0; i < targets.Length; i++)
            {
                fractionals[i] = targets[i] - Mathf.Floor(targets[i]);
            }
            return fractionals;
        }

        private int FindLargestFractional(float[] fractionals)
        {
            var maxIndex = 0;
            var maxValue = fractionals[0];

            for (var i = 1; i < fractionals.Length; i++)
            {
                if (!(fractionals[i] > maxValue)) continue;
                maxValue = fractionals[i];
                maxIndex = i;
            }

            return maxIndex;
        }

        private int FindLargestAllocation(int[] allocation)
        {
            var maxIndex = -1;
            var maxValue = 1; // don't reduce below 1

            for (var i = 0; i < allocation.Length; i++)
            {
                if (allocation[i] > maxValue)
                {
                    maxValue = allocation[i];
                    maxIndex = i;
                }
            }

            return maxIndex;
        }
        #endregion

        private struct LayoutConfig
        {
            public float StartX;
            public float StartY;
            public float StartZ;
            public int Columns;
            public int Rows;
            public int LayerSize => Columns * Rows;
        }
    }
}
