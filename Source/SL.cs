using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using SpartansLib.Attributes;
using SpartansLib.Extensions;

namespace SpartansLib
{
    public delegate void EventHandler<in SenderT, in ArgsT>(SenderT sender, ArgsT args);

    // ReSharper disable once InconsistentNaming
    public static class SL
    {
        // public static readonly Harmony Harmony = new Harmony("com.spartan322.spartanslib");
        //
        // [HarmonyReversePatch]
        //       public static void ReadyPrefix(Node instance)
        //       {
        //           instance.Wire();
        //       }
        //
        // public static void CtorPrefix(Node instance)
        // {
        // 	instance.Connect(Signal.Ready, _DummyRef, nameof(DummyRef.OnReady), instance);
        // 	instance.Connect(Signal.ScriptChanged, _DummyRef, nameof(DummyRef.TryCallReadyAgain), instance);
        // }
        //
        // public static void CtorPrefixForGlobal(Node instance)
        // {
        // 	var type = instance.GetType();
        // type.GetFirstAttribute<GlobalAttribute>().HandleFor(instance, type);
        // }
        //
        //       private class DummyRef : Reference
        //       {
        //           public void OnReady(Node instance)
        //               => ReadyPrefix(instance);
        // 	public void TryCallReadyAgain(Node instance)
        // 		=> instance.Notification(Node.NotificationReady);
        //       }
        //
        //       private static readonly DummyRef _DummyRef = new DummyRef();
        //
        // [MethodImpl(MethodImplOptions.NoInlining)]
        // public static void Init()
        //       {
        //           var nodeType = typeof(Node);
        //           var readyPrefix = new HarmonyMethod(typeof(SL).GetMethod(nameof(ReadyPrefix)));
        //           var ctorPrefix = new HarmonyMethod(typeof(SL).GetMethod(nameof(CtorPrefix)));
        // 	var ctorForGlobal = new HarmonyMethod(typeof(SL).GetMethod(nameof(CtorPrefixForGlobal)));
        //           foreach (var type in Assembly.GetCallingAssembly().GetExportedTypes())
        //           {
        //               if (type.IsSealed || type.IsAbstract || type.IsInterface || !nodeType.IsAssignableFrom(type)) continue;
        // 		if (type.HasAttribute<GlobalAttribute>())
        // 			Harmony.Patch(type.GetConstructor(new Type[0]), ctorForGlobal);
        // 		if (!type.HasSLAttribute()) continue;
        // 		//Harmony.PatchAll();
        //               var readyMethInfo = type.GetMethod(nameof(Node._Ready), new Type[0]);
        //               if(!readyMethInfo.IsDeclaredMember())
        //               {
        //                   Harmony.Patch(type.GetConstructor(new Type[0]), ctorPrefix);
        //                   continue;
        //               }
        // 		//if (readyMethInfo == null) continue;
        // 		Harmony.Patch(readyMethInfo, readyPrefix);
        //           }
        //       }
        //
        //      public static void Wire<T>(this T node)
        //          where T : Node
        //      {
        //          var type = node.GetType();
        //
        // foreach (var attribType in Assembly.GetExecutingAssembly().GetExportedTypes())
        // {
        // 	if (NotValidAttributeType(attribType)) continue;
        // 	var validOn = attribType.GetFirstAttribute<AttributeUsageAttribute>().ValidOn;
        // 	if (validOn == 0)
        // 		validOn = AttributeTargets.All;
        // 	//if (validOn.TargetsTypes())
        // 		//type.GetFirstAttributeFor<SpartansLibAttribute>(attribType)?.HandleFor(node, type);
        // 	if (!validOn.TargetsMembers()) continue;
        // 	foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        // 	{
        //
        // 	if (member is PropertyInfo && validOn.HasFlagOf(AttributeTargets.Property)
        // 	    || member is FieldInfo && validOn.HasFlagOf(AttributeTargets.Field)
        // 	    || member is MethodInfo && validOn.HasFlagOf(AttributeTargets.Method)
        // 	    || member is EventInfo && validOn.HasFlagOf(AttributeTargets.Event)
        // 	    || member is ConstructorInfo && validOn.HasFlagOf(AttributeTargets.Constructor))
        // 		member.GetFirstAttributeFor<SpartansLibAttribute>(attribType)?.HandleFor(node, member);
        // 	}
        // }
        //      }
        //
        // ReSharper disable once InconsistentNaming
        //       public static bool HasSLAttribute(this Type nodeType)
        // {
        // 	foreach (var attribType in Assembly.GetExecutingAssembly().GetExportedTypes())
        // 	{
        // 		if (NotValidAttributeType(attribType)) continue;
        // 		var validOn = attribType.GetFirstAttribute<AttributeUsageAttribute>()?.ValidOn ?? AttributeTargets.All;
        // 		if (validOn == 0)
        // 			validOn = AttributeTargets.All;
        // 		if (validOn.TargetsTypes())
        // 		{
        // 			var has = nodeType.HasAttribute(attribType);
        // 			if (has)
        // 				return true;
        // 		}
        //
        // 		if (!validOn.TargetsMembers()) continue;
        // 		{
        // 			var has = nodeType.GetMembers(
        // 					BindingFlags.Public
        // 					| BindingFlags.NonPublic
        // 					| BindingFlags.Instance)
        // 				.Any(m =>
        // 				{
        // 					if (m is PropertyInfo && validOn.HasFlagOf(AttributeTargets.Property)
        // 					    || m is FieldInfo && validOn.HasFlagOf(AttributeTargets.Field)
        // 					    || m is MethodInfo && validOn.HasFlagOf(AttributeTargets.Method)
        // 					    || m is EventInfo && validOn.HasFlagOf(AttributeTargets.Event)
        // 					    || m is ConstructorInfo && validOn.HasFlagOf(AttributeTargets.Constructor))
        // 						return m.HasAttribute(attribType);
        // 					return false;
        // 				});
        // 			if (has)
        // 				return true;
        // 		}
        // 	}
        // 	return false;
        // }
        //
        // private static bool NotValidAttributeType(Type type)
        // 	=> type == typeof(SpartansLibAttribute) || type == typeof(GlobalAttribute) || !typeof(SpartansLibAttribute).IsAssignableFrom(type);
    }
}