using Game.Client.Common;
using Game.Client.UI.Hud;
using Game.Core.Common;
using Game.Core.Economy;
using Game.Core.Player;
using Game.Core.Production;
using UnityEngine;

namespace Game.Client.Buildings
{
    /// <summary>
    /// Відповідає за візуалізацію будівлі, її складів та тригерів.
    /// </summary>
    public class BuildingView : MonoBehaviour
    {
        [SerializeField] private StoragePileView inputPile;
        [SerializeField] private StoragePileView outputPile;
        
        [SerializeField] private StorageTrigger inputTrigger;
        [SerializeField] private StorageTrigger outputTrigger;
        
        [SerializeField] private StorageHudItem prefabInputStorageHud;
        [SerializeField] private StorageHudItem prefabOutputStorageHud;
        [SerializeField] private BuildingHudItem prefabBuildingHud;

        private IResourceTransferFilter _inputFilter;
        private IResourceTransferFilter _outputFilter;
        
        public BuildingModel Model { get; private set; }

        public void Bind(BuildingModel model)
        {
            Model = model;
            _inputFilter = RecipeFilter.FromInputRecipe(model.Recipe);
            _outputFilter = new RecipeFilter(model.Recipe.Output);
        }

        public void BindPiles(IResourceCatalog catalog, IStorageInteractionSink sink)
        {
            inputPile?.Bind(catalog, Model.InputStorage);
            outputPile?.Bind(catalog, Model.OutputStorage);
            
            inputTrigger?.Bind(Model.InputStorage, sink, _inputFilter);
            outputTrigger?.Bind(Model.OutputStorage, sink);
        }

        public void SpawnHud(IResourceCatalog catalog, HudOverlayController hudOverlayController)
        {
            if (inputPile)
            {
                var bh = hudOverlayController.Spawn<StorageHudItem>(prefabInputStorageHud);
                bh.Bind(Model.InputStorage, _inputFilter);
                bh.BindPlacement(inputPile.transform);
            }
            if (outputPile)
            {
                var bh = hudOverlayController.Spawn<StorageHudItem>(prefabOutputStorageHud);
                bh.Bind(Model.OutputStorage, _outputFilter);
                bh.BindPlacement(outputPile.transform);
            }
            {
                var bh = hudOverlayController.Spawn<BuildingHudItem>(prefabBuildingHud);
                bh.Bind(Model, catalog);
                bh.BindPlacement(transform);
            }
        }


        public void RegisterPoints(ContainerTransformMap map)
        {
            if (Model == null) return;
            if (inputPile)
            {
                map.Register(Model.InputStorage, inputPile.transform);
                map.Register(Model.InPort, inputPile.transform);
            }
            if (outputPile)
            {
                map.Register(Model.OutputStorage, outputPile.transform);
                map.Register(Model.OutPort, outputPile.transform);
            }
        }        
    }
}