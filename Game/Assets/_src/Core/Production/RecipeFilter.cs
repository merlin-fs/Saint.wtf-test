using System.Collections.Generic;
using System.Linq;
using Game.Core.Common;
using Game.Core.Economy;

namespace Game.Core.Production
{
    public sealed class RecipeFilter : IResourceTransferFilter
    {
        private readonly IReadOnlyList<ResourceId> _resourceBundles;

        public static RecipeFilter FromInputRecipe(in Recipe recipe)
        {
            return new RecipeFilter(recipe.Inputs.Select(id => id.Resource).ToArray());
        }

        public RecipeFilter(params ResourceId[] resources)
        {
            _resourceBundles = resources;
        }

        public bool Allows(ResourceId id) => _resourceBundles.Contains(id);
    }
}