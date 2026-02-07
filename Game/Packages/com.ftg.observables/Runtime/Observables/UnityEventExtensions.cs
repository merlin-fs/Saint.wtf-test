using System;
using UnityEngine.Events;

namespace FTg.Common.Observables
{
    public static class UnityEventExtensions
    {
        public static IObservable<T> FromUnityEvent<T>(this UnityEvent<T> unityEvent)
        {
            return new UnityEventObservable<T>(unityEvent);
        }

        public static IObservable<Unit> FromUnityEvent(this UnityEvent unityEvent)
        {
            //Action aas;
            //aas.Invoke();
            return new UnityEventObservable(unityEvent);
        }

        #region UnityEventObservable
        private class UnityEventObservable : IObservable<Unit>
        {
            private readonly UnityEvent _unityEvent;

            public UnityEventObservable(UnityEvent unityEvent)
            {
                _unityEvent = unityEvent;
            }

            public IDisposable Subscribe(IObserver<Unit> observer)
            {
                void Handler() => observer.OnNext(Unit.Default);

                _unityEvent.AddListener(Handler);

                return new Unsubscriber(() => _unityEvent.RemoveListener(Handler));
            }

            private class Unsubscriber : IDisposable
            {
                private readonly Action _unsubscribe;
                public Unsubscriber(Action unsubscribe) => _unsubscribe = unsubscribe;
                public void Dispose() => _unsubscribe?.Invoke();
            }
        }

        private class UnityEventObservable<T> : IObservable<T>
        {
            private readonly UnityEvent<T> _unityEvent;

            public UnityEventObservable(UnityEvent<T> unityEvent)
            {
                _unityEvent = unityEvent;
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                void Handler(T value) => observer.OnNext(value);

                _unityEvent.AddListener(Handler);

                return new Unsubscriber(() => _unityEvent.RemoveListener(Handler));
            }

            private class Unsubscriber : IDisposable
            {
                private readonly Action _unsubscribe;

                public Unsubscriber(Action unsubscribe)
                {
                    _unsubscribe = unsubscribe;
                }

                public void Dispose()
                {
                    _unsubscribe?.Invoke();
                }
            }
        }
        #endregion
    }
}
