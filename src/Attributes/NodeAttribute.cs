using System;
using System.Collections;
using System.Reflection;
using System.Text;
using Godot;
using SpartansLib.Internal;
using SpartansLib.Common;
using System.Linq;
using System.Collections.Generic;
using ConnectFlags = Godot.Object.ConnectFlags;

namespace SpartansLib.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class NodeAttribute : SpartansLibAttribute
    {
        public string Path;
        public bool Required;
        public string PathName;

        public NodeAttribute(string path = null, bool required = true)
        {
            Path = path;
            Required = required;
        }

        private static string FilterName(string input)
        {
            var builder = new StringBuilder(input);
            if (builder[0] == '_') builder.Remove(0, 1);
            if (char.IsLower(builder[0])) builder[0] = char.ToUpper(builder[0]);
            return builder.ToString();
        }

        public override void OnReady<T>(T node, MemberInfo memberInfo)
        {
            var pathNameMemberName = $"{memberInfo.Name}Path";
            var tType = node.GetType();
            if (PathName == null && tType.GetMember(pathNameMemberName).Length > 0)
                PathName = pathNameMemberName;
            if (PathName != null)
            {
                var members = tType.GetMember(PathName);
                if (members.Length != 0)
                {
                    string path = null;
                    path = new VariableMemberInfo(members[0]).GetValue(node)?.ToString();
                    if (path != null)
                        Path = path;
                }
            }
            if (Path == null)
                Path = FilterName(memberInfo.Name);

            var val = node.GetNode(Path);
            if (val == null)
            {
                if (!Required) return;
                var err = $"Node '{Path}' is required but could not be found.";
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
