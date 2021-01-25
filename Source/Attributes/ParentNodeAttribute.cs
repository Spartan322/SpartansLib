using System;
using System.Reflection;
using Godot;
using SpartansLib;

namespace SpartansLib.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ParentNodeAttribute : SpartansLibAttribute
    {
        public Type ParentType;
        public bool Required;
        public int CheckingRange;
        public bool ExplicitType;

        public ParentNodeAttribute(Type parentType = null, bool required = true, int checkingRange = 5, bool explicitType = true)
        {
            ParentType = parentType;
            Required = required;
            CheckingRange = checkingRange;
            ExplicitType = explicitType;
        }

        private static Type GetMemberType(MemberInfo info)
        {
            switch (info)
            {
                case PropertyInfo propInfo:
                    return propInfo.PropertyType;
                case FieldInfo fieldInfo:
                    return fieldInfo.FieldType;
            }
            return null;
        }

        public override void OnReady<T>(T node, MemberInfo info)
        {
            Node checkingNode = node;
            for(var i = 0; i < CheckingRange; i++)
            {
                checkingNode = checkingNode.GetParent();
                if (ParentType == null && (GetMemberType(info)?.IsAssignableFrom(checkingNode.GetType()) ?? false)
                    || ExplicitType && checkingNode.GetType() == ParentType
                    || !ExplicitType && ParentType.IsInstanceOfType(checkingNode))
                    break;
                if (i == CheckingRange - 1) checkingNode = null;
            }
            if (checkingNode == null)
            {
                if (!Required) return;
                var err = $"Parental Node of type '{ParentType.FullName}' for '{node.GetPath()}' could not be found.";
                if (!Engine.EditorHint) throw new NullReferenceException(err);
                Logger.PushError(err);
                return;
            }
            switch (info)
            {
                case PropertyInfo propInfo when propInfo.CanWrite && propInfo.PropertyType.IsInstanceOfType(checkingNode):
                    propInfo.SetValue(node, checkingNode);
                    break;
                case FieldInfo fieldInfo when fieldInfo.FieldType.IsInstanceOfType(checkingNode):
                    fieldInfo.SetValue(node, checkingNode);
                    break;
            }
        }
    }
}