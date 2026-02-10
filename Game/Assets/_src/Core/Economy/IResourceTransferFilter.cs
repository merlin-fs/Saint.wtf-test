using Game.Core.Common;

namespace Game.Core.Economy
{
    public interface IResourceTransferFilter
    {
        bool Allows(ResourceId id);
    }
}