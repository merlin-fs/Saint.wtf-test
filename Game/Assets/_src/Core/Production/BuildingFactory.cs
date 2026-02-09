using Game.Core.Common;
using Game.Core.Economy;
using Game.Core.Transfers;

namespace Game.Core.Production
{
    public record BuildingBundle(BuildingModel Model, BuildingFsm Fsm);

    public static class BuildingFactory
    {
        public static BuildingBundle Create(
            IResourceCatalog catalog,
            BuildingId id,
            Recipe recipe,
            int inputStorageCapacity,
            int outputStorageCapacity,
            float inputTransferSecondsPerUnit,
            float outputTransferSecondsPerUnit,
            ITransferScheduler scheduler,
            ITransferStream stream,
            string debugNamePrefix)
        {
            // 2 склади
            var inputStorage  = new StorageModel(catalog, inputStorageCapacity,  $"{debugNamePrefix}.input");
            var outputStorage = new StorageModel(catalog, outputStorageCapacity, $"{debugNamePrefix}.output");

            // Порти (внутрішні буфери будівлі)
            // InPort: має вмістити всі інпути за цикл (0 якщо інпутів нема → зробимо хоча б 1)
            var inPortCap = recipe.TotalInputUnits();
            if (inPortCap < 1) inPortCap = 1;

            // OutPort: у нашій схемі 1 output за цикл → 1
            var outPortCap = 1;

            var inPort  = new StorageModel(catalog, inPortCap,  $"{debugNamePrefix}.inPort");
            var outPort = new StorageModel(catalog, outPortCap, $"{debugNamePrefix}.outPort");

            // Модель
            var model = new BuildingModel(
                id: id,
                recipe: recipe,
                inputStorage: inputStorage,
                outputStorage: outputStorage,
                inPort: inPort,
                outPort: outPort,
                inputTransferSecondsPerUnit: inputTransferSecondsPerUnit,
                outputTransferSecondsPerUnit: outputTransferSecondsPerUnit
            );

            // FSM (універсальна FSM + building states)
            var fsm = new BuildingFsm(model, scheduler, stream);

            return new BuildingBundle(model, fsm);
        }
    }
}