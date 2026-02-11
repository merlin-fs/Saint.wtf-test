namespace Game.Core.Common
{
    /// <summary>
    /// Система, яка виконує оновлення кожного кадру або з певним інтервалом часу.
    /// </summary>
    public interface ITickSystem
    {
        void Tick(float dt);
    }
}