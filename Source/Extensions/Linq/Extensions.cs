using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpartansLib.Extensions.Linq
{
    public static class Extensions
    {
        public static IReadOnlyList<T> ToReadonlyList<T>(this IEnumerable<T> enumerable)
        {
            var result = new List<T>();
            foreach (var enumer in enumerable)
                result.Add(enumer);
            return result.AsReadOnly();
        }

        public static IReadOnlyList<T> ToReadonlyList<T>(this ICollection<T> collection)
        {
            var result = new T[collection.Count];
            collection.CopyTo(result, 0);
            return result.AsReadOnly();
        }

        public static T SelectFirst<T>(this IEnumerable enumerable)
        {
            foreach(var enumer in enumerable)
                if (enumer is T select) return select;
            throw new InvalidOperationException($"No element satifies type of {nameof(T)}");
        }

        public static T2 SelectFirst<T2, T1>(this IEnumerable<T1> enumerable, Func<T1, T2> func)
            where T2 : class
        {
            T2 result = null;
            foreach (var enumer in enumerable)
                if ((result = func(enumer)) != null) return result;
            throw new InvalidOperationException($"No element satifies {nameof(func)}");
        }

        public static T SelectFirstOrDefault<T>(this IEnumerable enumerable)
        {
            foreach (var enumer in enumerable)
                if (enumer is T select) return select;
            return default;
        }

        public static T2 SelectFirstOrDefault<T2, T1>(this IEnumerable<T1> enumerable, Func<T1, T2> func)
            where T2 : class
        {
            T2 result = null;
            foreach (var enumer in enumerable)
                if ((result = func(enumer)) != null) return result;
            return null;
        }


        public static ChildT SelectFirstOrCreate<ParentT, ChildT>(
    	    this IEnumerable enumerable,
			ParentT parent,
			Func<ParentT, ChildT> create
		)
        {
            foreach (var enumer in enumerable)
                if (enumer is ChildT select) return select;
            return create(parent);
        }

        public static T2 SelectFirstOrCreate<T2, T1>(this IEnumerable<T1> enumerable, Func<T1, T2> func, Func<T2> create)
            where T2 : class
        {
            T2 result = null;
            foreach (var enumer in enumerable)
                if ((result = func(enumer)) != null) return result;
            return create();
        }
    }
}
