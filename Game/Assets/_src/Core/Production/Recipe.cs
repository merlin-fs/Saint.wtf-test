using System.Collections.Generic;
using Game.Core.Common;

namespace Game.Core.Production
{
    public struct ResourceBundle
    {
        public ResourceId Resource;
        public int Amount;

        public ResourceBundle(ResourceId resource, int amount)
        {
            Resource = resource;
            Amount = amount;
        }
    }

    // Гнучкий рецепт (можна розширити до списку входів)
    public record Recipe(
        ResourceId Output,
        IReadOnlyList<ResourceBundle> Inputs,
        float ProductionTimeSeconds
    );
}