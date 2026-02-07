using Game.Core.Common;

namespace Game.Core.Economy
{
    public record RemoveToken(ResourceId Resource);
    public record AddReservation(ResourceId Resource);
}
