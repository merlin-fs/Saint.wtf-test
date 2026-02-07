using Game.Core.Common;
using Game.Core.Economy;

namespace Game.Core.Transfers
{
    public record TransferRequest(
        IResourceContainer Source,
        IResourceContainer Destination,
        ResourceId Resource,
        float DurationSeconds,
        object Tag // buildingId / session / debug label
    );
}