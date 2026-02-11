using Game.Client.Player;
using Game.Core.Common;
using Game.Core.Economy;
using Game.Core.Player;
using UnityEngine;

namespace Game.Client.Buildings
{
    /// <summary>
    /// Тригер для взаємодії гравця зі складом будівлі.
    /// Відповідає за повідомлення про входження/виходження гравця в зону складу.
    /// </summary>
    public class StorageTrigger : MonoBehaviour
    {
        [SerializeField] private StorageRole role;

        private IResourceContainer _storage;
        private IStorageInteractionSink _sink;
        private IResourceTransferFilter _filter;
        

        public void Bind(IResourceContainer storage, IStorageInteractionSink sink, IResourceTransferFilter filter = null)
        {
            _storage = storage;
            _sink = sink;
            _filter = filter;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_storage == null || _sink == null) return;
            if (other.GetComponentInParent<PlayerView>() == null) return;

            _sink.EnterStorage(_storage, role, _filter);
        }

        private void OnTriggerExit(Collider other)
        {
            if (_storage == null || _sink == null) return;
            if (other.GetComponentInParent<PlayerView>() == null) return;

            _sink.ExitStorage(_storage);
        }
    }    
}