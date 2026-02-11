using Game.Core.Common;

namespace Game.Core.Economy
{
    /// <summary>
    /// Інтерфейс для фільтрації ресурсів під час передачі. Використовується для визначення, які ресурси можуть бути передані між контейнерами.
    /// </summary>
    public interface IResourceTransferFilter
    {
        bool Allows(ResourceId id);
    }
}