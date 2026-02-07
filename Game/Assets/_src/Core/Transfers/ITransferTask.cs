using Game.Core.Common;

namespace Game.Core.Transfers
{
    public interface ITransferTask
    {
        TransferId Id { get; }
        ResourceId Resource { get; }
        float DurationSeconds { get; }
        float Progress { get; } // 0..1
        TransferStatus Status { get; }
    }
}