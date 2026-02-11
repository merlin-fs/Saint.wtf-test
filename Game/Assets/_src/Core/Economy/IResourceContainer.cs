using System;
using FTg.Common.Observables;
using Game.Core.Common;

namespace Game.Core.Economy
{
    /// <summary>
    /// ReadOnly Інтерфейс для контейнера ресурсів.
    /// </summary>
    public interface IReadOnlyResourceContainer
    {
        int Capacity { get; }
        int Total { get; }
        int FreeSpace { get; }              // з урахуванням reserve
        int Count(ResourceId id);
    }
 
    /// <summary>
    /// Контейнер ресурсів, який підтримує операції додавання та видалення ресурсів з урахуванням анімацій та можливих відміни операцій.
    /// </summary>
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
        IObservable<Unit> Changed { get; }
    }
}