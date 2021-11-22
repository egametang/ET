using UnityEngine;

namespace ET
{
	public static class PositionHelper
	{
		public static Vector3 RayCastV2ToV3(Vector2 pos)
		{
            return new Vector3(pos.x, 0, pos.y);
		}

		public static Vector3 RayCastXYToV3(float x, float y)
        {
			return new Vector3(x, 0, y);
		}

		public static Vector3 RayCastV3ToV3(Vector3 pos)
		{
			return new Vector3(pos.x, 0, pos.z);
		}

		public static Quaternion GetVector3ToQuaternion(Vector3 source, Vector3 dire)
		{
			Vector3 nowPos = source;
			if (nowPos == dire)
			{
				return new Quaternion();
			}
			Vector3 direction = (dire - nowPos).normalized;
			return Quaternion.LookRotation(direction, Vector3.up);
		}

        public static float Distance2D(Unit u1, Unit u2)
        {
            Vector2 v1 = new Vector2(u1.Position.x, u1.Position.z);
            Vector2 v2 = new Vector2(u2.Position.x, u2.Position.z);
            return Vector2.Distance(v1, v2);
        }

        public static float Distance2D(Vector3 v1, Vector3 v2)
        {
            Vector2 d1 = new Vector2(v1.x, v1.z);
            Vector2 d2 = new Vector2(v2.x, v2.z);
            return Vector2.Distance(d1, d2);
        }

		public static float Vector3ToAngle360(Vector3 from, Vector3 to)
		{
			float angle = Vector3.Angle(from, to);
			Vector3 cross = Vector3.Cross(from, to);
			return cross.y > 0? angle : 360 - angle;
		}
        /// <summary>
        ///  求点到直线的距离，采用数学公式Ax+By+C = 0; d = A*p.x + B * p.y + C / sqrt(A^2 + B ^ 2)
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="point"></param>
        /// <returns></returns>
	    public static float DistanceOfPointToVector(Vector3 startPoint, Vector3 endPoint, Vector3 point)
	    {
            Vector2 startVe2 = startPoint.IgnoreYAxis();
            Vector2 endVe2 = endPoint.IgnoreYAxis();
            float A = endVe2.y - startVe2.y;
            float B = startVe2.x - endVe2.x;
            float C = endVe2.x * startVe2.y - startVe2.x * endVe2.y;
            float denominator = Mathf.Sqrt(A * A + B * B);
            Vector2 pointVe2 = point.IgnoreYAxis();
            return Mathf.Abs((A * pointVe2.x + B * pointVe2.y + C) / denominator); ;
        }
        /// <summary>
        /// 勾股定理
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static float GGTheorem(float x, float y)
        {
            return Mathf.Sqrt(x * x + y * y);
        }
        /// <summary>
        /// 去掉三维向量的Y轴，把向量投射到xz平面。
        /// </summary>
        /// <param name="vector3"></param>
        /// <returns></returns>
	    public static Vector2 IgnoreYAxis(this Vector3 vector3)
	    {
            return new Vector2(vector3.x, vector3.z);
        }
        /// <summary>
        /// 判断目标点是否位于向量的左边
        /// </summary>
        /// <returns>True is on left, false is on right</returns>
        public static bool PointOnLeftSideOfVector(this Vector3 vector3, Vector3 originPoint, Vector3 point)
        {
            Vector2 originVec2 = originPoint.IgnoreYAxis();

            Vector2 pointVec2 = (point.IgnoreYAxis() - originVec2).normalized;

            Vector2 vector2 = vector3.IgnoreYAxis();

            float verticalX = originVec2.x;

            float verticalY = (-verticalX * vector2.x) / vector2.y;

            Vector2 norVertical = (new Vector2(verticalX, verticalY)).normalized;

            float dotValue = Vector2.Dot(norVertical, pointVec2);

            return dotValue < 0f;
        }
	}
}