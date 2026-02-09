
using Game.Core.Common.Fsm;

namespace Game.Core.Production.States
{
    public sealed class BuildingStatePushOutput : IState<BuildingFsmContext, BuildingStatus>
    {
        public BuildingStatus Id => BuildingStatus.PushOutput;

        public void Enter(BuildingFsmContext ctx)
        {
            ctx.B.Status = BuildingStatus.PushOutput;
            ctx.PushScheduled = false;
            ctx.PendingPush = 0;
        }

        public void Exit(BuildingFsmContext ctx) { }

        public bool Tick(BuildingFsmContext ctx, float dt, out BuildingStatus next)
        {
            if (!ctx.PushScheduled)
            {
                if (!ctx.SchedulePushOutput())
                {
                    next = BuildingStatus.Stopped;
                    return true;
                }

                ctx.PushScheduled = true;
            }

            switch (ctx.PendingPush)
            {
                case < 0:
                    next = BuildingStatus.Stopped;
                    return true;
                case 0:
                    next = BuildingStatus.Idle;
                    return true;
                default:
                    next = default;
                    return false;
            }
        }
    }
}