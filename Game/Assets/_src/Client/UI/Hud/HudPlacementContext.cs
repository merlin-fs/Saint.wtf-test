using UnityEngine;

namespace Game.Client.UI.Hud
{
    public readonly struct HudPlacementContext
    {
        public readonly Camera WorldCamera;

        public HudPlacementContext(Camera worldCamera)
        {
            WorldCamera = worldCamera;
        }
    }
}