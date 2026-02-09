using Game.Core.Common;
using Game.Core.Common.Fsm;

namespace Game.Core.Production.States
{
    public sealed class BuildingStateIdle : IState<BuildingFsmContext, BuildingStatus>
    {
        public BuildingStatus Id => BuildingStatus.Idle;

        public void Enter(BuildingFsmContext ctx)
        {
            ctx.B.Status = BuildingStatus.Idle;
            ctx.B.ProductionProgress = 0f;
            ctx.B.StopReason = StopReason.None;
            ctx.ResetCycleRuntime();
        }

        public void Exit(BuildingFsmContext ctx) { }

        public bool Tick(BuildingFsmContext ctx, float dt, out BuildingStatus next)
        {
            if (!ctx.CanStartCycle(out var reason))
            {
                ctx.B.StopReason = reason;
                next = BuildingStatus.Stopped;
                return true;
            }

            next = ctx.B.Recipe.HasInputs()
                ? BuildingStatus.PullInputs
                : BuildingStatus.Producing;

            return true;
        }
    }
}