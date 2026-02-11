using JetBrains.Annotations;

namespace Game.Core.Common
{
    /// <summary>
    /// Ідентифікатор ресурсу. Використовується для посилання на конкретний тип ресурсу в грі.
    /// </summary>
    public readonly struct ResourceId : System.IEquatable<ResourceId>, System.IComparable<ResourceId>
    {
        public readonly int Value;
        public ResourceId(int value) => Value = value;

        public bool Equals([CanBeNull] ResourceId other) => Value == other.Value;
        public override bool Equals([CanBeNull] object obj) => obj is ResourceId other && Equals(other);
        public override int GetHashCode() => Value;
        public int CompareTo(ResourceId other) => Value.CompareTo(other.Value);

        public static bool operator ==(ResourceId a, ResourceId b) => a.Value == b.Value;
        public static bool operator !=(ResourceId a, ResourceId b) => a.Value != b.Value;

        public override string ToString() => Value.ToString();
    }    
}
