using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FTg.Common.Observables
{
    /// <summary>
    /// Async-подія з фазами (order), поверх ObservableEvent.
    /// </summary>
    public sealed class AsyncObservableEvent<TArgs> : IObservableAsync<TArgs>, IDisposable
    {
        private readonly ObservableEvent<TArgs> _stream = new();

        // key = order (фаза), value = список async-хендлерів
        private readonly SortedDictionary<int, List<Func<TArgs, Task>>> _handlers = new ();

        private bool _isCompleted;

        #region IObservable<TArgs>
        public IDisposable Subscribe(IObserver<TArgs> observer) => _stream.Subscribe(observer);
        #endregion

        /// <summary>
        /// Підписка async-хендлера з вказаною фазою (order).
        /// Чим менше order – тим раніше виконується.
        /// </summary>
        public IDisposable SubscribeAsync(int order, Func<TArgs, Task> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            if (!_handlers.TryGetValue(order, out var list))
            {
                list = new List<Func<TArgs, Task>>();
                _handlers.Add(order, list);
            }

            list.Add(handler);
            return new AsyncSubscription(this, order, handler);
        }

        /// <summary>
        /// Виклик події з очікуванням усіх async-хендлерів по фазах.
        /// </summary>
        public async Task InvokeAsync(TArgs args)
        {
            if (_isCompleted)
                return;

            // 1) звичайний стрім для IObservable-підписників
            _stream.Invoke(args);

            // 2) async-хендлери – по фазах
            foreach (var kv in _handlers)
            {
                var list = kv.Value;
                if (list.Count == 0)
                    continue;

                var tasks = new Task[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    tasks[i] = list[i]?.Invoke(args) ?? Task.CompletedTask;
                }

                await Task.WhenAll(tasks);
            }
        }

        /// <summary>
        /// Fire-and-forget виклик (як звичайний event).
        /// </summary>
        public void Invoke(TArgs args)
        {
            _ = InvokeAsync(args);
        }

        public void Complete()
        {
            if (_isCompleted) return;
            _isCompleted = true;

            _stream.Complete();
            _handlers.Clear();
        }

        public void Error(Exception error)
        {
            if (_isCompleted) return;
            _isCompleted = true;

            _stream.Error(error);
            _handlers.Clear();
        }

        public void Dispose()
        {
            Complete();
        }

        private sealed class AsyncSubscription : IDisposable
        {
            private AsyncObservableEvent<TArgs> _owner;
            private readonly int _order;
            private Func<TArgs, Task> _handler;

            public AsyncSubscription(AsyncObservableEvent<TArgs> owner, int order, Func<TArgs, Task> handler)
            {
                _owner = owner;
                _order = order;
                _handler = handler;
            }

            public void Dispose()
            {
                if (_owner != null && _handler != null &&
                    _owner._handlers.TryGetValue(_order, out var list))
                {
                    list.Remove(_handler);
                }

                _owner = null;
                _handler = null;
            }
        }
    }
}