using System;
using Game.Core.Common;
using Game.Core.Economy;
using Game.Core.Transfers;
using FTg.Common.Observables;
using JetBrains.Annotations;

namespace Game.Core.Player
{
    /// <summary>
    /// Відповідає за автоматичний перенос ресурсів між інвентарем гравця та складом, коли гравець знаходиться в зоні складу.
    /// </summary>
    public sealed class SimplePlayerTransferSystem : ITickSystem, IStorageInteractionSink, IDisposable
    {
        private readonly PlayerModel _player;
        private readonly IResourceCatalog _catalog;
        private readonly ITransferScheduler _scheduler;

        private readonly IDisposable _subFinished;

        private IResourceContainer _currentStorage;
        private StorageRole _currentRole;
        private IResourceTransferFilter _currentFilter;

        private TransferId _activeTransfer;
        private bool _hasActiveTransfer;

        private readonly float _pickupSecondsPerUnit;
        private readonly float _dropSecondsPerUnit;

        public SimplePlayerTransferSystem(
            PlayerModel player,
            IResourceCatalog catalog,
            ITransferScheduler scheduler,
            ITransferStream stream,
            float pickupSecondsPerUnit,
            float dropSecondsPerUnit)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));

            _pickupSecondsPerUnit = pickupSecondsPerUnit <= 0 ? 0.0001f : pickupSecondsPerUnit;
            _dropSecondsPerUnit = dropSecondsPerUnit <= 0 ? 0.0001f : dropSecondsPerUnit;

            _subFinished = stream.Finished.Subscribe(OnTransferFinished);
        }

        public void Dispose() => _subFinished.Dispose();

        public void EnterStorage(IResourceContainer storage, StorageRole role, IResourceTransferFilter filter = null)
        {
            _currentStorage = storage;
            _currentRole = role;
            _currentFilter = filter;
        }

        public void ExitStorage(IResourceContainer storage)
        {
            if (!ReferenceEquals(_currentStorage, storage))
                return;

            _currentStorage = null;
            _currentFilter = null;

            // Якщо вийшли з зони — зупиняємо поточний перенос (типово очікувана поведінка)
            if (!_hasActiveTransfer) return;
            
            _scheduler.Cancel(_activeTransfer);
            _hasActiveTransfer = false;
        }

        public void Tick(float dt)
        {
            if (_currentStorage == null)
                return;

            if (_hasActiveTransfer)
                return; // перенос "однієї штуки" вже в процесі

            // Спробувати запланувати наступний перенос по ролі
            if (_currentRole == StorageRole.Output)
                TrySchedulePickup(_currentStorage);
            else
                TryScheduleDrop(_currentStorage);
        }

        private void TrySchedulePickup(IResourceContainer fromStorage)
        {
            // з output складу беремо в інвентар
            if (_player.Inventory.FreeSpace <= 0)
                return;

            if (!TryPickFirstResourceWithCount(fromStorage, null, out var res))
                return;

            var id = _scheduler.Enqueue(new TransferRequest(
                Source: fromStorage,
                Destination: _player.Inventory,
                Resource: res,
                DurationSeconds: _pickupSecondsPerUnit,
                Tag: "player"
            ));

            _activeTransfer = id;
            _hasActiveTransfer = true;
        }

        private void TryScheduleDrop(IResourceContainer toStorage)
        {
            // в input склад віддаємо з інвентаря
            if (toStorage.FreeSpace <= 0)
                return;

            if (!TryPickFirstResourceWithCount(_player.Inventory, _currentFilter, out var res))
                return;

            var id = _scheduler.Enqueue(new TransferRequest(
                Source: _player.Inventory,
                Destination: toStorage,
                Resource: res,
                DurationSeconds: _dropSecondsPerUnit,
                Tag: "player"
            ));

            _activeTransfer = id;
            _hasActiveTransfer = true;
        }

        private bool TryPickFirstResourceWithCount(IResourceContainer c, [CanBeNull] IResourceTransferFilter filter, out ResourceId id)
        {
            for (var i = 0; i < _catalog.Count; i++)
            {
                var rid = _catalog.FromIndex(i);
                if (c.Count(rid) <= 0) continue;
                if (filter != null && !filter.Allows(rid))
                    continue;
                
                id = rid;
                return true;
            }
            id = default;
            return false;
        }

        private void OnTransferFinished(TransferFinished e)
        {
            if (!_hasActiveTransfer) return;
            if (e.Id.Value != _activeTransfer.Value) return;

            _hasActiveTransfer = false;
        }
    }
}