using Game.Core.Common;

namespace Game.Core.Economy
{
    /// <summary>
    /// Операції, які можуть бути виконані над ресурсами.
    /// </summary>
    public record RemoveToken(ResourceId Resource);

    /// <summary>
    /// Операції, які можуть бути виконані над ресурсами.
    /// </summary>
    public record AddReservation(ResourceId Resource);
}
