using System;
using System.Reflection;

namespace SpartansLib.Internal
{
	public class VariableMemberInfo
	{
		public Type VariableType { get; }
		public bool CanWrite { get; }
		public bool CanRead { get; }
		private Action<object, object> SetValueAction { get; }
		private Func<object, object> GetValueAction { get; }

		public VariableMemberInfo(MemberInfo info)
		{
			switch(info)
			{
				case FieldInfo field:
					VariableType = field.FieldType;
					CanWrite = true;
					CanRead = true;
					SetValueAction = field.SetValue;
					GetValueAction = field.GetValue;
					break;
				case PropertyInfo prop:
					VariableType = prop.PropertyType;
					CanWrite = prop.CanWrite;
					CanRead = prop.CanRead;
					SetValueAction = prop.SetValue;
					GetValueAction = prop.GetValue;
					break;
			}
		}

		public void SetValue(object target, object value)
		{
			if(CanWrite)
				SetValueAction(target, value);
		}

		public object GetValue(object target)
		{
			return CanRead ? GetValueAction(target) : null;
		}
	}
}
