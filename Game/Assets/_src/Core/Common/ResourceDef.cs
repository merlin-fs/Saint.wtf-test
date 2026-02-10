using System;

namespace Game.Core.Common
{
    [Serializable]
    public struct ResourceDef
    {
        public ResourceId Id;
        public string Key;
        public string Name;
        public int StackLimit;
    }
}