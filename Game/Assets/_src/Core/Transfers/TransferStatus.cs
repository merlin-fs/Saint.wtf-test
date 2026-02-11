namespace Game.Core.Transfers
{
    /// <summary>
    /// Статус передачі ресурсів між складами або гравцями.
    /// Визначає поточний стан процесу передачі, який може бути в процесі виконання, завершений, скасований або неуспішний.
    /// </summary>
    public enum TransferStatus : byte
    {
        Running = 0,
        Completed = 1,
        Cancelled = 2,
        Failed = 3,
    }
}