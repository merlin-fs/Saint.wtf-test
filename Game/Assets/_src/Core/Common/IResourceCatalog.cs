namespace Game.Core.Common
{
    /// <summary>
    /// Каталог ресурсів, який відповідає за зберігання інформації про всі ресурси в грі.
    /// </summary>
    public interface IResourceCatalog
    {
        int Count { get; }

        bool Contains(ResourceId id);
        ResourceDef GetDef(ResourceId id);

        // Для швидких контейнерів (int[] counts):
        int ToIndex(ResourceId id); // throw або -1
        ResourceId FromIndex(int index);
    }
}