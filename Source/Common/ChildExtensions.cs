using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SpartansLib.Extensions.Linq;

namespace SpartansLib.Common
{
    public static class ChildExtensions
    {
        public static ChildT GetChildOrCreate<ParentT, ChildT>(
            this ParentT parent,
            NodePath path,
            Func<ParentT, ChildT> func
        )
            where ParentT : Node
            where ChildT : Node
        {
            if (path == null)
                return parent.GetChildren().SelectFirstOrCreate(parent, func);
            if (!path.IsAbsolute() || path.ToString().Contains(".."))
                throw new InvalidOperationException($"{nameof(GetChild)} can not address parent nodes.");

            var result = parent.GetNode<ChildT>(path);
            if (result == null)
                return func(parent);
            return result;
        }

        public static ChildT GetChildOrDefault<ParentT, ChildT>(this ParentT parent, NodePath path = null)
            where ParentT : Node
            where ChildT : Node
            => parent.GetChildOrCreate<ParentT, ChildT>(path, _ => default);

        public static ChildT GetChild<ChildT>(this Node node, NodePath path = null)
            where ChildT : Node
        {
            if (path == null)
                return node.GetChildren().SelectFirst<ChildT>();
            if (!path.IsAbsolute() || path.ToString().Contains(".."))
                throw new InvalidOperationException($"{nameof(GetChild)} can not address parent nodes.");

            var result = node.GetNode<ChildT>(path);
            if (result == null)
                throw new NullReferenceException($"Node '{path}' could not be found as a child of {node.GetPath()}.");
            return result;
        }

        public static IList<ChildT> GetChildren<ChildT>(this Node node)
            where ChildT : Node
            => node.GetChildren().OfType<ChildT>().ToList();

        public static ChildT AddChildAndReturn<ChildT>(
            this Node node,
            ChildT child,
            bool legibleUniqueName = false
        ) where ChildT : Node
        {
            node.AddChild(child, legibleUniqueName);
            return child;
        }

        public static ChildT AddChildAndSkip<ChildT>(
            this ChildT node,
            Node child,
            bool legibleUniqueName = false
        ) where ChildT : Node
        {
            node.AddChild(child, legibleUniqueName);
            return node;
        }

        public static ParentT MoveChildEx<ParentT>(this ParentT parent, Node child, int pos)
            where ParentT : Node
        {
            if (pos < 0)
                pos = parent.GetChildCount() + pos + 1;
            parent.MoveChild(child, pos);
            return parent;
        }
    }
}
