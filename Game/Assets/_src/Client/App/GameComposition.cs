using System.Collections.Generic;
using Game.Client.Player;
using Game.Client.Resources;
using Game.Core.Common;
using Game.Core.Economy;
using Game.Core.Player;
using Game.Core.Production;
using Game.Core.Transfers;

namespace Game.Client.App
{
    // Цей клас - "композиція" гри, яка збирає всі частини разом (каталог, будівлі, FSM-и, scheduler).
    // тестова заміна DI
    public sealed class GameComposition
    {
        public IResourceCatalog Catalog { get; }
        public TransferScheduler Scheduler { get; }
        public List<BuildingModel> Buildings { get; }
        public BuildingProductionSystem ProductionSystem { get; }
        public PlayerBundle PlayerBundle { get; }

        // Можеш також зберігати bundles (model+fsm), якщо потрібно Dispose
        private readonly List<BuildingFsm> _fsms = new();

        public GameComposition(ResourceLibrary resourceLibrary)
        {
            var n1 = new ResourceId(1);
            var n2 = new ResourceId(2);
            var n3 = new ResourceId(3);
            
            Catalog = new ResourceCatalog(resourceLibrary.All);

            // Scheduler (і stream)
            Scheduler = new TransferScheduler();
            ITransferScheduler scheduler = Scheduler;
            ITransferStream stream = Scheduler;

            // Recipes
            var r1 = new Recipe(
                Output: n1,
                ProductionTimeSeconds: 2.0f,
                Inputs: new List<ResourceBundle>()
            );

            var r2 = new Recipe(
                Output: n2,
                ProductionTimeSeconds: 3.5f,
                Inputs: new List<ResourceBundle>
                {
                    new ResourceBundle(n1, 2),
                }
            );

            var r3 = new Recipe(
                Output: n3,
                ProductionTimeSeconds: 6.0f,
                Inputs: new List<ResourceBundle>
                {
                    new ResourceBundle(n1, 3),
                    new ResourceBundle(n2, 2),
                }
            );
            
            var rWarehouse = new Recipe(
                Output: new ResourceId(0),   
                ProductionTimeSeconds: 0f,
                Inputs: new List<ResourceBundle>()
                {
                    new ResourceBundle(n3, 0),                    
                }
            );
            

            // Buildings
            var b1 = BuildingFactory.Create(
                catalog: Catalog,
                id: new BuildingId(1),
                recipe: r1,
                inputStorageCapacity: 0,     
                outputStorageCapacity: 20,
                inputTransferSecondsPerUnit: 0.55f,
                outputTransferSecondsPerUnit: 0.55f,
                scheduler: scheduler,
                stream: stream,
                debugNamePrefix: "B1"
            );

            var b2 = BuildingFactory.Create(
                catalog: Catalog,
                id: new BuildingId(2),
                recipe: r2,
                inputStorageCapacity: 20,
                outputStorageCapacity: 20,
                inputTransferSecondsPerUnit: 0.65f,
                outputTransferSecondsPerUnit: 0.65f,
                scheduler: scheduler,
                stream: stream,
                debugNamePrefix: "B2"
            );

            var b3 = BuildingFactory.Create(
                catalog: Catalog,
                id: new BuildingId(3),
                recipe: r3,
                inputStorageCapacity: 20,
                outputStorageCapacity: 20,
                inputTransferSecondsPerUnit: 1.25f,
                outputTransferSecondsPerUnit: 1.25f,
                scheduler: scheduler,
                stream: stream,
                debugNamePrefix: "B3"
            );
            
            var bWarehouse = BuildingFactory.Create(
                catalog: Catalog,
                id: new BuildingId(4),
                recipe: rWarehouse,
                inputStorageCapacity: 999,
                outputStorageCapacity: 0,
                inputTransferSecondsPerUnit: 0.25f,
                outputTransferSecondsPerUnit: 0f,
                scheduler: scheduler,
                stream: stream,
                debugNamePrefix: "Warehouse"
            );
            

            Buildings = new List<BuildingModel> { b1.Model, b2.Model, b3.Model, bWarehouse.Model };

            _fsms.Add(b1.Fsm);
            _fsms.Add(b2.Fsm);
            _fsms.Add(b3.Fsm);

            // Production system
            ProductionSystem = new BuildingProductionSystem(_fsms);

            PlayerBundle = BuildPlayer(Catalog, 
                inventoryCapacity: 10, 
                scheduler, 
                stream, 
                pickupSecondsPerUnit: 0.25f, 
                dropSecondsPerUnit: 0.25f);
            
            
            // стартовий ресурс, щоб цикл запустився (можна прибрати)
            b2.Model.InputStorage.TryAddInstant(n1);
            b2.Model.InputStorage.TryAddInstant(n1);
            b3.Model.InputStorage.TryAddInstant(n1);
            b3.Model.InputStorage.TryAddInstant(n2);
        }
        
        private PlayerBundle BuildPlayer(IResourceCatalog catalog,
            int inventoryCapacity,
            ITransferScheduler scheduler,
            ITransferStream stream,
            float pickupSecondsPerUnit,
            float dropSecondsPerUnit)
        {
            var inventory = new StorageModel(catalog, inventoryCapacity, "Player.Inventory");
            var model = new PlayerModel(inventory);

            var carry = new SimplePlayerTransferSystem(
                player: model,
                catalog: catalog,
                scheduler: scheduler,
                stream: stream,
                pickupSecondsPerUnit: pickupSecondsPerUnit,
                dropSecondsPerUnit: dropSecondsPerUnit
            );

            return new PlayerBundle(model, carry);
        }
        
        
        public void Tick(float dt)
        {
            // scheduler (прогрес transfer-ів)
            Scheduler.Tick(dt);
            // production (FSM-и, які створюють нові transfers)
            ProductionSystem.Tick(dt);
            // player (прогрес transfer-ів)
            PlayerBundle.PlayerTransfer.Tick(dt);            
        }
    }
}
