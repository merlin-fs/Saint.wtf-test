using Game.Core.Economy;

namespace Game.Core.Player
{
    /// <summary>
    /// Інтерфейс, що представляє гравця в грі, який має інвентар для зберігання ресурсів та предметів.
    /// </summary>
    public interface IPlayer
    {
        IResourceContainer Inventory { get; }
    }
}