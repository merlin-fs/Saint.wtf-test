using Game.Core.Common;
using Game.Core.Economy;

namespace Game.Core.Player
{
    /// <summary>
    /// Інтерфейс для обробки взаємодії з ресурсними складами.
    /// Забезпечує методи для входу та виходу зі складу, а також можливість застосування фільтрів для обмеження доступних ресурсів.
    /// </summary>
    public interface IStorageInteractionSink
    {
        void EnterStorage(IResourceContainer storage, StorageRole role, IResourceTransferFilter filter = null);
        void ExitStorage(IResourceContainer storage);
    }
}