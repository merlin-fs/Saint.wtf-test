using System;

namespace Game.Core.Transfers
{
    /// <summary>
    /// Інтерфейс для потока передач ресурсів між контейнерами.
    /// Він надає спостережувані події для початку, прогресу та завершення передачі, дозволяючи відстежувати стан передачі в реальному часі.
    /// </summary>
    public interface ITransferStream
    {
        IObservable<TransferStarted> Started { get; }
        IObservable<TransferProgress> Progress { get; }
        IObservable<TransferFinished> Finished { get; }
    }
}