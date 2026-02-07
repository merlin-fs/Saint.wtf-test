using System.Collections.Generic;
using Game.Core.Production;

namespace Game.Core.UI
{
    public record BuildingStatusLine(BuildingId BuildingId, string Text);
    
    public interface IBuildingStatusProvider
    {
        IReadOnlyList<BuildingStatusLine> GetLines();
    }
}