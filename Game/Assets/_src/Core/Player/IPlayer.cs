using Game.Core.Economy;

namespace Game.Core.Player
{
    public interface IPlayer
    {
        IResourceContainer Inventory { get; }
    }
}