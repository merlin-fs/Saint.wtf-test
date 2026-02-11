using Game.Core.Common;
using Game.Core.Economy;

namespace Game.Core.Transfers
{
    /// <summary>
    /// Подія, що сигналізує про початок передачі ресурсу між двома контейнерами.
    /// Вона містить інформацію про ідентифікатор передачі, джерело та призначення, тип ресурсу, тривалість передачі та додаткові дані (Tag),
    /// які можуть бути використані для зберігання будь-якої додаткової інформації, пов'язаної з цією передачею.
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Source"></param>
    /// <param name="Destination"></param>
    /// <param name="Resource"></param>
    /// <param name="DurationSeconds"></param>
    /// <param name="Tag"></param>
    public record TransferStarted(
        TransferId Id,
        IResourceContainer Source,
        IResourceContainer Destination,
        ResourceId Resource,
        float DurationSeconds,
        object Tag
    );

    /// <summary>
    /// Подія, що сигналізує про оновлення прогресу передачі ресурсу.
    /// Вона містить інформацію про ідентифікатор передачі та поточний прогрес у вигляді числа від 0 до 1, де 0 означає початок передачі, а 1 - її завершення.
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Progress"></param>
    public record TransferProgress(TransferId Id, float Progress); // 0..1

    /// <summary>
    /// Подія, що сигналізує про завершення передачі ресурсу між двома контейнерами.
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Status"></param>
    public record TransferFinished(TransferId Id, TransferStatus Status);
}