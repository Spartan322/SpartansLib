using System;
using Godot;
using SpartansLib.Structure;

namespace SpartansLib.Common
{
    public static class Extensions
    {
        public static Angle GetRotationAngle(this Node2D self)
            => self.Rotation;

        public static void SetRotationAngle(this Node2D self, Angle angle)
            => self.Rotation = angle;

        public static T Get<T>(this Godot.Object obj, string property)
            => (T)obj.Get(property);

        public static ChildT GetNodeOrCreate<ParentT, ChildT>(
            this ParentT parent,
            NodePath path,
            Func<ParentT, ChildT> func
        )
            where ParentT : Node
            where ChildT : Node
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var result = parent.GetNode<ChildT>(path);
            if (result != null) return result;
            result = func(parent);
            var parentPath = path.GetParentName();
            var node = parent.GetNodeOrNull(parentPath);
            if (node == null)
                throw new InvalidOperationException($"Node '{parentPath}' not found.");
            node.AddChild(result);
            return result;
        }

        public static ChildT GetNodeOrDefault<ParentT, ChildT>(this ParentT node, NodePath path)
            where ParentT : Node
            where ChildT : Node
            => node.GetNodeOrCreate<ParentT, ChildT>(path, _ => default);

        public static T FindOrCreateMeta<T>(this Godot.Object obj, string metaName, T def = default)
        {
            if (obj.HasMeta(metaName))
                return (T)obj.GetMeta(metaName);
            obj.SetMeta(metaName, def);
            return def;
        }
    }
}
