using System;
using System.Collections.Generic;

namespace BaldurToolkit.DataTables.Indexes
{
    public static class UniqueIndex
    {
        public static UniqueIndex<TKey, TElement> Create<TKey, TElement>(IEnumerable<TElement> items, Func<TElement, TKey> keyExpression, IEqualityComparer<TKey> comparer)
        {
            return new UniqueIndex<TKey, TElement>(items, keyExpression, comparer);
        }

        public static UniqueIndex<TKey, TElement> Create<TKey, TElement>(IEnumerable<TElement> items, Func<TElement, TKey> keyExpression)
        {
            return new UniqueIndex<TKey, TElement>(items, keyExpression);
        }

        public static UniqueIndex<TKey1, TKey2, TElement> Create<TKey1, TKey2, TElement>(IEnumerable<TElement> items, Func<TElement, TKey1> key1Expression, Func<TElement, TKey2> key2Expression)
        {
            return new UniqueIndex<TKey1, TKey2, TElement>(items, key1Expression, key2Expression);
        }

        public static UniqueIndex<TKey1, TKey2, TElement> Create<TKey1, TKey2, TElement>(IEnumerable<TElement> items, Func<TElement, TKey1> key1Expression, Func<TElement, TKey2> key2Expression, IEqualityComparer<Tuple<TKey1, TKey2>> comparer)
        {
            return new UniqueIndex<TKey1, TKey2, TElement>(items, key1Expression, key2Expression, comparer);
        }
    }
}
