using UnityEngine;
namespace com.otm.utility
{
	public static class OTMRectTransfomUtility
	{

		public static Vector2 GetRelatedAnchorPosition(this RectTransform rectTransform, RectTransform target)
		{
			Vector2 pos;
			Vector2 screenP = RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(target, screenP, null, out pos);
			return pos * -1f;
		}

		public static (Vector2, Vector2) ConvertBoundToUISpace(Bounds bounds, RectTransform perent, Camera camera)
		{
			if (!perent)
				return (bounds.center, bounds.size);

			bool hasPoint = false;
			Vector2 min = Vector2.zero;
			Vector2 max = Vector2.zero;
			for (int x = 0; x <= 1; x++)
			{
				for (int y = 0; y <= 1; y++)
				{
					for (int z = 0; z <= 1; z++)
					{
						Vector3 corner = new Vector3(x == 0 ? bounds.min.x : bounds.max.x, y == 0 ? bounds.min.y : bounds.max.y, z == 0 ? bounds.min.z : bounds.max.z);

						if (!TryWorldToLocalPoint(perent, corner, camera, out Vector2 localPoint))
							continue;

						if (!hasPoint)
						{
							min = localPoint;
							max = localPoint;
							hasPoint = true;
							continue;
						}

						min = Vector2.Min(min, localPoint);
						max = Vector2.Max(max, localPoint);
					}
				}
			}

			if (!hasPoint)
				return (bounds.center, bounds.size);

			Vector2 size = max - min;
			return (min + (size * 0.5f), size);
		}

		public static bool TryWorldToLocalPoint(RectTransform parentRect, Vector3 worldPoint, Camera camera, out Vector2 localPoint)
		{
			Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, worldPoint);
			return RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, camera, out localPoint);
		}

		public static void SetAnchor(this RectTransform rectTransform, TextAnchor anchor)
		{
			switch (anchor)
			{
				case TextAnchor.UpperLeft:
					rectTransform.anchorMin = Vector2.up;
					rectTransform.anchorMax = Vector2.up;
					rectTransform.pivot = Vector2.up;
					break;
				case TextAnchor.UpperCenter:
					Vector2 value = new Vector2(0.5f, 1f);
					rectTransform.anchorMin = value;
					rectTransform.anchorMax = value;
					rectTransform.pivot = value;
					break;
				case TextAnchor.UpperRight:
					rectTransform.anchorMin = Vector2.one;
					rectTransform.anchorMax = Vector2.one;
					rectTransform.pivot = Vector2.one;
					break;
				case TextAnchor.MiddleLeft:
					rectTransform.anchorMin = Vector2.up * 0.5f;
					rectTransform.anchorMax = Vector2.up * 0.5f;
					rectTransform.pivot = Vector2.up * 0.5f;
					break;
				case TextAnchor.MiddleCenter:
					rectTransform.anchorMin = Vector2.one * 0.5f;
					rectTransform.anchorMax = Vector2.one * 0.5f;
					rectTransform.pivot = Vector2.one * 0.5f;
					break;
				case TextAnchor.MiddleRight:
					value = new Vector2(1f, 0.5f);
					rectTransform.anchorMin = value;
					rectTransform.anchorMax = value;
					rectTransform.pivot = value;
					break;
				case TextAnchor.LowerLeft:
					rectTransform.anchorMin = Vector2.zero;
					rectTransform.anchorMax = Vector2.zero;
					rectTransform.pivot = Vector2.zero;
					break;
				case TextAnchor.LowerCenter:
					value = new Vector2(0.5f, 0);
					rectTransform.anchorMin = value;
					rectTransform.anchorMax = value;
					rectTransform.pivot = value;
					break;
				case TextAnchor.LowerRight:
					rectTransform.anchorMin = Vector2.right;
					rectTransform.anchorMax = Vector2.right;
					rectTransform.pivot = Vector2.right;
					break;
			}
		}
	}

}
