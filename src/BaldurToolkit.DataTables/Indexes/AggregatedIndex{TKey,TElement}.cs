using System;
using System.Collections.Generic;
using System.Linq;

namespace BaldurToolkit.DataTables.Indexes
{
    public class AggregatedIndex<TKey, TElement>
    {
        public AggregatedIndex(IEnumerable<TElement> items, Func<TElement, TKey> keyExpression)
        {
            this.Lookup = items.ToLookup(keyExpression);
        }

        public AggregatedIndex(IEnumerable<TElement> items, Func<TElement, TKey> keyExpression, IEqualityComparer<TKey> comparer)
        {
            this.Lookup = items.ToLookup(keyExpression, comparer);
        }

        public int Count => this.Lookup.Count;

        protected ILookup<TKey, TElement> Lookup { get; }

        public IEnumerable<TElement> this[TKey key] => this.Lookup[key];

        public bool ContainsKey(TKey key)
        {
            return this.Lookup.Contains(key);
        }
    }
}
