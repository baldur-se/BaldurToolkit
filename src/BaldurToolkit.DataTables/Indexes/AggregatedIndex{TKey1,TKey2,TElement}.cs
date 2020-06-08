using System;
using System.Collections.Generic;

namespace BaldurToolkit.DataTables.Indexes
{
    public class AggregatedIndex<TKey1, TKey2, TElement> : AggregatedIndex<Tuple<TKey1, TKey2>, TElement>
    {
        public AggregatedIndex(IEnumerable<TElement> items, Func<TElement, TKey1> key1Expression, Func<TElement, TKey2> key2Expression)
            : base(items, value => Tuple.Create(key1Expression(value), key2Expression(value)))
        {
        }

        public AggregatedIndex(IEnumerable<TElement> items, Func<TElement, TKey1> key1Expression, Func<TElement, TKey2> key2Expression, IEqualityComparer<Tuple<TKey1, TKey2>> comparer)
            : base(items, value => Tuple.Create(key1Expression(value), key2Expression(value)), comparer)
        {
        }

        public IEnumerable<TElement> this[TKey1 key1, TKey2 key2] => this[Tuple.Create(key1, key2)];

        public bool ContainsKey(TKey1 key1, TKey2 key2)
        {
            return this.ContainsKey(Tuple.Create(key1, key2));
        }
    }
}
