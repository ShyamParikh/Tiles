using System;
using System.Diagnostics;

namespace com.otm.utility
{
	[Conditional("UNITY_EDITOR")]
	public class IntConstanrDropDownAttribute : Attribute
	{
		public Type TargetType { get; private set; }
		public IntConstanrDropDownAttribute(Type type)
		{
			TargetType = type;
		}
	}
}
