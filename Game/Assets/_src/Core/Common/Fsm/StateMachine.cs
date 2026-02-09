using System;
using System.Collections.Generic;

namespace Game.Core.Common.Fsm
{
    public sealed class StateMachine<TContext, TStateId>
        where TStateId : notnull
    {
        private readonly Dictionary<TStateId, IState<TContext, TStateId>> _states = new();

        public TContext Context { get; }
        public IState<TContext, TStateId> Current { get; private set; }
        public TStateId CurrentId { get; private set; }

        public StateMachine(TContext context)
        {
            Context = context;
        }

        public StateMachine<TContext, TStateId> Add(IState<TContext, TStateId> state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            _states[state.Id] = state;
            return this;
        }

        public void Start(TStateId initial)
        {
            if (!_states.TryGetValue(initial, out var st))
                throw new InvalidOperationException($"State '{initial}' not registered.");

            Current = st;
            CurrentId = initial;
            st.Enter(Context);
        }

        public void Tick(float dt)
        {
            if (Current == null)
                throw new InvalidOperationException("StateMachine is not started.");

            if (Current.Tick(Context, dt, out var next))
                TransitionTo(next);
        }

        public void TransitionTo(TStateId next)
        {
            if (Current == null)
                throw new InvalidOperationException("StateMachine is not started.");

            if (CurrentId != null && EqualityComparer<TStateId>.Default.Equals(CurrentId, next))
                return;

            if (!_states.TryGetValue(next, out var st))
                throw new InvalidOperationException($"State '{next}' not registered.");

            Current.Exit(Context);

            Current = st;
            CurrentId = next;

            st.Enter(Context);
        }
    }
}