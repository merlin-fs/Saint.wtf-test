using Game.Core.Common;
using Game.Core.Economy;

namespace Game.Core.Transfers
{
    /// <summary>
    /// Запит на передачу ресурса между двумя контейнерами. 
    /// </summary>
    /// <param name="Source"></param>
    /// <param name="Destination"></param>
    /// <param name="Resource"></param>
    /// <param name="DurationSeconds"></param>
    /// <param name="Tag"></param>
    public record TransferRequest(
        IResourceContainer Source,
        IResourceContainer Destination,
        ResourceId Resource,
        float DurationSeconds,
        object Tag // buildingId / session / debug label
    );
}