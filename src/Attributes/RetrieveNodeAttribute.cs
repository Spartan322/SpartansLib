using System;
using System.Linq;
using System.Collections.Generic;
using SpartansLib.Internal;
using SpartansLib.Common;
using Godot;

namespace SpartansLib.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class RetrieveNodeAttribute : SpartansLibAttribute
    {
        public string Name;
        public bool Required;

        public RetrieveNodeAttribute(string name = null, bool required = true)
        {
            Name = name;
            Required = required;
        }

        public override void OnReady<T>(T node, System.Reflection.MemberInfo memberInfo)
        {
            if (Name == null)
                Name = FilterName(memberInfo.Name);
            var val = NodeRegistry.GetOrNull(Name);
            if (val == null)
            {
                if (!Required) return;
                var err = $"Node '{Name}' must be registered but could not be found.";
                if (!Engine.EditorHint) throw new NullReferenceException(err);
                Logger.PushError(err);
                return;
            }

            var varInfo = new VariableMemberInfo(memberInfo);
            if (varInfo.VariableType.ContainsGenericParameters
                && typeof(IEnumerable<>)
                    .MakeGenericType(varInfo.VariableType.GenericTypeArguments[0])
                    .IsAssignableFrom(varInfo.VariableType))
            {
                var assignable = val.GetChildren<Node>().Where(n => varInfo.VariableType.GenericTypeArguments[0].IsInstanceOfType(n));
                varInfo.SetValue(node, varInfo.VariableType == typeof(IList<>)
                    .MakeGenericType(varInfo.VariableType.GenericTypeArguments[0])
                    ? assignable.ToList()
                    : assignable);
            }
            else if (varInfo.VariableType.IsInstanceOfType(val))
                varInfo.SetValue(node, val);
            else
                GD.PushWarning($"{val} not of type {varInfo.VariableType.FullName}");
        }
    }
}