using System;
using System.Collections.Generic;
using System.Linq;

namespace BaldurToolkit.DataTables.Indexes
{
    public class UniqueIndex<TKey, TElement>
    {
        public UniqueIndex(IEnumerable<TElement> items, Func<TElement, TKey> keyExpression)
        {
            this.Lookup = items.ToDictionary(keyExpression);
        }

        public UniqueIndex(IEnumerable<TElement> items, Func<TElement, TKey> keyExpression, IEqualityComparer<TKey> comparer)
        {
            this.Lookup = items.ToDictionary(keyExpression, comparer);
        }

        public int Count => this.Lookup.Count;

        public ICollection<TKey> Keys => this.Lookup.Keys;

        public ICollection<TElement> Values => this.Lookup.Values;

        protected IDictionary<TKey, TElement> Lookup { get; }

        public TElement this[TKey key]
        {
            get
            {
                if (this.TryGetValue(key, out var value))
                {
                    return value;
                }

                return default(TElement);
            }
        }

        public bool ContainsKey(TKey key)
        {
            return this.Lookup.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TElement value)
        {
            return this.Lookup.TryGetValue(key, out value);
        }
    }
}
