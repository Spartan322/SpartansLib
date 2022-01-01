using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Godot.Collections;

namespace SpartansLib.Extensions
{
    internal static class ErrCheck
    {
        internal static void Null<T>(T check, string nameof)
            where T : class
        {
            if (check == null) throw new ArgumentNullException(nameof);
        }
    }

    public static class EnumerableExtensions
    {
        public static Godot.Collections.Array ToGodotArray(this IEnumerable source)
        {
            ErrCheck.Null(source, nameof(source));
            return new Godot.Collections.Array(source);
        }

        public static Array<T> ToGodotArray<T>(this IEnumerable<T> source)
        {
            ErrCheck.Null(source, nameof(source));
            return new Array<T>(source);
        }

        public static Array<T> Cast<T>(this Godot.Collections.Array source)
        {
            ErrCheck.Null(source, nameof(source));
            return new Array<T>(source);
        }

        public static Array<T> CastGodotArray<T>(this Godot.Collections.Array source)
            => Cast<T>(source);

        public static Array<T> CastGodotArray<T>(this IEnumerable source)
        {
            if (source is Godot.Collections.Array arr) return arr.Cast<T>();
            ErrCheck.Null(source, nameof(source));
            return new Array<T>(new Godot.Collections.Array(source));
        }

        public static bool All(this IEnumerable source, Predicate<object> predicate)
        {
            ErrCheck.Null(source, nameof(source));
            ErrCheck.Null(predicate, nameof(predicate));
            foreach (var i in source)
                if (!predicate(i)) return false;
            return true;
        }

        // public static bool Any(this IEnumerable source)
        // {
        //     ErrCheck.Null(source, nameof(source));
        //     var enumerator = source.GetEnumerator();
        //     if (enumerator is IDisposable disposable)
        //         using (disposable) return enumerator.MoveNext();
        //     return enumerator.MoveNext();
        // }

        // public static bool Any(this IEnumerable source, Predicate<object> predicate)
        // {
        //     ErrCheck.Null(source, nameof(source));
        //     ErrCheck.Null(predicate, nameof(predicate));
        //     foreach (var i in source)
        //         if (predicate(i)) return true;
        //     return false;
        // }

        public static IReadOnlyList<T> AsReadOnly<T>(this IList<T> source)
            => new ReadOnlyCollection<T>(source);

        public static string JoinString<T>(this IEnumerable<T> source, string sep, Func<T, string> selector)
            => string.Join(sep, source.Select(selector));

        public static string JoinString<T>(this IEnumerable<T> source, string sep = ", ")
            => source.JoinString(sep, v => v?.ToString());

        public static float GetFloat(this BitArray source, int index)
            => source.Get(index) ? 1 : 0;

        public static int GetInt(this BitArray source, int index)
            => source.Get(index) ? 1 : 0;

        public static bool Get(this BitArray source, Enum @enum)
            => source.Get((int)(object)@enum);

        public static float GetFloat(this BitArray source, Enum @enum)
            => source.Get(@enum) ? 1 : 0;

        public static int GetInt(this BitArray source, Enum @enum)
            => source.Get(@enum) ? 1 : 0;

        public static void Set(this BitArray source, Enum @enum, bool value)
            => source.Set((int)(object)@enum, value);
    }
}