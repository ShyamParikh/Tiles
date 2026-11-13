using com.otm.utility;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.otm.tutorial
{
	public class InvokeThing : TutorialThings
	{
		#region public variables

		[ValueDropdown("$types")] public Type objectType;
		[ValueDropdown("$methodsNames"), OnValueChanged("OnMethodNameChange")]
		public string methodName;
		[ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false), ShowIf("IsValidParaments")]
		public object[] parameters = new object[0];
		private Object targteObject;

		#endregion

		#region overrided methods

		public override void Init(int tutorialId, TutorialMono tutorialMono)
		{
			base.Init(tutorialId, tutorialMono);
			FindObject();
		}

		public override void Active()
		{
			if (string.IsNullOrEmpty(methodName))
				return;
			if (targteObject == null)
				FindObject();
			if (targteObject)
			{
				MethodInfo methodInfo = objectType.GetMethod(methodName);
				methodInfo.Invoke(targteObject, parameters.ToArray());
			}
		}

		public override void DeActive()
		{

		}

		public override void Stop()
		{
			
		}

		#endregion

		#region private methods


		private void FindObject()
		{
			targteObject = Object.FindAnyObjectByType(objectType, FindObjectsInactive.Include);
		}


		#endregion

		#region editor methods

#if UNITY_EDITOR

		List<string> methodsNames = new List<string>();
		Type[] types = new Type[0];


		[OnInspectorGUI]
		private void FindMonoBehaviours()
		{
			if (types == null)
				types = new Type[0];
			if (types.Length <= 0)
				types = ReflactionUtility.FindAllSubType<MonoBehaviour>();
			GetMethodList();
		}

		private void GetMethodList()
		{
			if (objectType == null)
				return;
			methodsNames = new List<string>();
			MethodInfo[] methodInfos = ReflactionUtility.GetMethodInfoWithVoid(objectType);
			for (int i = 0; i < methodInfos.Length; i++)
			{
				if (!methodsNames.Contains(methodInfos[i].Name))
					methodsNames.Add(methodInfos[i].Name);
			}
		}

		private bool IsValidParaments()
		{
			return parameters.Length > 0;
		}

		private void OnMethodNameChange()
		{
			if (objectType == null || string.IsNullOrEmpty(methodName))
				return;

			MethodInfo methodInfo = objectType.GetMethod(methodName);
			ParameterInfo[] parameterInfo = methodInfo.GetParameters();
			parameters = new object[parameterInfo.Length];
			for (int i = 0; i < parameterInfo.Length; i++)
			{
				parameters[i] = ReflactionUtility.GetInstance(parameterInfo[i].ParameterType);
			}
		}

#endif

		#endregion
	}
}
