using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;

namespace SpartansLib
{
	public static class NodeRegistry
	{
		private static readonly Dictionary<string, Node> nameRegistry = new Dictionary<string, Node>();

		public static T RegisterOrReplace<T>(this T node, string name = null)
			where T : Node
		{
			nameRegistry[name ?? typeof(T).Name] = node;
			return node;
		}

		public static T Register<T>(this T node, string name = null)
			where T : Node
		{
			name = name ?? typeof(T).Name;
			if (IsRegistered(name))
				throw new NotSupportedException($"NodeRegistry already contains a '{name}' Node.");
			return node.RegisterOrReplace(name);
		}

		public static bool IsRegistered(string name)
			=> nameRegistry.ContainsKey(name);

		public static bool IsRegisterAssignableTo<T>(string name)
			=> typeof(T).IsAssignableFrom(nameRegistry[name].GetType());

		public static Node Get(string name)
			=> nameRegistry[name];

		public static T Get<T>(string name = null)
			where T : Node
		{
			return (T)Get(name ?? typeof(T).Name);
		}

		public static Node GetOrNull(string name)
		{
			if (nameRegistry.TryGetValue(name, out var node))
				return node;
			return null;
		}

		public static T GetOrNull<T>(string name)
			where T : Node
			=> GetOrNull(name) as T;
	}
}
