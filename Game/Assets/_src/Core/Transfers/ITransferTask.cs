using Game.Core.Common;

namespace Game.Core.Transfers
{
    /// <summary>
    /// Інтерфейс, який представляє завдання передачі ресурсу між двома контейнерами.
    /// Він містить інформацію про ідентифікатор передачі, ресурс, тривалість, прогрес та статус передачі.
    /// </summary>
    public interface ITransferTask
    {
        TransferId Id { get; }
        ResourceId Resource { get; }
        float DurationSeconds { get; }
        float Progress { get; } // 0..1
        TransferStatus Status { get; }
    }
}