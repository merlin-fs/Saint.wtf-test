namespace Game.Core.Common
{
    public enum StopReason : byte
    {
        None = 0,
        NoInput = 1,
        OutputFull = 2,
        TransferBlocked = 3,
    }
}