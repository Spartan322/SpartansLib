using System;

namespace SpartansLib.Attributes
{
    [Obsolete("Not Implemented")]
    public class SingletonAttribute : SpartansLibAttribute
    {
        public Type[] Types { get; }

        public SingletonAttribute(Type[] types)
        {
            Types = types;
        }

        // public override void HandleFor<T>(T node, MemberInfo info)
        // {
        //     base.HandleFor(node, info);
        // }
    }
}
