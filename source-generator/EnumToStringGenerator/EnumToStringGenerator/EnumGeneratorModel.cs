using System.Collections;

namespace EnumToStringGenerator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Namespace">產出程式碼的命名空間</param>
    /// <param name="TypeName">產出程式碼的型別名稱</param>
    /// <param name="EnumTypeName">對應的 Enum 型別名稱</param>
    /// <param name="Fields">對應的 Enum 欄位成員名稱</param>
    public record struct EnumGeneratorModel(string Namespace, string TypeName, string EnumTypeName, SequenceEquatableCollection<string> Fields);

    public class SequenceEquatableCollection<T> : IEquatable<SequenceEquatableCollection<T>>, IEnumerable<T> where T : IEquatable<T>
    {
        private readonly T[] _value;

        public SequenceEquatableCollection(T[] value) => _value = value ?? throw new ArgumentNullException(nameof(value));
        public override bool Equals(object? obj) => obj is SequenceEquatableCollection<T> collection && Equals(collection);
        public bool Equals(SequenceEquatableCollection<T> other)
        {
            if (other == null) return false;
            if (_value.Length != other._value.Length) return false;
            return _value.SequenceEqual(other._value);
        }

        public override int GetHashCode() => _value.Aggregate(0, (current, item) => current ^ item.GetHashCode());

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_value).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private T[] AsArray() => _value.ToArray();

        public static bool operator ==(SequenceEquatableCollection<T>? left, SequenceEquatableCollection<T>? right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(SequenceEquatableCollection<T>? left, SequenceEquatableCollection<T>? right) => !(left == right);

        // 隱含轉換 array => SequenceEquatableCollection<T>
        public static implicit operator SequenceEquatableCollection<T>(T[] value) => new SequenceEquatableCollection<T>(value);

        // 顯式轉換 SequenceEquatableCollection<T> => array
        public static explicit operator T[](SequenceEquatableCollection<T> value) => value.AsArray();
    }
}
