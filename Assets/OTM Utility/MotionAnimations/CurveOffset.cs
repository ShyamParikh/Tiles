using Sirenix.OdinInspector;
using UnityEngine;

namespace com.otm.animation
{

	[HideReferenceObjectPicker]
	public class CurveOffSet3d
	{
		#region private variables
		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curveX = new AnimationCurve(new Keyframe() { time = 0, value = 0 }, new Keyframe() { time = 1, value = 0 });
		[SerializeField] private Vector2 randomXvalue;
		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curveY = new AnimationCurve(new Keyframe() { time = 0, value = 0 }, new Keyframe() { time = 1, value = 0 });
		[SerializeField] private Vector2 randomYvalue;
		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curveZ = new AnimationCurve(new Keyframe() { time = 0, value = 0 }, new Keyframe() { time = 1, value = 0 });
		[SerializeField] private Vector2 randomZvalue;

		private Vector3 curveRandomValue;
		private Vector3 value;

		#endregion

		#region public methods

		public void SetRandomCruveValue()
		{
			curveRandomValue.x = Random.Range(randomXvalue.x, randomXvalue.y);
			curveRandomValue.y = Random.Range(randomYvalue.x, randomYvalue.y);
			curveRandomValue.z = Random.Range(randomZvalue.x, randomZvalue.y);
		}

		public Vector3 Evaluate(float evaluate)
		{
			value.x = curveRandomValue.x * curveX.Evaluate(evaluate);
			value.y = curveRandomValue.y * curveY.Evaluate(evaluate);
			value.z = curveRandomValue.z * curveZ.Evaluate(evaluate);
			return value;
		}

		#endregion
	}

	[HideReferenceObjectPicker]
	public class CurveOffSet2d
	{
		#region private variables
		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curveX = new AnimationCurve(new Keyframe() { time = 0, value = 0 }, new Keyframe() { time = 1, value = 0 });
		[SerializeField] private Vector2 randomXvalue;
		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curveY = new AnimationCurve(new Keyframe() { time = 0, value = 0 }, new Keyframe() { time = 1, value = 0 });
		[SerializeField] private Vector2 randomYvalue;

		private Vector2 curveRandomValue;
		private Vector2 value;

		#endregion

		#region public methods

		public void SetRandomCruveValue()
		{
			curveRandomValue.x = Random.Range(randomXvalue.x, randomXvalue.y);
			curveRandomValue.y = Random.Range(randomYvalue.x, randomYvalue.y);
		}

		public Vector2 Evaluate(float evaluate)
		{
			value.x = curveRandomValue.x * curveX.Evaluate(evaluate);
			value.y = curveRandomValue.y * curveY.Evaluate(evaluate);
			return value;
		}

		#endregion
	}

}
