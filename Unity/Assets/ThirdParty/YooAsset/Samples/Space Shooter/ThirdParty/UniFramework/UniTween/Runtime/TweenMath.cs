using UnityEngine;

namespace UniFramework.Tween
{
	public static class TweenMath
	{
		/// <summary>
		/// 角度插值
		/// </summary>
		public static Vector3 AngleLerp(Vector3 from, Vector3 to, float progress)
		{
			float x = Mathf.LerpAngle(from.x, to.x, progress);
			float y = Mathf.LerpAngle(from.y, to.y, progress);
			float z = Mathf.LerpAngle(from.z, to.z, progress);
			Vector3 result = new Vector3(x, y, z);
			return result;
		}

		/// <summary>
		/// 二阶贝塞尔曲线
		/// </summary>
		public static Vector3 QuadBezier(Vector3 p1, Vector3 c, Vector3 p2, float progress)
		{
			float t = progress;
			float d = 1f - t;
			return d * d * p1 + 2f * d * t * c + t * t * p2;
		}
		
		/// <summary>
		/// 三阶贝塞尔曲线
		/// </summary>
		public static Vector3 CubicBezier(Vector3 p1, Vector3 c1, Vector3 c2, Vector3 p2, float progress)
		{
			float t = progress;
			float d = 1f - t;
			return d * d * d * p1 + 3f * d * d * t * c1 + 3f * d * t * t * c2 + t * t * t * p2;
		}

		/// <summary>
		/// 样条曲线
		/// </summary>
		public static Vector3 CatmullRoom(Vector3 c1, Vector3 p1, Vector3 p2, Vector3 c2, float progress)
		{
			float t = progress;
			return .5f *
			(
				(-c1 + 3f * p1 - 3f * p2 + c2) * (t * t * t)
				+ (2f * c1 - 5f * p1 + 4f * p2 - c2) * (t * t)
				+ (-c1 + p2) * t
				+ 2f * p1
			);
		}
		
		/// <summary>
		/// 震动采样
		/// </summary>
		public static Vector3 Shake(Vector3 magnitude, Vector3 position, float progress)
		{
			float x = Random.Range(-magnitude.x, magnitude.x) * progress;
			float y = Random.Range(-magnitude.y, magnitude.y) * progress;
			float z = Random.Range(-magnitude.z, magnitude.z) * progress;
			return position + new Vector3(x, y, z);
		}
	}
}