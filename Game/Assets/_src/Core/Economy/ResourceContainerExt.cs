using Game.Core.Common;

namespace Game.Core.Economy
{
    public static class ResourceContainerExt
    {
        /// <summary>
        /// Instant add without transfer (internal building logic).
        /// Uses reserve+commit to keep invariants consistent.
        /// </summary>
        public static bool TryAddInstant(this IResourceContainer container, ResourceId id)
        {
            if (!container.TryReserveAdd(id, out var r))
                return false;

            container.CommitAdd(r);
            return true;
        }

        /// <summary>
        /// Consume N units instantly (no visualization). Returns false if not enough.
        /// Removes items permanently (does not cancel).
        /// </summary>
        public static bool TryConsume(this IResourceContainer container, ResourceId id, int amount)
        {
            if (amount <= 0) return true;

            // quick precheck
            if (container.Count(id) < amount) return false;

            for (var i = 0; i < amount; i++)
            {
                if (!container.TryBeginRemove(id, out _))
                    return false; // should not happen after precheck
            }

            return true;
        }
    }
}