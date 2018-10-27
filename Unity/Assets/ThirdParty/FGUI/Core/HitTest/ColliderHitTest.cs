using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class ColliderHitTest : IHitTest
	{
		/// <summary>
		/// 
		/// </summary>
		public Collider collider;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		virtual public void SetArea(float x, float y, float width, float height)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void SetEnabled(bool value)
		{
			collider.enabled = value;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="container"></param>
		/// <param name="localPoint"></param>
		/// <returns></returns>
		virtual public bool HitTest(Container container, ref Vector2 localPoint)
		{
			Camera camera = container.GetRenderCamera();
			RaycastHit hit;
			if (!HitTestContext.GetRaycastHitFromCache(camera, out hit))
				return false;

			if (hit.collider != collider)
				return false;

			localPoint = container.GetHitTestLocalPoint();
			return true;
		}
	}
}
