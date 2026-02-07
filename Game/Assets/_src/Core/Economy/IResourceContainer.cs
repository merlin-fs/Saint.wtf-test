using System;
using FTg.Common.Observables;
using Game.Core.Common;

namespace Game.Core.Economy
{
    public interface IReadOnlyResourceContainer
    {
        int Capacity { get; }
        int Total { get; }
        int FreeSpace { get; }              // з урахуванням reserve
        int Count(ResourceId id);
    }
    
    public interface IResourceContainer : IReadOnlyResourceContainer
    {
        // Source: "зняли 1 шт" (або зарезервували зняття)
        bool TryBeginRemove(ResourceId id, out RemoveToken token);
        void CancelRemove(RemoveToken token);

        // Destination: "зарезервували місце"
        bool TryReserveAdd(ResourceId id, out AddReservation reservation);
        void CancelAdd(AddReservation reservation);

        // Фіналізація після завершення анімації
        void CommitAdd(AddReservation reservation);

        /// <summary>
        /// Сигнал “щось змінилось” (counts/total/freeSpace).
        /// Споживач (UI/VM/Client) при потребі сам читає стан контейнера.
        /// </summary>
        IObservable<Unit> Changed { get; }
    }
}