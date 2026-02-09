using Game.Core.Common;
using Game.Core.Common.Fsm;

namespace Game.Core.Production.States
{
     public class BuildingStatePullInputs : IState<BuildingFsmContext, BuildingStatus>
    {
        public BuildingStatus Id => BuildingStatus.PullInputs;

        public void Enter(BuildingFsmContext ctx)
        {
            ctx.B.Status = BuildingStatus.PullInputs;
            ctx.B.ProductionProgress = 0f;

            ctx.PullScheduled = false;
            ctx.PendingPull = 0;
        }

        public void Exit(BuildingFsmContext ctx) { }

        public bool Tick(BuildingFsmContext ctx, float dt, out BuildingStatus next)
        {
            if (!ctx.PullScheduled)
            {
                if (!ctx.SchedulePullInputs())
                {
                    next = BuildingStatus.Stopped;
                    return true;
                }

                ctx.PullScheduled = true;
            }

            // TransferFinished може ставити PendingPull < 0 як "failed marker"
            if (ctx.PendingPull < 0)
            {
                next = BuildingStatus.Stopped;
                return true;
            }

            if (ctx.PendingPull == 0)
            {
                if (!ctx.ConsumeInputsFromInPort())
                {
                    ctx.B.StopReason = StopReason.TransferBlocked;
                    next = BuildingStatus.Stopped;
                    return true;
                }

                next = BuildingStatus.Producing;
                return true;
            }

            next = default;
            return false;
        }
    }
}