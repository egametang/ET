using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class MeshColliderHitTest : ColliderHitTest
	{
		float width;
		float height;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="collider"></param>
		public MeshColliderHitTest(MeshCollider collider)
		{
			this.collider = collider;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		override public void SetArea(float x, float y, float width, float height)
		{
			this.width = width;
			this.height = height;
		}

		override public bool HitTest(Container container, ref Vector2 localPoint)
		{
			Camera camera = container.GetRenderCamera();

			RaycastHit hit;
			if (!HitTestContext.GetRaycastHitFromCache(camera, out hit))
				return false;

			if (hit.collider != collider)
				return false;

			localPoint = new Vector2(hit.textureCoord.x * this.width, (1 - hit.textureCoord.y) * this.height);
			HitTestContext.direction = Vector3.back;
			HitTestContext.worldPoint = StageCamera.main.ScreenToWorldPoint(new Vector2(localPoint.x, Screen.height - localPoint.y));

			return true;
		}

		public bool ScreenToLocal(Camera camera, Vector3 screenPoint, ref Vector2 point)
		{
			Ray ray = camera.ScreenPointToRay(screenPoint);
			RaycastHit hit;
			if (collider.Raycast(ray, out hit, 100))
			{
				point = new Vector2(hit.textureCoord.x * this.width, (1 - hit.textureCoord.y) * this.height);
				return true;
			}
			else
				return false;
		}
	}
}
