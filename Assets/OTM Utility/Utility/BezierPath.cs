using System.Collections.Generic;
using UnityEngine;

namespace com.otm.utility
{
	public static class BezierPath 
	{
		public static List<Vector3> SmoothPath(List<Transform> rawPath, float step = 0.05f)
		{
			List<Vector3> smooth = new List<Vector3>();
			if (rawPath == null)
				return smooth;
			int count = rawPath.Count;
			if (count < 2)
			{
				for (int i = 0; i < count; i++)
				{
					smooth.Add(rawPath[i].position);
				}
				return smooth;
			}

			smooth.Add(rawPath[0].position);

			for (int i = 0; i < count - 2; i++)
			{
				Vector3 p0 = rawPath[i].position;
				Vector3 p1 = rawPath[i + 1].position;
				Vector3 p2 = rawPath[i + 2].position;

				Vector3 c1 = (p0 + p1) * 0.5f;
				Vector3 c2 = (p1 + p2) * 0.5f;

				for (float t = 0; t <= 1f; t += step)
				{
					Vector3 pos = GetQuadraticBezierPoint(c1, p1, c2, t);
					smooth.Add(pos);
				}
			}

			smooth.Add(rawPath[count - 1].position);
			return smooth;
		}


		public static Vector3 GetQuadraticBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
		{
			float u = 1f - t;
			float tt = t * t;
			float uu = u * u;
			return (uu * p0) + (2f * u * t * p1) + (tt * p2);
		}
	}
}
