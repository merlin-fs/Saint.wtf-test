namespace Game.Core.Common.Fsm
{
    public interface IState<in TContext, TStateId>
    {
        TStateId Id { get; }

        void Enter(TContext ctx);
        void Exit(TContext ctx);

        /// <summary>
        /// Return true if state requests transition to next state.
        /// </summary>
        bool Tick(TContext ctx, float dt, out TStateId next);
    }
}