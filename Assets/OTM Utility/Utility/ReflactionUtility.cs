using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace com.otm.utility
{
	public static class ReflactionUtility
	{
		public static Type[] FindAllSubType<T>() where T : class
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			Assembly assembly = null;
			for (int i = 0; i < assemblies.Length; i++)
			{
				if (!assemblies[i].FullName.Contains("Editor") && assemblies[i].FullName.Contains("Assembly-CSharp"))
				{
					assembly = assemblies[i];
					break;
				}
			}
			if (assembly == null)
				return new Type[0];
			return assembly.GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))).ToArray();
		}

		public static object GetInstance(Type type)
		{
			if (type == typeof(string))
			{
				return "";
			}
			if (type.IsSubclassOf(typeof(UnityEngine.Object)))
			{
				return default;
			}
			else
			{
				return Activator.CreateInstance(type);
			}
		}

		public static MethodInfo[] GetMethodInfoWithVoid(Type type)
		{
			MethodInfo[] methodInfos = type.GetMethods();
			return methodInfos.Where(m => !m.IsSpecialName && m.ReturnType == typeof(void)).ToArray();
		}
	}
}
