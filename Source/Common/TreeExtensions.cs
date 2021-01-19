using System;
using Godot;

namespace SpartansLib.Common
{
    public static class TreeExtensions
    {
        /// <summary>
        /// Reparent a node to a different node.
        /// </summary>
        /// <param name="self">The node to reparent.</param>
        /// <param name="newParent">The node's new parent.</param>
        public static T ReparentTo<T>(this T self, Node newParent)
            where T : Node
        {
            self.GetParent()?.RemoveChild(self);
            return newParent?.AddChildAndReturn(self);
        }

        public static T MoveInParent<T>(this T child, int pos)
            where T : Node
        {
            child.GetParent().MoveChildEx(child, pos);
            return child;
        }

        public static void Purge(this Node node)
        {
            node.GetParent()?.RemoveChild(node);
            node.Free();
        }

        public static void QueuePurge(this Node node)
        {
            node.GetParent()?.RemoveChild(node);
            node.QueueFree();
        }
    }
}
