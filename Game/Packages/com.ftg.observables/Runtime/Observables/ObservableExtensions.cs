using System;

namespace FTg.Common.Observables
{

    public static class ObservableExtensions
    {
        public static IDisposable Subscribe(this IObservable<Unit> observable, Action callback)
        {
            return observable.Subscribe(new Observer<Unit>(_ => callback()));
        }

        public static IDisposable Subscribe<T>(this IObservable<T> observable, Action<T> callback)
        {
            return observable.Subscribe(new Observer<T>(callback));
        }

        public static IDisposable Subscribe<T1, T2>(this IObservable<(T1, T2)> observable, Action<T1, T2> callback)
        {
            return observable.Subscribe(new Observer<(T1, T2)>(args => callback(args.Item1, args.Item2)));
        }

        public static IDisposable Subscribe<T1, T2, T3>(this IObservable<(T1, T2, T3)> observable, Action<T1, T2, T3> callback)
        {
            return observable.Subscribe(new Observer<(T1, T2, T3)>(args => callback(args.Item1, args.Item2, args.Item3)));
        }


        public static IDisposable Once(this IObservable<Unit> observable, Action callback)
        {
            IDisposable subscription = null;

            subscription = observable.Subscribe(_ =>
            {
                callback();
                subscription.Dispose();
            });

            return subscription;
        }

        public static IDisposable Once<T>(this IObservable<T> observable, Action<T> callback)
        {
            IDisposable subscription = null;

            subscription = observable.Subscribe(value =>
            {
                callback(value);
                subscription.Dispose();
            });

            return subscription;
        }

        public static IDisposable Once<T1, T2>(this IObservable<(T1, T2)> observable, Action<T1, T2> callback)
        {
            IDisposable subscription = null;

            subscription = observable.Subscribe(value =>
            {
                callback(value.Item1, value.Item2);
                subscription.Dispose(); 
            });

            return subscription;
        }

        public static IDisposable Once<T1, T2, T3>(this IObservable<(T1, T2, T3)> observable, Action<T1, T2, T3> callback)
        {
            IDisposable subscription = null;

            subscription = observable.Subscribe(value =>
            {
                callback(value.Item1, value.Item2, value.Item3);
                subscription.Dispose();
            });

            return subscription;
        }

        private class Observer<T> : IObserver<T>
        {
            private readonly Action<T> _onNext;

            public Observer(Action<T> onNext)
            {
                _onNext = onNext;
            }

            public void OnNext(T value) => _onNext?.Invoke(value);

            public void OnError(Exception error) { /* Not used */ }

            public void OnCompleted() { /* Not used */ }
        }
    }

    public readonly struct Unit
    {
        public static readonly Unit Default = default;
    }
}
