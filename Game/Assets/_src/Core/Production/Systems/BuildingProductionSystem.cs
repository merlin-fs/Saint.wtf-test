using System.Collections.Generic;
using Game.Core.Common;

namespace Game.Core.Production
{
    /// <summary>
    /// Система виробництва для будівель, що керує циклами виробництва та передачами ресурсів
    /// </summary>
    public sealed class BuildingProductionSystem : ITickSystem
    {
        private readonly List<BuildingFsm> _machines;

        public BuildingProductionSystem(IEnumerable<BuildingFsm> machines)
        {
            _machines = new List<BuildingFsm>(machines);
        }

        public void Tick(float dt)
        {
            foreach (var t in _machines)
                t.Tick(dt);
        }        
    }
}
