using System;
using System.Collections.Generic;
using UnityEngine;

namespace FTg.Common.Observables
{
    
    /// <summary>
    /// Observable подія з навантаженням (T). Аналог UnityEvent<T>,
    /// але через IObservable<T>.
    /// </summary>
    
    public sealed class ObservableEvent<T> : IObservable<T>
    {
        private readonly object _gate = new ();
        private readonly List<IObserver<T>> _observers = new ();
        private bool _isStopped;

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));
            lock (_gate)
            {
                if (_isStopped)
                {
                    // Стopped: одразу Completed
                    observer.OnCompleted();
                    return Disposable.Empty;
                }

                _observers.Add(observer);
                return new Unsubscriber(this, observer);
            }
        }

        /// <summary>Зручна підписка делегатом.</summary>
        public IDisposable Subscribe(Action<T> onNext, Action<Exception> onError = null, Action onCompleted = null)
        {
            return Subscribe(new ActionObserver<T>(onNext, onError, onCompleted));
        }

        /// <summary>Виклик події (аналог UnityEvent.Invoke(value)).</summary>
        public void Invoke(T value) => Raise(value);

        /// <summary>Синонім.</summary>
        public void Raise(T value)
        {
            IObserver<T>[] snapshot;
            lock (_gate)
            {
                if (_isStopped) return;
                snapshot = _observers.ToArray();
            }
            foreach (var t in snapshot)
                t.OnNext(value);
        }

        /// <summary>Завершити стрім (OnCompleted) і відписати всіх.</summary>
        public void Complete()
        {
            IObserver<T>[] snapshot;
            lock (_gate)
            {
                if (_isStopped) return;
                _isStopped = true;
                snapshot = _observers.ToArray();
                _observers.Clear();
            }
            foreach (var t in snapshot)
                t.OnCompleted();
        }

        /// <summary>Завершити помилкою (OnError) і відписати всіх.</summary>
        public void Error(Exception error)
        {
            IObserver<T>[] snapshot;
            lock (_gate)
            {
                if (_isStopped) return;
                _isStopped = true;
                snapshot = _observers.ToArray();
                _observers.Clear();
            }
            foreach (var t in snapshot)
                t.OnError(error);
        }

        private void Unsubscribe(IObserver<T> observer)
        {
            lock (_gate)
            {
                if (_isStopped) return;
                _observers.Remove(observer);
            }
        }

        private sealed class Unsubscriber : IDisposable
        {
            private ObservableEvent<T> _parent;
            private IObserver<T> _observer;

            public Unsubscriber(ObservableEvent<T> parent, IObserver<T> observer)
            {
                _parent = parent;
                _observer = observer;
            }

            public void Dispose()
            {
                var p = _parent;
                var o = _observer;
                if (p == null || o == null) return;
                
                _parent = null;
                _observer = null;
                p.Unsubscribe(o);
            }
        }

        private sealed class ActionObserver<TValue> : IObserver<TValue>
        {
            private readonly Action<TValue> _onNext;
            private readonly Action<Exception> _onError;
            private readonly Action _onCompleted;

            public ActionObserver(Action<TValue> onNext, Action<Exception> onError, Action onCompleted)
            {
                _onNext = onNext ?? (_ => { });
                _onError = onError ?? (ex => Debug.LogException(ex));
                _onCompleted = onCompleted ?? (() => { });
            }

            public void OnNext(TValue value) => _onNext(value);
            public void OnError(Exception error) => _onError(error);
            public void OnCompleted() => _onCompleted();
        }

        private sealed class Disposable : IDisposable
        {
            public static readonly IDisposable Empty = new Disposable();
            public void Dispose() { }
        }
    }

    /// <summary>
    /// Observable подія без даних. Аналог UnityEvent (void).
    /// </summary>
    public sealed class ObservableEvent : IObservable<Unit>
    {
        private readonly ObservableEvent<Unit> _inner = new ObservableEvent<Unit>();

        public IDisposable Subscribe(IObserver<Unit> observer) => _inner.Subscribe(observer);

        public IDisposable Subscribe(Action onNext, Action<Exception> onError = null, Action onCompleted = null)
            => _inner.Subscribe(_ => onNext?.Invoke(), onError, onCompleted);

        public void Invoke() => _inner.Raise(Unit.Default);
        public void Raise() => _inner.Raise(Unit.Default);
        public void Complete() => _inner.Complete();
        public void Error(Exception error) => _inner.Error(error);
    }

    public static class ObservableEventExtensions
    {
        /// <summary>Одноразова підписка: після першого OnNext відписується.</summary>
        public static IDisposable Once<T>(this IObservable<T> source, Action<T> onNext)
        {
            IDisposable sub = null;
            sub = source.Subscribe(value =>
            {
                try { onNext?.Invoke(value); }
                finally { sub?.Dispose(); }
            });
            return sub;
        }
    }    
}