using System.Collections.Generic;
using Game.Client.Resources;
using Game.Core.Transfers;
using FTg.Common.Observables;
using Game.Client.Common;
using UnityEngine;

namespace Game.Client.Transfers
{
    /// <summary>
    /// Відповідає за візуалізацію процесу трансферу ресурсів між контейнерами. Просто анімує блок ресурсу від джерела до приймача.
    /// </summary>
    public sealed class VisualTransferSystem : MonoBehaviour
    {
        [Header("Prefab/Pool")]
        [SerializeField] private ResourceBlockView blockPrefab;
        [SerializeField] private Transform poolRoot;

        [Header("Visuals")]
        [SerializeField] private ResourceLibrary visualCatalog;

        [Header("Placement")]
        [SerializeField] private Vector3 startOffset = new(0f, 0.05f, 0f);
        [SerializeField] private Vector3 endOffset   = new(0f, 0.05f, 0f);

        private ResourceBlockPool _pool;
        private ContainerTransformMap _map;
        private CompositeDisposable _disposables;
        private readonly Dictionary<int, ActiveVisual> _active = new();

        public void Initialize(ITransferStream stream, ContainerTransformMap map)
        {
            _map = map;
            _pool = new ResourceBlockPool(blockPrefab, poolRoot != null ? poolRoot : transform);

            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            stream.Started.Subscribe(OnStarted).AddTo(_disposables);
            stream.Progress.Subscribe(OnProgress).AddTo(_disposables);
            stream.Finished.Subscribe(OnFinished).AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        private void OnStarted(TransferStarted e)
        {
            // source/destination у світі
            var from = _map.GetPosition(e.Source) + startOffset;
            var to   = _map.GetPosition(e.Destination) + endOffset;

            var view = _pool.Rent();
            view.transform.position = from;
            view.gameObject.SetActive(true);

            // застосувати view під ресурс
            if (visualCatalog != null && visualCatalog.TryGet(e.Resource, out var visual))
                view.Apply(visual);

            _active[e.Id.Value] = new ActiveVisual
            {
                View = view,
                From = from,
                To = to
            };
        }

        private void OnProgress(TransferProgress e)
        {
            if (!_active.TryGetValue(e.Id.Value, out var v))
                return;

            var p = Mathf.Clamp01(e.Progress);
            v.View.transform.position = Vector3.Lerp(v.From, v.To, p);
        }

        private void OnFinished(TransferFinished e)
        {
            if (_pool == null) return;

            if (_active.Remove(e.Id.Value, out var v))
                _pool.Return(v.View);
        }

        private struct ActiveVisual
        {
            public ResourceBlockView View;
            public Vector3 From;
            public Vector3 To;
        }

    }
}
