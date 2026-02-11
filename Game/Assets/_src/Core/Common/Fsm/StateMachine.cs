using System;
using System.Collections.Generic;

namespace Game.Core.Common.Fsm
{
    /// <summary>
    /// Простая реализация конечного автомата.
    /// Состояния должны реализовать IState<TContext, TStateId>.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TStateId"></typeparam>
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

        /// <summary>
        /// Запускає автомат у початковому стані. Викликає метод Enter початкового стану.
        /// </summary>
        /// <param name="initial"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Start(TStateId initial)
        {
            if (!_states.TryGetValue(initial, out var st))
                throw new InvalidOperationException($"State '{initial}' not registered.");

            Current = st;
            CurrentId = initial;
            st.Enter(Context);
        }

        /// <summary>
        /// Викликає метод Tick поточного стану.
        /// Якщо він повертає true, то виконує перехід до наступного стану, який повертається через out-параметр next.
        /// </summary>
        /// <param name="dt"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Tick(float dt)
        {
            if (Current == null)
                throw new InvalidOperationException("StateMachine is not started.");

            if (Current.Tick(Context, dt, out var next))
                TransitionTo(next);
        }

        /// <summary>
        /// Виконує перехід до стану next. Викликає метод Exit поточного стану, а потім Enter нового стану.
        /// </summary>
        /// <param name="next"></param>
        /// <exception cref="InvalidOperationException"></exception>
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