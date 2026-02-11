namespace Game.Core.Common
{
    /// <summary>
    /// Причини зупинки процесу, які можуть виникнути під час виконання операцій, таких як передача ресурсів або виробництво.
    /// </summary>
    public enum StopReason : byte
    {
        None = 0,
        NoInput = 1,
        OutputFull = 2,
        TransferBlocked = 3,
    }
}