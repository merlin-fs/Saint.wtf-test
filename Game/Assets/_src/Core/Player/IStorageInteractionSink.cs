using Game.Core.Common;
using Game.Core.Economy;

namespace Game.Core.Player
{
    public interface IStorageInteractionSink
    {
        void EnterStorage(IResourceContainer storage, StorageRole role);
        void ExitStorage(IResourceContainer storage);
    }
}