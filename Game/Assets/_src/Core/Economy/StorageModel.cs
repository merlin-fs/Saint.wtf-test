using System;
using FTg.Common.Observables;
using Game.Core.Common;

namespace Game.Core.Economy
{
    /// <summary>
    /// Модель сховища ресурсів з підтримкою резервування місця для вхідних трансферів
    /// </summary>
    public sealed class StorageModel : IResourceContainer
    {
        private readonly IResourceCatalog _catalog;

        private readonly int[] _counts;         // фактичні предмети в сховищі
        private readonly int[] _reservedAdds;   // зарезервовані вхідні предмети

        private int _total;          // sum(_counts) - загальна кількість предметів
        private int _reservedTotal;  // sum(_reservedAdds) - загальна кількість зарезервованих

        private readonly ObservableEvent<Unit> _changed = new();

        public StorageModel(IResourceCatalog catalog, int capacity, string debugName = null)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));

            Capacity = capacity;
            DebugName = debugName ?? string.Empty;

            _counts = new int[catalog.Count];
            _reservedAdds = new int[catalog.Count];
        }

        public string DebugName { get; }

        /// <summary>
        /// Максимальна ємність сховища
        /// </summary>
        public int Capacity { get; }

        /// <summary>
        /// Поточна кількість предметів у сховищі
        /// </summary>
        public int Total => _total;

        /// <summary>
        /// Вільне місце з урахуванням зарезервованих слотів
        /// </summary>
        public int FreeSpace => Capacity - (_total + _reservedTotal);

        public IObservable<Unit> Changed => _changed;

        /// <summary>
        /// Повертає кількість конкретного ресурсу в сховищі
        /// </summary>
        public int Count(ResourceId id)
        {
            var i = _catalog.ToIndex(id);
            return _counts[i];
        }

        /// <summary>
        /// Спробувати почати видалення ресурсу (негайне зняття зі сховища)
        /// </summary>
        public bool TryBeginRemove(ResourceId id, out RemoveToken token)
        {
            var i = _catalog.ToIndex(id);
            if (_counts[i] <= 0)
            {
                token = default;
                return false;
            }

            // Негайно віднімаємо предмет
            _counts[i]--;
            _total--;

            token = new RemoveToken(id);
            _changed.Invoke(Unit.Default);
            return true;
        }

        /// <summary>
        /// Скасувати видалення (повернути предмет назад у сховище)
        /// </summary>
        public void CancelRemove(RemoveToken token)
        {
            var i = _catalog.ToIndex(token.Resource);

            // Повертаємо предмет, який був раніше знятий
            _counts[i]++;
            _total++;

            _changed.Invoke(Unit.Default);
        }

        /// <summary>
        /// Зарезервувати місце для вхідного ресурсу
        /// </summary>
        public bool TryReserveAdd(ResourceId id, out AddReservation reservation)
        {
            if (FreeSpace <= 0)
            {
                reservation = default;
                return false;
            }

            var i = _catalog.ToIndex(id);
            _reservedAdds[i]++;
            _reservedTotal++;

            reservation = new AddReservation(id);
            _changed.Invoke(Unit.Default); // Змінилося вільне місце
            return true;
        }

        /// <summary>
        /// Скасувати резервування місця для додавання
        /// </summary>
        public void CancelAdd(AddReservation reservation)
        {
            var i = _catalog.ToIndex(reservation.Resource);

            if (_reservedAdds[i] <= 0 || _reservedTotal <= 0)
                throw new InvalidOperationException($"CancelAdd без активного резервування. Сховище='{DebugName}', Ресурс={reservation.Resource}");

            _reservedAdds[i]--;
            _reservedTotal--;

            _changed.Invoke(Unit.Default);
        }

        /// <summary>
        /// Підтвердити додавання ресурсу (перетворити резервування в фактичний предмет)
        /// </summary>
        public void CommitAdd(AddReservation reservation)
        {
            var i = _catalog.ToIndex(reservation.Resource);

            if (_reservedAdds[i] <= 0 || _reservedTotal <= 0)
                throw new InvalidOperationException($"CommitAdd без активного резервування. Сховище='{DebugName}', Ресурс={reservation.Resource}");

            // Знімаємо резервування
            _reservedAdds[i]--;
            _reservedTotal--;

            // Додаємо фактичний предмет
            if (_total >= Capacity)
            {
                // Це не повинно статися, якщо FreeSpace правильно враховує резервування
                throw new InvalidOperationException($"CommitAdd перевищить ємність. Сховище='{DebugName}', Ємність={Capacity}, Всього={_total}");
            }

            _counts[i]++;
            _total++;

            _changed.Invoke(Unit.Default);
        }
    }
}
