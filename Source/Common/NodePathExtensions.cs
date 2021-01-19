using System;
using Godot;

namespace SpartansLib.Common
{
	public static class NodePathExtensions
	{
		public static NodePath GetParentName(this NodePath path, int backtrack = 1)
		{
			var nameCount = path.GetNameCount();
			var newPath = path.GetName(0);
			for (int i = 1; i < nameCount - backtrack; i++)
				newPath += '/' + path.GetName(i);
			return newPath;
		}
	}
}
