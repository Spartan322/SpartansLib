using System;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

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

        public virtual void OnConstructor(ILProcessor il, MemberReference reference)
        {
        }

        public virtual void OnReady(ILProcessor il, MemberReference reference)
        {
        }
    }
}
