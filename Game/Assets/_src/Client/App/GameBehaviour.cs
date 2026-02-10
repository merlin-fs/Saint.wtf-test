using Game.Client;
using Game.Client.App;
using Game.Client.App.Debugging;
using Game.Client.Buildings;
using Game.Client.Player;
using Game.Client.Resources;
using Game.Client.Transfers;
using Game.Client.UI.Hud;
using UnityEngine;

public class GameBehaviour : MonoBehaviour
{
    [SerializeField] private ResourceLibrary resourceLibrary;
    [SerializeField] private VisualTransferSystem visualTransfers;
    [SerializeField] private HudOverlayController hudOverlayController;
    
    [SerializeField] private BuildingView[] buildingViews;
    
    [SerializeField] private PlayerView playerView;
    
    private GameComposition _composition;
    private GameCompositionDebug _debug;
    private readonly ContainerTransformMap _map = new();

    private void Start()
    {
        _composition = new GameComposition(resourceLibrary);
        for (var i = 0; i < buildingViews.Length && i < _composition.Buildings.Count; i++)
        {
            buildingViews[i].Bind(_composition.Buildings[i]);
            buildingViews[i].BindPiles(_composition.Catalog, _composition.PlayerBundle.PlayerTransfer);
            buildingViews[i].RegisterPoints(_map);
            buildingViews[i].SpawnHud(_composition.Catalog, hudOverlayController);
        }
        
        visualTransfers.Initialize(_composition.Scheduler, _map);

        playerView.Bind(_composition.PlayerBundle.Model, _composition.Catalog, _map);
        
        _debug = new GameCompositionDebug(_composition, new GameCompositionDebug.Settings
        {
            LogTransferStarted = true,
            LogTransferFinished = true,
            LogTransferProgress = false,

            LogContainerChanged = true,
            LogContainerSummaryOnChange = true,
            MinSecondsBetweenContainerLogs = 0.1f, // щоб не спамити

            LogBuildingStateChanges = true,
            LogBuildingProducingProgress = false,

            Prefix = "[SIM]"
        });        
    }

    // Update is called once per frame
    private void Update()
    {
        _composition?.Tick(Time.deltaTime);
    }
}
