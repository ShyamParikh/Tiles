using System;
using System.Diagnostics;

namespace com.otm.utility
{
	[Conditional("UNITY_EDITOR")]
	public class StringConstantDropDownAttribute : Attribute
	{
		public Type TargetType { get; private set; }
		public StringConstantDropDownAttribute(Type type)
		{
			TargetType = type;	
		}
	}
}
