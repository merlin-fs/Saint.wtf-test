using Game.Core.Common;
using Game.Core.Common.Fsm;

namespace Game.Core.Production.States
{
    public sealed class BuildingStateProducing : IState<BuildingFsmContext, BuildingStatus>
    {
        public BuildingStatus Id => BuildingStatus.Producing;

        public void Enter(BuildingFsmContext ctx)
        {
            ctx.B.Status = BuildingStatus.Producing;
            ctx.ProduceTimer = 0f;
            ctx.B.ProductionProgress = 0f;
        }

        public void Exit(BuildingFsmContext ctx) { }

        public bool Tick(BuildingFsmContext ctx, float dt, out BuildingStatus next)
        {
            var t = ctx.B.Recipe.ProductionTimeSeconds <= 0f ? 0.0001f : ctx.B.Recipe.ProductionTimeSeconds;
            ctx.ProduceTimer += dt;

            var p = ctx.ProduceTimer / t;
            if (p > 1f) p = 1f;
            ctx.B.ProductionProgress = p;

            if (p < 1f)
            {
                next = default;
                return false;
            }

            // output з’являється в OutPort миттєво (логіка), а перенос до складу — через transfer
            if (!ctx.SpawnOutputToOutPort())
            {
                ctx.B.StopReason = StopReason.OutputFull;
                next = BuildingStatus.Stopped;
                return true;
            }

            next = BuildingStatus.PushOutput;
            return true;
        }
    }
}