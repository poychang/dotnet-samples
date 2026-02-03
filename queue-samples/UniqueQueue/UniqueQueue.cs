using System.Collections;
using System.Text.Json;

namespace UniqueQueue
{
    public sealed class UniqueQueue<T> : IReadOnlyCollection<T>
    {
        /// <summary>
        /// 維持先進先出順序的儲存本體。
        /// </summary>
        private readonly Queue<T> _queue = new();
        /// <summary>
        /// 判斷重複佔位用，避免重複入列。
        /// </summary>
        private readonly HashSet<T> _set;

        /// <summary>
        /// 建立唯一佇列，可提供自訂相等比較器。
        /// </summary>
        /// <param name="comparer">用於判定元素是否相等的比較器。</param>
        public UniqueQueue(IEqualityComparer<T>? comparer = null)
            => _set = new HashSet<T>(comparer);

        /// <summary>
        /// 取得目前佇列中的元素數量。
        /// </summary>
        public int Count => _queue.Count;

        /// <summary>
        /// 指示佇列是否為空。
        /// </summary>
        public bool IsEmpty => _queue.Count == 0;

        /// <summary>
        /// 將元素加入佇列；若已存在則不加入並回傳 false。
        /// </summary>
        /// <param name="item">要加入的元素。</param>
        /// <returns>成功入列回傳 true；若元素已存在回傳 false。</returns>
        /// <exception cref="ArgumentNullException">當 <paramref name="item"/> 為 null。</exception>
        public bool Enqueue(T item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            if (!_set.Add(item)) return false; // Add 同時檢查是否有重複
            _queue.Enqueue(item);
            return true;
        }

        /// <summary>
        /// 嘗試將元素加入佇列；若已存在則回傳 false。
        /// </summary>
        /// <param name="item">要加入的元素。</param>
        /// <returns>成功入列回傳 true；若元素已存在回傳 false。</returns>
        /// <exception cref="ArgumentNullException">當 <paramref name="item"/> 為 null。</exception>
        public bool TryEnqueue(T item) => Enqueue(item);

        /// <summary>
        /// 取出並移除佇列前端的元素。
        /// </summary>
        /// <returns>佇列前端的元素。</returns>
        /// <exception cref="InvalidOperationException">當佇列為空。</exception>
        public T Dequeue()
        {
            if (_queue.Count == 0) throw new InvalidOperationException("Queue is empty.");

            var item = _queue.Dequeue();
            _set.Remove(item); // 解除佔位，之後允許再次入列
            return item;
        }

        /// <summary>
        /// 嘗試取出並移除佇列前端的元素。
        /// </summary>
        /// <param name="item">若成功則為取出的元素；失敗為預設值。</param>
        /// <returns>佇列非空且成功取出時回傳 true；否則 false。</returns>
        public bool TryDequeue(out T? item)
        {
            if (_queue.Count == 0)
            {
                item = default;
                return false;
            }

            item = _queue.Dequeue();
            _set.Remove(item);
            return true;
        }

        /// <summary>
        /// 檢視佇列前端的元素但不移除。
        /// </summary>
        /// <returns>佇列前端的元素。</returns>
        /// <exception cref="InvalidOperationException">當佇列為空。</exception>
        public T Peek()
        {
            if (_queue.Count == 0) throw new InvalidOperationException("Queue is empty.");
            return _queue.Peek();
        }

        /// <summary>
        /// 嘗試檢視佇列前端的元素但不移除。
        /// </summary>
        /// <param name="item">若成功則為前端元素；失敗為預設值。</param>
        /// <returns>佇列非空且取得元素時回傳 true；否則 false。</returns>
        public bool TryPeek(out T? item)
        {
            if (_queue.Count == 0)
            {
                item = default;
                return false;
            }

            item = _queue.Peek();
            return true;
        }

        /// <summary>
        /// 判斷佇列是否包含指定元素。
        /// </summary>
        /// <param name="item">要尋找的元素。</param>
        /// <returns>若找到則為 true；否則 false。</returns>
        /// <exception cref="ArgumentNullException">當 <paramref name="item"/> 為 null。</exception>
        public bool Contains(T item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));
            return _set.Contains(item);
        }

        /// <summary>
        /// 將佇列內容複製為陣列（維持順序）。
        /// </summary>
        /// <returns>包含所有元素的新陣列。</returns>
        public T[] ToArray() => [.. _queue];

        /// <summary>
        /// 清空佇列並移除所有判重佔位。
        /// </summary>
        public void Clear()
        {
            _queue.Clear();
            _set.Clear();
        }

        /// <summary>
        /// 依序傳回佇列的列舉器。
        /// </summary>
        /// <returns>列舉器。</returns>
        public IEnumerator<T> GetEnumerator() => _queue.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public sealed class UniqueQueueComparer<T> : IEqualityComparer<T>
    {
        private static readonly JsonSerializerOptions Options = new() { WriteIndented = false };

        /// <summary>
        /// 比較兩個物件的序列化結果是否相等（序位比較）。
        /// </summary>
        /// <param name="x">第一個物件。</param>
        /// <param name="y">第二個物件。</param>
        /// <returns>相等為 true；否則 false。</returns>
        public bool Equals(T? x, T? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;

            var left = Serialize(x);
            var right = Serialize(y);
            return StringComparer.Ordinal.Equals(left, right);
        }

        /// <summary>
        /// 取得物件序列化結果的雜湊碼（序位比較）。
        /// </summary>
        /// <param name="obj">目標物件。</param>
        /// <returns>雜湊碼。</returns>
        /// <exception cref="ArgumentNullException">當 <paramref name="obj"/> 為 null。</exception>
        public int GetHashCode(T obj)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));
            return StringComparer.Ordinal.GetHashCode(Serialize(obj));
        }

        private static string Serialize(T value) => JsonSerializer.Serialize(value, Options);
    }
}
