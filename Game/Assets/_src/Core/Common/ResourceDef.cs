using System;

namespace Game.Core.Common
{
    /// <summary>
    /// Визначає ресурс в грі, включаючи його унікальний ідентифікатор, ключ для локалізації, назву та обмеження на стекування.
    /// </summary>
    [Serializable]
    public struct ResourceDef
    {
        public ResourceId Id;
        public string Key;
        public string Name;
        public int StackLimit;
    }
}