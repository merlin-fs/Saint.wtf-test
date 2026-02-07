using Game.Core.Common;

namespace Game.Core.Transfers
{
    public interface ITransferScheduler : ITickSystem
    {
        TransferId Enqueue(TransferRequest request);
        bool TryGet(TransferId id, out ITransferTask task);
        void Cancel(TransferId id);
    }
}