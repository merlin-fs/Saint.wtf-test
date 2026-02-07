using Game.Core.Common;
using Game.Core.Economy;

namespace Game.Core.Production
{
    public interface IBuilding
    {
        BuildingId Id { get; }
        Recipe Recipe { get; }

        // 2 склади
        IResourceContainer InputStorage { get; }
        IResourceContainer OutputStorage { get; }

        // “порти” для візуалізації: storage <-> building
        IResourceContainer InPort { get; }
        IResourceContainer OutPort { get; }

        BuildingStatus Status { get; }
        StopReason StopReason { get; }

        float ProductionProgress { get; } // 0..1 (тільки коли Producing)
    }
}