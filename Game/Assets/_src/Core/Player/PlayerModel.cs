using System;
using Game.Core.Economy;

namespace Game.Core.Player
{
    public sealed class PlayerModel : IPlayer
    {
        public PlayerModel(IResourceContainer inventory)
        {
            Inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
        }

        public IResourceContainer Inventory { get; }
    }    
}