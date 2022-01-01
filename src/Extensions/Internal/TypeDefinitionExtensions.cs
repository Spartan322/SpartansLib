using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace SpartansLib.Extensions.Internal
{
    public static class TypeDefinitionExtensions
    {
        public static IEnumerable<TypeReference> GetHierachy(this TypeDefinition type)
        {
            TypeReference next;
            do
            {
                yield return next = type.BaseType;
            }
            while(next != type.Module.TypeSystem.Object);
        }

        public static bool CanBeAssignedTo(this TypeDefinition derived, Type baseType)
            => derived.CanBeAssignedTo(baseType.FullName);

        public static bool CanBeAssignedTo(this TypeDefinition derived, string baseFullName)
            => derived.GetHierachy().Any(type => type.FullName == baseFullName);

        public static bool IsBaseFor(this TypeReference @base, TypeDefinition derivedFrom)
            => derivedFrom.GetHierachy().Any(type => type.FullName == @base.FullName);

        public static string MakeGenericRef(this Type orig, params TypeReference[] generics)
        {
            return $"{orig.FullName}[{string.Join(",", generics.Select(t => t.FullName))}]";
        }
    }
}