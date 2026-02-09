using Game.Core.Common.Fsm;

namespace Game.Core.Production.States
{
    public sealed class BuildingStateStopped : IState<BuildingFsmContext, BuildingStatus>
    {
        public BuildingStatus Id => BuildingStatus.Stopped;

        public void Enter(BuildingFsmContext ctx)
        {
            ctx.B.Status = BuildingStatus.Stopped;
            // StopReason вже має бути виставлений контекстом/попереднім станом
        }

        public void Exit(BuildingFsmContext ctx) { }

        public bool Tick(BuildingFsmContext ctx, float dt, out BuildingStatus next)
        {
            // авто-старт коли умови знову ок
            if (ctx.CanStartCycle(out _))
            {
                next = BuildingStatus.Idle;
                return true;
            }

            next = default;
            return false;
        }
    }
}