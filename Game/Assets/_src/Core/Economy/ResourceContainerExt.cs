using Game.Core.Common;

namespace Game.Core.Economy
{
    public static class ResourceContainerExt
    {
        /// <summary>
        /// Спробувати додати ресурс до контейнера миттєво, без можливості відміни.
        /// Повертає true, якщо ресурс успішно додано, і false, якщо не вистачає місця або ресурс не може бути доданий.
        /// </summary>
        public static bool TryAddInstant(this IResourceContainer container, ResourceId id)
        {
            if (!container.TryReserveAdd(id, out var r))
                return false;

            container.CommitAdd(r);
            return true;
        }

        /// <summary>
        /// Спробувати видалити ресурс з контейнера миттєво, без можливості відміни.
        /// Повертає true, якщо ресурс успішно видалено, і false, якщо ресурсу недостатньо або він не може бути видалений.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="id"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static bool TryConsume(this IResourceContainer container, ResourceId id, int amount)
        {
            if (amount <= 0) return true;

            if (container.Count(id) < amount) return false;

            for (var i = 0; i < amount; i++)
            {
                if (!container.TryBeginRemove(id, out _))
                    return false; 
            }

            return true;
        }
    }
}