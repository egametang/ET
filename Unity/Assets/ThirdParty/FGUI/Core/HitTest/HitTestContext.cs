using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class HitTestContext
	{
		//set before hit test
		public static Vector2 screenPoint;
		public static Vector3 worldPoint;
		public static Vector3 direction;
		public static bool forTouch;

		public static int layerMask = -1;
		public static float maxDistance = Mathf.Infinity;

		public static Camera cachedMainCamera;

		static Dictionary<Camera, RaycastHit?> raycastHits = new Dictionary<Camera, RaycastHit?>();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="hit"></param>
		/// <returns></returns>
		public static bool GetRaycastHitFromCache(Camera camera, out RaycastHit hit)
		{
			RaycastHit? hitRef;
			if (!HitTestContext.raycastHits.TryGetValue(camera, out hitRef))
			{
				Ray ray = camera.ScreenPointToRay(HitTestContext.screenPoint);
				if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
				{
					HitTestContext.raycastHits[camera] = hit;
					return true;
				}
				else
				{
					HitTestContext.raycastHits[camera] = null;
					return false;
				}
			}
			else if (hitRef == null)
			{
				hit = new RaycastHit();
				return false;
			}
			else
			{
				hit = (RaycastHit)hitRef;
				return true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="hit"></param>
		public static void CacheRaycastHit(Camera camera, ref RaycastHit hit)
		{
			HitTestContext.raycastHits[camera] = hit;
		}

		/// <summary>
		/// 
		/// </summary>
		public static void ClearRaycastHitCache()
		{
			raycastHits.Clear();
		}
	}

}
