using PF;

namespace Pathfinding.Voxels {
	/** Utility for clipping polygons */
	internal struct VoxelPolygonClipper {
		/** Cache this buffer to avoid unnecessary allocations */
		float[] clipPolygonCache;

		/** Cache this buffer to avoid unnecessary allocations */
		int[] clipPolygonIntCache;

		/** Initialize buffers if they are null */
		void Init () {
			if (clipPolygonCache == null) {
				clipPolygonCache = new float[7*3];
				clipPolygonIntCache = new int[7*3];
			}
		}

		/** Clips a polygon against an axis aligned half plane.
		 * \param vIn Input vertices in XYZ format, so each group of 3 indices form a single vertex
		 * \param n Number of input vertices (may be less than the length of the vIn array)
		 * \param vOut Output vertices, needs to be large enough
		 * \param multi Scale factor for the input vertices
		 * \param offset Offset to move the input vertices with before cutting
		 * \param axis Axis to cut along, either x=0, y=1, z=2
		 *
		 * \returns Number of output vertices
		 *
		 * The vertices will be scaled and then offset, after that they will be cut using either the
		 * x axis, y axis or the z axis as the cutting line. The resulting vertices will be added to the
		 * vOut array in their original space (i.e before scaling and offsetting).
		 */
		public int ClipPolygon (float[] vIn, int n, float[] vOut, float multi, float offset, int axis) {
			Init();

			float[] d = clipPolygonCache;

			for (int i = 0; i < n; i++) {
				d[i] = multi*vIn[i*3+axis]+offset;
			}

			//Number of resulting vertices
			int m = 0;

			for (int i = 0, j = n-1; i < n; j = i, i++) {
				bool prev = d[j] >= 0;
				bool curr = d[i] >= 0;

				if (prev != curr) {
					int m3 = m*3;
					int i3 = i*3;
					int j3 = j*3;

					float s = d[j] / (d[j] - d[i]);

					vOut[m3+0] = vIn[j3+0] + (vIn[i3+0]-vIn[j3+0])*s;
					vOut[m3+1] = vIn[j3+1] + (vIn[i3+1]-vIn[j3+1])*s;
					vOut[m3+2] = vIn[j3+2] + (vIn[i3+2]-vIn[j3+2])*s;

					//vOut[m*3+0] = vIn[j*3+0] + (vIn[i*3+0]-vIn[j*3+0])*s;
					//vOut[m*3+1] = vIn[j*3+1] + (vIn[i*3+1]-vIn[j*3+1])*s;
					//vOut[m*3+2] = vIn[j*3+2] + (vIn[i*3+2]-vIn[j*3+2])*s;

					m++;
				}

				if (curr) {
					int m3 = m*3;
					int i3 = i*3;

					vOut[m3+0] = vIn[i3+0];
					vOut[m3+1] = vIn[i3+1];
					vOut[m3+2] = vIn[i3+2];

					m++;
				}
			}

			return m;
		}

		public int ClipPolygonY (float[] vIn, int n, float[] vOut, float multi, float offset, int axis) {
			Init();

			float[] d = clipPolygonCache;

			for (int i = 0; i < n; i++) {
				d[i] = multi*vIn[i*3+axis]+offset;
			}

			//Number of resulting vertices
			int m = 0;

			for (int i = 0, j = n-1; i < n; j = i, i++) {
				bool prev = d[j] >= 0;
				bool curr = d[i] >= 0;

				if (prev != curr) {
					vOut[m*3+1] = vIn[j*3+1] + (vIn[i*3+1]-vIn[j*3+1]) * (d[j] / (d[j] - d[i]));

					m++;
				}

				if (curr) {
					vOut[m*3+1] = vIn[i*3+1];

					m++;
				}
			}

			return m;
		}

		/** Clips a polygon against an axis aligned half plane.
		 * \param vIn Input vertices
		 * \param n Number of input vertices (may be less than the length of the vIn array)
		 * \param vOut Output vertices, needs to be large enough
		 * \param multi Scale factor for the input vertices
		 * \param offset Offset to move the input vertices with before cutting
		 * \param axis Axis to cut along, either x=0, y=1, z=2
		 *
		 * \returns Number of output vertices
		 *
		 * The vertices will be scaled and then offset, after that they will be cut using either the
		 * x axis, y axis or the z axis as the cutting line. The resulting vertices will be added to the
		 * vOut array in their original space (i.e before scaling and offsetting).
		 */
		public int ClipPolygon (Int3[] vIn, int n, Int3[] vOut, int multi, int offset, int axis) {
			Init();

			int[] d = clipPolygonIntCache;

			for (int i = 0; i < n; i++) {
				d[i] = multi*vIn[i][axis]+offset;
			}

			// Number of resulting vertices
			int m = 0;

			for (int i = 0, j = n-1; i < n; j = i, i++) {
				bool prev = d[j] >= 0;
				bool curr = d[i] >= 0;

				if (prev != curr) {
					double s = (double)d[j] / (d[j] - d[i]);

					vOut[m] = vIn[j] + (vIn[i]-vIn[j])*s;
					m++;
				}

				if (curr) {
					vOut[m] = vIn[i];
					m++;
				}
			}

			return m;
		}
	}
}
