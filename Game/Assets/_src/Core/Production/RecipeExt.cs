using System;
using System.Linq;
using Game.Core.Common;

namespace Game.Core.Production
{
    /// <summary>
    /// Розширення для класу Recipe, що надає додаткові методи для роботи з інгредієнтами рецепту, такими як перевірка наявності інгредієнтів,
    /// підрахунок загальної кількості одиниць та ітерація по інгредієнтах.
    /// </summary>
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