using System;

namespace Game.Core.Transfers
{
    public interface ITransferStream
    {
        IObservable<TransferStarted> Started { get; }
        IObservable<TransferProgress> Progress { get; }
        IObservable<TransferFinished> Finished { get; }
    }
}