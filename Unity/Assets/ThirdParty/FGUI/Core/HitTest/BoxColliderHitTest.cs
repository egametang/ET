using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class BoxColliderHitTest : ColliderHitTest
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="collider"></param>
		public BoxColliderHitTest(BoxCollider collider)
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
			((BoxCollider)collider).center = new Vector3(x + width / 2, -y - height / 2);
			((BoxCollider)collider).size = new Vector3(width, height, 0);
		}
	}
}
