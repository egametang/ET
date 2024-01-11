using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// The sphere bounds.
    /// </summary>
    public struct SphereBounds
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SphereBounds"/> struct.
        /// </summary>
        public SphereBounds(Vector3 center, float radius)
        {
            this.Center = center;
            this.Radius = radius;
        }

        /// <summary>
        /// Gets or sets the center of this sphere.
        /// </summary>
        public Vector3 Center { get; set; }

        /// <summary>
        /// Gets or sets the radius of this sphere.
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// Check whether intersects with other sphere.
        /// </summary>
        public bool Intersects(SphereBounds bounds)
        {
            var sqrDistance = (this.Center - bounds.Center).sqrMagnitude;
            var minDistance = this.Radius + bounds.Radius;
            return sqrDistance <= minDistance * minDistance;
        }

        /// <summary>
        /// Check whether intersects with other AABB.
        /// </summary>
        public bool Intersects(Bounds bounds)
        {
            // Check if the sphere is inside the AABB
            if (bounds.Contains(this.Center))
            {
                return true;
            }

            // Check if the sphere and the AABB intersect.
            var boundsMin = bounds.min;
            var boundsMax = bounds.max;

            float s = 0.0f;
            float d = 0.0f;
            if (this.Center.x < boundsMin.x)
            {
                s =  this.Center.x - boundsMin.x;
                d += s * s;
            }
            else if (this.Center.x > boundsMax.x)
            {
                s =  this.Center.x - boundsMax.x;
                d += s * s;
            }

            if (this.Center.y < boundsMin.y)
            {
                s =  this.Center.y - boundsMin.y;
                d += s * s;
            }
            else if (this.Center.y > boundsMax.y)
            {
                s =  this.Center.y - boundsMax.y;
                d += s * s;
            }

            if (this.Center.z < boundsMin.z)
            {
                s =  this.Center.z - boundsMin.z;
                d += s * s;
            }
            else if (this.Center.z > boundsMax.z)
            {
                s =  this.Center.z - boundsMax.z;
                d += s * s;
            }

            return d <= this.Radius * this.Radius;
        }
    }
}