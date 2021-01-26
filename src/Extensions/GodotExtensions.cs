using Godot;
using Godot.Collections;

namespace SpartansLib.Extensions
{
    public static class GodotExtensions
    {
        public static Dictionary<string, object> AsDict(this object source)
            => (Dictionary<string, object>)source;

        public static SignalInstance<T> GetSignal<T>(this T obj, Signal signal)
            where T : Object
            => signal.For(obj);

        public static T EmitSignal<T>(this T obj, Signal signal, params object[] args)
            where T : Object
        {
            obj.EmitSignal(signal.Name, args);
            return obj;
        }

        public static Error Connect<T>(this T obj, Signal signal, Object target, string method, uint flags, params object[] binds)
            where T : Object
            => obj.Connect(signal.Name, target, method, new Array(binds), flags);

        public static Error Connect<T>(this T obj, Signal signal, Object target, string method, params object[] binds)
            where T : Object
            => obj.Connect(signal.Name, target, method, 0, binds);
    }
}
