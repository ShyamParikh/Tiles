using UnityEngine;

namespace com.otm.utility
{
	public static class BoundsExtension
	{
		public static bool Intersects(this Bounds bounds1, Bounds bounds2)
		{
			return bounds1.Intersects(bounds2);
		}

		public static Vector3 GetClampPosition(this Bounds targetBounds, Vector3 position, Bounds bounds2)
		{
			Vector2 size = bounds2.size;
			if (size.x > targetBounds.size.x)
				size.x -= targetBounds.size.x;
			else
				size.x = 0;
			if (size.y > targetBounds.size.y)
				size.y -= targetBounds.size.y;
			else
				size.y = 0;
			Bounds intersection = new Bounds(bounds2.center, size);
			return intersection.ClosestPoint(position);
		}

		public static Bounds GetBoundsIntersection(Bounds bounds1, Bounds bounds2)
		{
			Vector3 min = Vector3.Max(bounds1.min, bounds2.min);
			Vector3 max = Vector3.Min(bounds1.max, bounds2.max);
			Vector3 size = max - min;
			return new Bounds((min + max) * 0.5f, size);
		}
	}
}
