using System;
using System.Reflection;
using Godot;

namespace SpartansLib.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class GlobalAttribute : SpartansLibAttribute
    {
        public string Name;
        public bool? Singular = null;

        public GlobalAttribute(string name = null)
        {
            Name = name;
        }

        public GlobalAttribute(string name, bool singular) : this(name)
        {
            Singular = singular;
        }

        public override void OnConstructor<T>(T node, MemberInfo info)
        {
            if (Name == null)
                Name = node.GetType().Name;
            var counter = 2;
            var currentName = string.Format(Name, "");
            switch (Singular)
            {
                case false:
                    while (NodeRegistry.IsRegistered(currentName))
                        currentName = string.Format(Name, counter++);
                    break;
                case true:
                    if (NodeRegistry.IsRegistered(Name))
                    {
                        GD.PushError($"{Name} already register in {nameof(NodeRegistry)}");
                        return;
                    }
                    break;
                default:
                    if (NodeRegistry.IsRegistered(Name))
                    {
                        node.RegisterOrReplace(Name);
                        return;
                    }
                    break;
            }
            node.Register(currentName);
        }
    }
}
