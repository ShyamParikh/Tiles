using Sirenix.OdinInspector;
using UnityEngine;

namespace com.otm.utility
{
	public static class VectorExtension
	{
		public static Vector3 Clamp(this Vector3 value, Vector3 minValue, Vector3 maxValue)
		{
			value.x = Mathf.Clamp(value.x, minValue.x, maxValue.x);
			value.y = Mathf.Clamp(value.y, minValue.y, maxValue.y);
			value.z = Mathf.Clamp(value.z, minValue.z, maxValue.z);
			return value;
		}

		public static Vector2 Clamp(this Vector2 value, Vector2 minValue, Vector2 maxValue)
		{
			value.x = Mathf.Clamp(value.x, minValue.x, maxValue.x);
			value.y = Mathf.Clamp(value.y, minValue.y, maxValue.y);
			return value;
		}

		public static float Distance(this Vector3 position1, Vector3 position2)
		{
			Vector3 gap = position1 - position2;
			float distanceSquared = gap.x * gap.x + gap.y * gap.y + gap.z * gap.z;
			return Mathf.Sqrt(distanceSquared);
		}

		public static float Distance(this Vector2 position1, Vector2 position2)
		{
			Vector2 gap = position1 - position2;
			float distanceSquared = gap.x * gap.x + gap.y * gap.y;
			return Mathf.Sqrt(distanceSquared);
		}

		public static float GetAngle(Vector2 point1, Vector2 point2, bool clamp360 = false)
		{
			Vector2 distance = point1 - point2;
			float angle = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;
			angle += 90;
			if (clamp360)
			{
				if (angle < 0)
					angle += 360;
				if (angle >= 360)
					angle = 0;
			}
			return angle;
		}

		public static float GetAngleY(Vector3 point1, Vector3 point2, bool clamp360 = false)
		{
			Vector3 distance = point1 - point2;
			float angle = Mathf.Atan2(distance.x, distance.z) * Mathf.Rad2Deg;
			angle += 90;
			if (clamp360)
			{
				if (angle < 0)
					angle += 360;
				if (angle >= 360)
					angle = 0;
			}
			return angle;
		}

		public static bool Contains2d(this Bounds bounds, Vector2 pos)
		{
			return bounds.Contains(pos);
		}

		public static bool InnerRayCast2d(Vector2 startPoint, out Vector2 hitPoint, float angle, float distance, Bounds bounds)
		{
			angle += 90;
			Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
			Vector2 endPoint = startPoint + (direction * distance);
			if (SegmentIntersection(startPoint, endPoint, new Vector2(bounds.min.x, bounds.max.y), bounds.max, out hitPoint))
				return true;
			else if (SegmentIntersection(startPoint, endPoint, bounds.min, new Vector2(bounds.max.x, bounds.min.y), out hitPoint))
				return true;
			else if (SegmentIntersection(startPoint, endPoint, bounds.min, new Vector2(bounds.min.x, bounds.max.y), out hitPoint))
				return true;
			else if (SegmentIntersection(startPoint, endPoint, new Vector2(bounds.max.x, bounds.min.y), bounds.max, out hitPoint))
				return true;
			return false;
		}

		public static bool SegmentIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
		{
			intersection = Vector2.zero;

			float denominator = (p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x);

			// Lines are parallel or coincident
			if (Mathf.Approximately(denominator, 0))
				return false;

			float t = ((p1.x - p3.x) * (p3.y - p4.y) - (p1.y - p3.y) * (p3.x - p4.x)) / denominator;

			float u = ((p1.x - p3.x) * (p1.y - p2.y) - (p1.y - p3.y) * (p1.x - p2.x)) / denominator;

			// Check if intersection point is within both segments
			if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
			{
				intersection = new Vector2(p1.x + t * (p2.x - p1.x), p1.y + t * (p2.y - p1.y));
				return true;
			}

			return false; // Intersection is outside the bounds
		}
	}

	public struct CustomVector3
	{
		[HorizontalGroup]
		public float x;
		[HorizontalGroup]
		public float y;
		[HorizontalGroup]
		public float z;


		public CustomVector3(Vector3 vector3)
		{
			x = vector3.x;
			y = vector3.y;
			z = vector3.z;
		}

		public Vector3 ToVector3()
		{
			return new Vector3(x, y, z);
		}
	}
}
