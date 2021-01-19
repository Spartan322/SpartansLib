using System;
using System.Linq;
using System.Reflection;

namespace SpartansLib.Extensions
{
    public static class ReflectionExtensions
    {

		public static bool HasAttribute<InfoT>(this InfoT info, Type attributeType, bool inherit = false)
			where InfoT : MemberInfo
		{
			var attribs = info.GetCustomAttributes(attributeType, inherit);
			return attribs.Length > 0 && attribs[0] != null;
		}

		public static bool HasAttribute<InfoT, T>(this InfoT info, bool inherit = false)
            where InfoT : MemberInfo
            where T : Attribute
        {
            var attribs = info.GetCustomAttributes(typeof(T), inherit);
            return attribs.Length > 0 && attribs[0] != null;
        }

        public static bool HasAttribute<T>(this MemberInfo info, bool inherit = false)
            where T : Attribute
            => info.HasAttribute<MemberInfo, T>(inherit);

        public static bool HasAttribute<T>(this MethodInfo info, bool inherit = false)
            where T : Attribute
            => info.HasAttribute<MethodInfo, T>(inherit);

        public static bool HasAttribute<T>(this FieldInfo info, bool inherit = false)
            where T : Attribute
            => info.HasAttribute<FieldInfo, T>(inherit);

        public static bool HasAttribute<T>(this Type info, bool inherit = false)
            where T : Attribute
            => info.HasAttribute<Type, T>(inherit);

		public static T GetFirstAttributeFor<InfoT, T>(this InfoT info, Type attributeType, bool inherit = false)
			where InfoT : MemberInfo
			where T : Attribute
		{
			var attribs = info.GetCustomAttributes(attributeType, inherit);
			return attribs.Length > 0 ? attribs[0] as T : null;
		}

		public static T GetFirstAttributeFor<T>(this MemberInfo info, Type attributeType, bool inherit = false)
			where T : Attribute
			=> info.GetFirstAttributeFor<MemberInfo, T>(attributeType, inherit);

		public static T GetFirstAttribute<InfoT, T>(this InfoT info, bool inherit = false)
            where InfoT : MemberInfo
            where T : Attribute
        {
            var attribs = info.GetCustomAttributes(typeof(T), inherit);
            return attribs.Length > 0 ? attribs[0] as T : null;
        }

        public static T GetFirstAttribute<T>(this MemberInfo info, bool inherit = false)
            where T : Attribute
            => info.GetFirstAttribute<MemberInfo, T>(inherit);

		public static T[] GetAllAttributesOf<T>(this MemberInfo info, bool inherit = false)
			where T : Attribute
			=> info.GetCustomAttributes(typeof(T), inherit).Cast<T>().ToArray();

		//public static T GetFirstAttribute<T>(this MethodInfo info, bool inherit = false)
		//    where T : Attribute
		//    => info.GetFirstAttribute<MethodInfo, T>(inherit);

		//public static T GetFirstAttribute<T>(this FieldInfo info, bool inherit = false)
		//    where T : Attribute
		//    => info.GetFirstAttribute<FieldInfo, T>(inherit);

		//public static T GetFirstAttribute<T>(this Type info, bool inherit = false)
		//where T : Attribute
		//=> info.GetFirstAttribute<Type, T>(inherit);

		public static DelegateT CreateDelegate<DelegateT>(this MethodInfo info)
            where DelegateT : Delegate
            => (DelegateT)info.CreateDelegate(typeof(DelegateT));

        public static DelegateT CreateDelegate<DelegateT>(this MethodInfo info, object target)
            where DelegateT : Delegate
            => (DelegateT)info.CreateDelegate(typeof(DelegateT), target);

        public static DelegateT CreateInstancedDelegate<DelegateT, T>(this MethodInfo info, T target = default)
            where DelegateT : Delegate
            => (DelegateT)Delegate.CreateDelegate(typeof(DelegateT), target, info);
    }
}
