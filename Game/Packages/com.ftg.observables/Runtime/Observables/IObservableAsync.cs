using System;
using System.Threading.Tasks;


namespace FTg.Common.Observables
{
    public interface IObservableAsync<out TArgs> : IObservable<TArgs>
    {
        IDisposable SubscribeAsync(int order, Func<TArgs, Task> handler);
        //IDisposable SubscribeAsync(Func<TArgs, Task> handler);
    }
}