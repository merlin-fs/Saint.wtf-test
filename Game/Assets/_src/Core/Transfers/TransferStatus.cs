namespace Game.Core.Transfers
{
    public enum TransferStatus : byte
    {
        Running = 0,
        Completed = 1,
        Cancelled = 2,
        Failed = 3,
    }
}