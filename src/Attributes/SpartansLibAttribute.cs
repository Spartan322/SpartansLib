using System;
using System.Reflection;
using Godot;

namespace SpartansLib.Attributes
{
    public class SpartansLibAttribute : Attribute
    {
        // ReSharper disable once UnusedMember.Global
        public virtual void OnConstructor<T>(T node, MemberInfo memberInfo)
            where T : Node
        {
        }

        // ReSharper disable once UnusedMember.Global
        public virtual void OnReady<T>(T node, MemberInfo memberInfo)
            where T : Node
        {
        }
    }
}
