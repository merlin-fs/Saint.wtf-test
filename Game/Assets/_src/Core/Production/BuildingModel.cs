using Game.Core.Common;
using Game.Core.Economy;

namespace Game.Core.Production
{
    public sealed class BuildingModel : IBuilding
    {
        public BuildingModel(
            BuildingId id,
            Recipe recipe,
            IResourceContainer inputStorage,
            IResourceContainer outputStorage,
            IResourceContainer inPort,
            IResourceContainer outPort,
            float inputTransferSecondsPerUnit,
            float outputTransferSecondsPerUnit)
        {
            Id = id;
            Recipe = recipe;

            InputStorage = inputStorage;
            OutputStorage = outputStorage;

            InPort = inPort;
            OutPort = outPort;

            InputTransferSecondsPerUnit = inputTransferSecondsPerUnit;
            OutputTransferSecondsPerUnit = outputTransferSecondsPerUnit;

            Status = BuildingStatus.Idle;
            StopReason = StopReason.None;
        }

        public BuildingId Id { get; }
        public Recipe Recipe { get; }

        public IResourceContainer InputStorage { get; }
        public IResourceContainer OutputStorage { get; }

        public IResourceContainer InPort { get; }
        public IResourceContainer OutPort { get; }

        public float InputTransferSecondsPerUnit { get; }
        public float OutputTransferSecondsPerUnit { get; }

        public BuildingStatus Status { get; internal set; }
        public StopReason StopReason { get; internal set; }

        public float ProductionProgress { get; internal set; }
    }
}