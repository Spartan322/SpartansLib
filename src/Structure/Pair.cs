using System;
namespace SpartansLib.Structure
{
    public class Pair<T1, T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        public Pair(T1 item1 = default, T2 item2 = default)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public static implicit operator Tuple<T1, T2>(Pair<T1, T2> pair)
            => new Tuple<T1, T2>(pair.Item1, pair.Item2);

        public static implicit operator ValueTuple<T1, T2>(Pair<T1, T2> pair)
            => new ValueTuple<T1, T2>(pair.Item1, pair.Item2);
    }
}
