using System;
using System.Linq;
using Game.Core.Common;

namespace Game.Core.Production
{
    public static class RecipeExt
    {
        public static bool HasInputs(this Recipe recipe) => recipe.Inputs?.Count > 0;

        public static int TotalInputUnits(this Recipe recipe) => recipe.Inputs?.Sum(input => input.Amount) ?? 0;

        public static void ForEachInput(this Recipe recipe, Action<ResourceId, int> visit)
        {
            if (recipe.Inputs == null) return;
            foreach (var input in recipe.Inputs)
            {
                visit(input.Resource, input.Amount);
            }
        }
    }
}