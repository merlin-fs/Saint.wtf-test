using UnityEngine;

namespace Game.Client.UI.Hud
{
    public interface IWorldBoundsProvider
    {
        bool TryGetBounds(out Bounds bounds);
    }
}