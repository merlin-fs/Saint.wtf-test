using System.Collections.Generic;
using Game.Core.Common;

namespace Game.Core.Production
{
    public struct ResourceBundle
    {
        public readonly ResourceId Resource;
        public readonly int Amount;

        public ResourceBundle(ResourceId resource, int amount)
        {
            Resource = resource;
            Amount = amount;
        }
    }

    // рецепт (можна розширити)
    public record Recipe(
        ResourceId Output,
        IReadOnlyList<ResourceBundle> Inputs,
        float ProductionTimeSeconds
    );
}