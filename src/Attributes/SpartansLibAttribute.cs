using System;
using System.Text;
using System.Reflection;
using Godot;

namespace SpartansLib.Attributes
{
    public class SpartansLibAttribute : Attribute
    {
        protected static string FilterName(string input)
        {
            var builder = new StringBuilder(input);
            if (builder[0] == '_') builder.Remove(0, 1);
            if (char.IsLower(builder[0])) builder[0] = char.ToUpper(builder[0]);
            return builder.ToString();
        }

        // ReSharper disable once UnusedMember.Global
        public virtual void OnConstructor<T>(T node, MemberInfo memberInfo)
            where T : Node
        {
        }

        // ReSharper disable once UnusedMember.Global
        public virtual void OnReady<T>(T node, MemberInfo memberInfo)
            where T : Node
        {
        }
    }
}
