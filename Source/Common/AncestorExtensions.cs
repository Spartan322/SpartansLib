using System;
using System.Collections.Generic;
using Godot;

namespace SpartansLib.Common
{
	public static class AncestorExtensions
	{
		public static IList<T> GetAllAncestors<T>(this Node node)
			where T : Node
		{
			var result = new List<T>();
			var parent = node.GetParent() as T;
			while (parent.GetParent() != null)
				if((parent = parent.GetParent() as T) != null)
					result.Add(parent);
			return result;
		}

		public static IList<Node> GetAllAncestors(this Node node)
			=> node.GetAllAncestors<Node>();

		public static T GetFirstAncestorOf<T>(this Node node)
			where T : Node
		{
			var newParent = node.GetParent();
			T currentParent = null;
			while ((newParent = newParent.GetParent()) != null)
				if ((currentParent = newParent as T) != null) return currentParent;
			return null;
		}
	}
}
