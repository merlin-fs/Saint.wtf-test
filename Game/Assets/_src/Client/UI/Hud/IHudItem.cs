namespace Game.Client.UI.Hud
{
    public interface IHudItem
    {
        bool IsVisible { get; }
        void SetVisible(bool visible);

        /// <summary> Оновити розміщення у світі (позиція/орієнтація). </summary>
        void UpdatePlacement(in HudPlacementContext ctx);

        /// <summary> Очистити підписки/ресурси. </summary>
        void Dispose();
    }
}