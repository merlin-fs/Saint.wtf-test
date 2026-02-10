using Game.Core.Common;
using Game.Core.Economy;

namespace Game.Core.Player
{
    public interface IStorageInteractionSink
    {
        void EnterStorage(IResourceContainer storage, StorageRole role, IResourceTransferFilter filter = null);
        void ExitStorage(IResourceContainer storage);
    }
}