using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class RectHitTest : IHitTest
	{
		/// <summary>
		/// 
		/// </summary>
		public Rect rect { get; set; }

		public void SetEnabled(bool value)
		{
		}

		public bool HitTest(Container container, ref Vector2 localPoint)
		{
			localPoint = container.GetHitTestLocalPoint();
			return rect.Contains(localPoint);
		}
	}
}
