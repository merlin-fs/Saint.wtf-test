namespace Game.Core.Production
{
    public enum BuildingStatus : byte
    {
        Idle = 0,
        PullInputs = 1,
        Producing = 2,
        PushOutput = 3,
        Stopped = 4,
    }
}