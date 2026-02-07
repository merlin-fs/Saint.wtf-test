using System;

namespace FTg.Common.Observables
{
    public static class EventExtensions
    {
        public static IObservable<(object sender, TEventArgs args)> FromEvent<TEventArgs>(
            Action<EventHandler<TEventArgs>> subscribe,
            Action<EventHandler<TEventArgs>> unsubscribe)
        {
            return new EventHandlerObservable<TEventArgs>(subscribe, unsubscribe);
        }

        public static IObservable<Unit> FromEvent(Action<Action> subscribe, Action<Action> unsubscribe)
        {
            return new EventObservable(subscribe, unsubscribe);
        }
        public static IObservable<T> FromEvent<T>(Action<Action<T>> subscribe, Action<Action<T>> unsubscribe)
        {
            return new EventObservable<T>(subscribe, unsubscribe);
        }

        public static IObservable<(T1, T2)> FromEvent<T1, T2>(Action<Action<T1, T2>> subscribe, Action<Action<T1, T2>> unsubscribe)
        {
            return new EventObservable<T1, T2>(subscribe, unsubscribe);
        }

        public static IObservable<(T1, T2, T3)> FromEvent<T1, T2, T3>(Action<Action<T1, T2, T3>> subscribe, Action<Action<T1, T2, T3>> unsubscribe)
        {
            return new EventObservable<T1, T2, T3>(subscribe, unsubscribe);
        }
        #region EventObservable

        private class EventHandlerObservable<TEventArgs> : IObservable<(object, TEventArgs)>
        {
            private readonly Action<EventHandler<TEventArgs>> _subscribe;
            private readonly Action<EventHandler<TEventArgs>> _unsubscribe;

            public EventHandlerObservable(Action<EventHandler<TEventArgs>> subscribe, Action<EventHandler<TEventArgs>> unsubscribe)
            {
                _subscribe = subscribe;
                _unsubscribe = unsubscribe;
            }

            public IDisposable Subscribe(IObserver<(object, TEventArgs)> observer)
            {
                void Handler(object sender, TEventArgs args) => observer.OnNext((sender, args));

                _subscribe(Handler);

                return new Unsubscriber(() => _unsubscribe(Handler));
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

        private class EventObservable : IObservable<Unit>
        {
            private readonly Action<Action> _subscribe;
            private readonly Action<Action> _unsubscribe;

            public EventObservable(Action<Action> subscribe, Action<Action> unsubscribe)
            {
                _subscribe = subscribe;
                _unsubscribe = unsubscribe;
            }

            public IDisposable Subscribe(IObserver<Unit> observer)
            {
                void Handler() => observer.OnNext(Unit.Default);

                _subscribe(Handler);

                return new Unsubscriber(() => _unsubscribe(Handler));
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

        private class EventObservable<T> : IObservable<T>
        {
            private readonly Action<Action<T>> _subscribe;
            private readonly Action<Action<T>> _unsubscribe;

            public EventObservable(Action<Action<T>> subscribe, Action<Action<T>> unsubscribe)
            {
                _subscribe = subscribe;
                _unsubscribe = unsubscribe;
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                void Handler(T value) => observer.OnNext(value);

                _subscribe(Handler);

                return new Unsubscriber(() => _unsubscribe(Handler));
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

        private class EventObservable<T1, T2> : IObservable<(T1, T2)>
        {
            private readonly Action<Action<T1, T2>> _subscribe;
            private readonly Action<Action<T1, T2>> _unsubscribe;

            public EventObservable(Action<Action<T1, T2>> subscribe, Action<Action<T1, T2>> unsubscribe)
            {
                _subscribe = subscribe;
                _unsubscribe = unsubscribe;
            }

            public IDisposable Subscribe(IObserver<(T1, T2)> observer)
            {
                void Handler(T1 arg1, T2 arg2) => observer.OnNext((arg1, arg2));

                _subscribe(Handler);

                return new Unsubscriber(() => _unsubscribe(Handler));
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


        private class EventObservable<T1, T2, T3> : IObservable<(T1, T2, T3)>
        {
            private readonly Action<Action<T1, T2, T3>> _subscribe;
            private readonly Action<Action<T1, T2, T3>> _unsubscribe;

            public EventObservable(Action<Action<T1, T2, T3>> subscribe, Action<Action<T1, T2, T3>> unsubscribe)
            {
                _subscribe = subscribe;
                _unsubscribe = unsubscribe;
            }

            public IDisposable Subscribe(IObserver<(T1, T2, T3)> observer)
            {
                void Handler(T1 arg1, T2 arg2, T3 arg3) => observer.OnNext((arg1, arg2, arg3));

                _subscribe(Handler);

                return new Unsubscriber(() => _unsubscribe(Handler));
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
