using Game.Core.Common;
using Game.Core.Economy;

namespace Game.Core.Transfers
{
    public record TransferStarted(
        TransferId Id,
        IResourceContainer Source,
        IResourceContainer Destination,
        ResourceId Resource,
        float DurationSeconds,
        object Tag
    );

    public record TransferProgress(TransferId Id, float Progress); // 0..1

    public record TransferFinished(TransferId Id, TransferStatus Status);
}