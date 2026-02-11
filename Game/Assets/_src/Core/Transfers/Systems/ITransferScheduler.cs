using Game.Core.Common;

namespace Game.Core.Transfers
{
    /// <summary>
    /// Система, яка відповідає за планування та виконання передач ресурсів між контейнерами.
    /// Вона керує чергою завдань на передачу, забезпечує їх виконання та дозволяє скасовувати заплановані передачі.
    /// </summary>
    public interface ITransferScheduler : ITickSystem
    {
        TransferId Enqueue(TransferRequest request);
        bool TryGet(TransferId id, out ITransferTask task);
        void Cancel(TransferId id);
    }
}