using Game.Client.Common;
using Game.Core.Common;
using Game.Core.Player;
using UnityEngine;

namespace Game.Client.Player
{
    public record PlayerBundle(PlayerModel Model, SimplePlayerTransferSystem PlayerTransfer);
    
    public sealed class PlayerView : MonoBehaviour
    {
        [SerializeField] private Transform inventoryPoint;      
        [SerializeField] private StoragePileView inventoryStack;
        

        public void Bind(PlayerModel player, IResourceCatalog catalog, ContainerTransformMap map)
        {
            map.Register(player.Inventory, inventoryPoint);
            inventoryStack.Bind(catalog, player.Inventory);
        }
    }
}