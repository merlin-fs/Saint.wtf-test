using System.Collections.Generic;
using Game.Core.Common;

namespace Game.Core.Production
{
    /// <summary>
    /// Структура, яка представляє пакет ресурсів, що складається з певного ресурсу (Resource) та його кількості (Amount).
    /// Використовується для визначення вхідних ресурсів у рецептах виробництва.
    /// </summary>
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

    /// <summary>
    /// Рецепт виробництва, який визначає, який ресурс виробляється (Output), які ресурси потрібні для виробництва (Inputs) та скільки часу займає виробництво (ProductionTimeSeconds).
    /// </summary>
    /// <param name="Output"></param>
    /// <param name="Inputs"></param>
    /// <param name="ProductionTimeSeconds"></param>
    public record Recipe(
        ResourceId Output,
        IReadOnlyList<ResourceBundle> Inputs,
        float ProductionTimeSeconds
    );
}