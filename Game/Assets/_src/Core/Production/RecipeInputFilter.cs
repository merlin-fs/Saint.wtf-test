using System.Collections.Generic;
using System.Linq;
using Game.Core.Common;
using Game.Core.Economy;

namespace Game.Core.Production
{
    public sealed class RecipeInputFilter : IResourceTransferFilter
    {
        private readonly IReadOnlyList<ResourceId> _resourceBundles;

        public RecipeInputFilter(in Recipe recipe)
        {
            _resourceBundles = recipe.Inputs.Select(id => id.Resource).ToList();
        }

        public RecipeInputFilter(params ResourceId[] resources)
        {
            _resourceBundles = resources;
        }

        public bool Allows(ResourceId id) => _resourceBundles.Contains(id);
    }
}