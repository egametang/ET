using PF;
using UnityEngine;

namespace Pathfinding.Voxels {
	using Pathfinding.Util;

	public partial class Voxelize {
		/** Returns T iff (v_i, v_j) is a proper internal
		 * diagonal of P.
		 */
		public static bool Diagonal (int i, int j, int n, int[] verts, int[] indices) {
			return InCone(i, j, n, verts, indices) && Diagonalie(i, j, n, verts, indices);
		}

		public static bool InCone (int i, int j, int n, int[] verts, int[] indices) {
			int pi = (indices[i] & 0x0fffffff) * 4;
			int pj = (indices[j] & 0x0fffffff) * 4;
			int pi1 = (indices[Next(i, n)] & 0x0fffffff) * 4;
			int pin1 = (indices[Prev(i, n)] & 0x0fffffff) * 4;

			// If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
			if (LeftOn(pin1, pi, pi1, verts))
				return Left(pi, pj, pin1, verts) && Left(pj, pi, pi1, verts);
			// Assume (i-1,i,i+1) not collinear.
			// else P[i] is reflex.
			return !(LeftOn(pi, pj, pi1, verts) && LeftOn(pj, pi, pin1, verts));
		}

		/** Returns true iff c is strictly to the left of the directed
		 * line through a to b.
		 */
		public static bool Left (int a, int b, int c, int[] verts) {
			return Area2(a, b, c, verts) < 0;
		}

		public static bool LeftOn (int a, int b, int c, int[] verts) {
			return Area2(a, b, c, verts) <= 0;
		}

		public static bool Collinear (int a, int b, int c, int[] verts) {
			return Area2(a, b, c, verts) == 0;
		}

		public static int Area2 (int a, int b, int c, int[] verts) {
			return (verts[b] - verts[a]) * (verts[c+2] - verts[a+2]) - (verts[c+0] - verts[a+0]) * (verts[b+2] - verts[a+2]);
		}

		/**
		 * Returns T iff (v_i, v_j) is a proper internal *or* external
		 * diagonal of P, *ignoring edges incident to v_i and v_j*.
		 */
		static bool Diagonalie (int i, int j, int n, int[] verts, int[] indices) {
			int d0 = (indices[i] & 0x0fffffff) * 4;
			int d1 = (indices[j] & 0x0fffffff) * 4;

			/*int a = (i+1) % indices.Length;
			 * if (a == j) a = (i-1 + indices.Length) % indices.Length;
			 * int a_v = (indices[a] & 0x0fffffff) * 4;
			 *
			 * if (a != j && Collinear (d0,a_v,d1,verts)) {
			 *  return false;
			 * }*/

			// For each edge (k,k+1) of P
			for (int k = 0; k < n; k++) {
				int k1 = Next(k, n);
				// Skip edges incident to i or j
				if (!((k == i) || (k1 == i) || (k == j) || (k1 == j))) {
					int p0 = (indices[k] & 0x0fffffff) * 4;
					int p1 = (indices[k1] & 0x0fffffff) * 4;

					if (Vequal(d0, p0, verts) || Vequal(d1, p0, verts) || Vequal(d0, p1, verts) || Vequal(d1, p1, verts))
						continue;

					if (Intersect(d0, d1, p0, p1, verts))
						return false;
				}
			}


			return true;
		}

		//	Exclusive or: true iff exactly one argument is true.
		//	The arguments are negated to ensure that they are 0/1
		//	values.  Then the bitwise Xor operator may apply.
		//	(This idea is due to Michael Baldwin.)
		public static bool Xorb (bool x, bool y) {
			return !x ^ !y;
		}

		//	Returns true iff ab properly intersects cd: they share
		//	a point interior to both segments.  The properness of the
		//	intersection is ensured by using strict leftness.
		public static bool IntersectProp (int a, int b, int c, int d, int[] verts) {
			// Eliminate improper cases.
			if (Collinear(a, b, c, verts) || Collinear(a, b, d, verts) ||
				Collinear(c, d, a, verts) || Collinear(c, d, b, verts))
				return false;

			return Xorb(Left(a, b, c, verts), Left(a, b, d, verts)) && Xorb(Left(c, d, a, verts), Left(c, d, b, verts));
		}

		// Returns T iff (a,b,c) are collinear and point c lies
		// on the closed segement ab.
		static bool Between (int a, int b, int c, int[] verts) {
			if (!Collinear(a, b, c, verts))
				return false;
			// If ab not vertical, check betweenness on x; else on y.
			if (verts[a+0] != verts[b+0])
				return ((verts[a+0] <= verts[c+0]) && (verts[c+0] <= verts[b+0])) || ((verts[a+0] >= verts[c+0]) && (verts[c+0] >= verts[b+0]));
			else
				return ((verts[a+2] <= verts[c+2]) && (verts[c+2] <= verts[b+2])) || ((verts[a+2] >= verts[c+2]) && (verts[c+2] >= verts[b+2]));
		}

		// Returns true iff segments ab and cd intersect, properly or improperly.
		static bool Intersect (int a, int b, int c, int d, int[] verts) {
			if (IntersectProp(a, b, c, d, verts))
				return true;
			else if (Between(a, b, c, verts) || Between(a, b, d, verts) ||
					 Between(c, d, a, verts) || Between(c, d, b, verts))
				return true;
			else
				return false;
		}

		static bool Vequal (int a, int b, int[] verts) {
			return verts[a+0] == verts[b+0] && verts[a+2] == verts[b+2];
		}

		/** (i-1+n) % n assuming 0 <= i < n */
		public static int Prev (int i, int n) { return i-1 >= 0 ? i-1 : n-1; }
		/** (i+1) % n assuming 0 <= i < n */
		public static int Next (int i, int n) { return i+1 < n ? i+1 : 0; }

		/** Builds a polygon mesh from a contour set.
		 *
		 * \param cset contour set to build a mesh from.
		 * \param nvp Maximum allowed vertices per polygon. \warning Currently locked to 3.
		 * \param mesh Results will be written to this mesh.
		 */
		public void BuildPolyMesh (VoxelContourSet cset, int nvp, out VoxelMesh mesh) {
			AstarProfiler.StartProfile("Build Poly Mesh");

			nvp = 3;

			int maxVertices = 0;
			int maxTris = 0;
			int maxVertsPerCont = 0;

			for (int i = 0; i < cset.conts.Count; i++) {
				// Skip null contours.
				if (cset.conts[i].nverts < 3) continue;

				maxVertices += cset.conts[i].nverts;
				maxTris += cset.conts[i].nverts - 2;
				maxVertsPerCont = System.Math.Max(maxVertsPerCont, cset.conts[i].nverts);
			}

			Int3[] verts = ArrayPool<Int3>.Claim(maxVertices);
			int[] polys = ArrayPool<int>.Claim(maxTris*nvp);
			int[] areas = ArrayPool<int>.Claim(maxTris);

			Memory.MemSet<int>(polys, 0xff, sizeof(int));

			int[] indices = ArrayPool<int>.Claim(maxVertsPerCont);
			int[] tris = ArrayPool<int>.Claim(maxVertsPerCont*3);

			int vertexIndex = 0;
			int polyIndex = 0;
			int areaIndex = 0;

			for (int i = 0; i < cset.conts.Count; i++) {
				VoxelContour cont = cset.conts[i];

				// Skip degenerate contours
				if (cont.nverts < 3) {
					continue;
				}

				for (int j = 0; j < cont.nverts; j++) {
					indices[j] = j;
					// Convert the z coordinate from the form z*voxelArea.width which is used in other places for performance
					cont.verts[j*4+2] /= voxelArea.width;
				}

				// Triangulate the contour
				int ntris = Triangulate(cont.nverts, cont.verts, ref indices, ref tris);

				// Assign the correct vertex indices
				int startIndex = vertexIndex;
				for (int j = 0; j < ntris*3; polyIndex++, j++) {
					//@Error sometimes
					polys[polyIndex] = tris[j]+startIndex;
				}

				// Mark all triangles generated by this contour
				// as having the area cont.area
				for (int j = 0; j < ntris; areaIndex++, j++) {
					areas[areaIndex] = cont.area;
				}

				// Copy the vertex positions
				for (int j = 0; j < cont.nverts; vertexIndex++, j++) {
					verts[vertexIndex] = new Int3(cont.verts[j*4], cont.verts[j*4+1], cont.verts[j*4+2]);
				}
			}

			mesh = new VoxelMesh {
				verts = Memory.ShrinkArray(verts, vertexIndex),
				tris = Memory.ShrinkArray(polys, polyIndex),
				areas = Memory.ShrinkArray(areas, areaIndex)
			};

			ArrayPool<Int3>.Release(ref verts);
			ArrayPool<int>.Release(ref polys);
			ArrayPool<int>.Release(ref areas);
			ArrayPool<int>.Release(ref indices);
			ArrayPool<int>.Release(ref tris);
			AstarProfiler.EndProfile("Build Poly Mesh");
		}

		int Triangulate (int n, int[] verts, ref int[] indices, ref int[] tris) {
			int ntris = 0;

			int[] dst = tris;

			int dstIndex = 0;

			// Debug code
			//int on = n;

			// The last bit of the index is used to indicate if the vertex can be removed.
			for (int i = 0; i < n; i++) {
				int i1 = Next(i, n);
				int i2 = Next(i1, n);
				if (Diagonal(i, i2, n, verts, indices)) {
					indices[i1] |= 0x40000000;
				}
			}

			while (n > 3) {
				#if ASTARDEBUG
				for (int j = 0; j < n; j++) {
					DrawLine(Prev(j, n), j, indices, verts, Color.red);
				}
				#endif

				int minLen = -1;
				int mini = -1;

				for (int q = 0; q < n; q++) {
					int q1 = Next(q, n);
					if ((indices[q1] & 0x40000000) != 0) {
						int p0 = (indices[q] & 0x0fffffff) * 4;
						int p2 = (indices[Next(q1, n)] & 0x0fffffff) * 4;

						int dx = verts[p2+0] - verts[p0+0];
						int dz = verts[p2+2] - verts[p0+2];

						#if ASTARDEBUG
						DrawLine(q, Next(q1, n), indices, verts, Color.blue);
						#endif

						//Squared distance
						int len = dx*dx + dz*dz;

						if (minLen < 0 || len < minLen) {
							minLen = len;
							mini = q;
						}
					}
				}

				if (mini == -1) {
					Debug.LogWarning("Degenerate triangles might have been generated.\n" +
						"Usually this is not a problem, but if you have a static level, try to modify the graph settings slightly to avoid this edge case.");

					// Can't run the debug stuff because we are likely running from a separate thread
					//for (int j=0;j<on;j++) {
					//	DrawLine (Prev(j,on),j,indices,verts,Color.red);
					//}

					// Should not happen.
					/*			printf("mini == -1 ntris=%d n=%d\n", ntris, n);
					 *          for (int i = 0; i < n; i++)
					 *          {
					 *              printf("%d ", indices[i] & 0x0fffffff);
					 *          }
					 *          printf("\n");*/
					//yield break;
					return -ntris;
				}

				int i = mini;
				int i1 = Next(i, n);
				int i2 = Next(i1, n);

				#if ASTARDEBUG
				for (int j = 0; j < n; j++) {
					DrawLine(Prev(j, n), j, indices, verts, Color.red);
				}

				DrawLine(i, i2, indices, verts, Color.magenta);
				for (int j = 0; j < n; j++) {
					DrawLine(Prev(j, n), j, indices, verts, Color.red);
				}
				#endif

				dst[dstIndex] = indices[i] & 0x0fffffff;
				dstIndex++;
				dst[dstIndex] = indices[i1] & 0x0fffffff;
				dstIndex++;
				dst[dstIndex] = indices[i2] & 0x0fffffff;
				dstIndex++;
				ntris++;

				// Removes P[i1] by copying P[i+1]...P[n-1] left one index.
				n--;
				for (int k = i1; k < n; k++) {
					indices[k] = indices[k+1];
				}

				if (i1 >= n) i1 = 0;
				i = Prev(i1, n);
				// Update diagonal flags.
				if (Diagonal(Prev(i, n), i1, n, verts, indices)) {
					indices[i] |= 0x40000000;
				} else {
					indices[i] &= 0x0fffffff;
				}
				if (Diagonal(i, Next(i1, n), n, verts, indices)) {
					indices[i1] |= 0x40000000;
				} else {
					indices[i1] &= 0x0fffffff;
				}
			}

			dst[dstIndex] = indices[0] & 0x0fffffff;
			dstIndex++;
			dst[dstIndex] = indices[1] & 0x0fffffff;
			dstIndex++;
			dst[dstIndex] = indices[2] & 0x0fffffff;
			dstIndex++;
			ntris++;

			return ntris;
		}
	}
}
