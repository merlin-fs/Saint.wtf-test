namespace Game.Core.Common
{
    /// <summary>
    /// Визначає роль складу: чи він є джерелом (Input) чи приймачем (Output) ресурсів.
    /// </summary>
    public enum StorageRole : byte
    {
        Input = 0,
        Output = 1,
    }
}