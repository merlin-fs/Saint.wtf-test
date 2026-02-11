using UnityEngine;

namespace Game.Client.UI.Hud
{
    /// <summary>
    /// Інтерфейс для отримання меж світу, який може бути використаний для позиціонування елементів інтерфейсу, таких як міні-карта або індикатори за межами екрана.
    /// </summary>
    public interface IWorldBoundsProvider
    {
        bool TryGetBounds(out Bounds bounds);
    }
}