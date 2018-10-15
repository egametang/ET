using UnityEngine;
using System.Collections.Generic;
using PF;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Vector3 = UnityEngine.Vector3;

#if NETFX_CORE && !UNITY_EDITOR
//using MarkerMetro.Unity.WinLegacy.IO;
#endif

namespace Pathfinding.Voxels {
	/** Stores a voxel field. \astarpro */
	public class VoxelArea {
		public const uint MaxHeight = 65536;
		public const int MaxHeightInt = 65536;

		/** Constant for default LinkedVoxelSpan top and bottom values.
		 * It is important with the U since ~0 != ~0U
		 * This can be used to check if a LinkedVoxelSpan is valid and not just the default span
		 */
		public const uint InvalidSpanValue = ~0U;

		/** Initial estimate on the average number of spans (layers) in the voxel representation. Should be greater or equal to 1 */
		public const float AvgSpanLayerCountEstimate = 8;

		/** The width of the field along the x-axis. [Limit: >= 0] [Units: vx] */
		public readonly int width = 0;

		/** The depth of the field along the z-axis. [Limit: >= 0] [Units: vx] */
		public readonly int depth = 0;

#if ASTAR_RECAST_CLASS_BASED_LINKED_LIST
		public VoxelCell[] cells;
#endif

		public CompactVoxelSpan[] compactSpans;
		public CompactVoxelCell[] compactCells;
		public int compactSpanCount;

		public ushort[] tmpUShortArr;

		public int[] areaTypes;

		public ushort[] dist;
		public ushort maxDistance;

		public int maxRegions = 0;

		public int[] DirectionX;
		public int[] DirectionZ;

		public Vector3[] VectorDirection;

		public void Reset () {
#if !ASTAR_RECAST_CLASS_BASED_LINKED_LIST
			ResetLinkedVoxelSpans();
#else
			for (int i = 0; i < cells.Length; i++) cells[i].firstSpan = null;
#endif

			for (int i = 0; i < compactCells.Length; i++) {
				compactCells[i].count = 0;
				compactCells[i].index = 0;
			}
		}

#if !ASTAR_RECAST_CLASS_BASED_LINKED_LIST
		private void ResetLinkedVoxelSpans () {
			int len = linkedSpans.Length;

			linkedSpanCount = width*depth;
			LinkedVoxelSpan df = new LinkedVoxelSpan(InvalidSpanValue, InvalidSpanValue, -1, -1);
			for (int i = 0; i < len; ) {
				// 16x unrolling, actually improves performance
				linkedSpans[i] = df; i++;
				linkedSpans[i] = df; i++;
				linkedSpans[i] = df; i++;
				linkedSpans[i] = df; i++;
				linkedSpans[i] = df; i++;
				linkedSpans[i] = df; i++;
				linkedSpans[i] = df; i++;
				linkedSpans[i] = df; i++;
				linkedSpans[i] = df; i++;
				linkedSpans[i] = df; i++;
				linkedSpans[i] = df; i++;
				linkedSpans[i] = df; i++;
				linkedSpans[i] = df; i++;
				linkedSpans[i] = df; i++;
				linkedSpans[i] = df; i++;
				linkedSpans[i] = df; i++;
			}
			removedStackCount = 0;
		}
#endif

		public VoxelArea (int width, int depth) {
			this.width = width;
			this.depth = depth;

			int wd = width*depth;
			compactCells = new CompactVoxelCell[wd];

#if !ASTAR_RECAST_CLASS_BASED_LINKED_LIST
			// & ~0xF ensures it is a multiple of 16. Required for unrolling
			linkedSpans = new LinkedVoxelSpan[((int)(wd*AvgSpanLayerCountEstimate) + 15)& ~0xF];
			ResetLinkedVoxelSpans();
#else
			cells = new VoxelCell[wd];
#endif

			DirectionX = new int[4] { -1, 0, 1, 0 };
			DirectionZ = new int[4] { 0, width, 0, -width };

			VectorDirection = new Vector3[4] { Vector3.left, Vector3.forward, Vector3.right, Vector3.back };
		}

		public int GetSpanCountAll () {
			int count = 0;

			int wd = width*depth;

			for (int x = 0; x < wd; x++) {
#if !ASTAR_RECAST_CLASS_BASED_LINKED_LIST
				for (int s = x; s != -1 && linkedSpans[s].bottom != InvalidSpanValue; s = linkedSpans[s].next) {
					count++;
				}
#else
				for (VoxelSpan s = cells[x].firstSpan; s != null; s = s.next) {
					count++;
				}
#endif
			}

			return count;
		}

		public int GetSpanCount () {
			int count = 0;

			int wd = width*depth;

			for (int x = 0; x < wd; x++) {
#if !ASTAR_RECAST_CLASS_BASED_LINKED_LIST
				for (int s = x; s != -1 && linkedSpans[s].bottom != InvalidSpanValue; s = linkedSpans[s].next) {
					if (linkedSpans[s].area != 0) {
						count++;
					}
				}
#else
				for (VoxelSpan s = cells[x].firstSpan; s != null; s = s.next) {
					if (s.area != 0) {
						count++;
					}
				}
#endif
			}
			return count;
		}


#if !ASTAR_RECAST_CLASS_BASED_LINKED_LIST
		private int linkedSpanCount;
		public LinkedVoxelSpan[] linkedSpans;

		private int[] removedStack = new int[128];
		private int removedStackCount = 0;

		void PushToSpanRemovedStack (int index) {
			// Make sure we don't overflow the list
			if (removedStackCount == removedStack.Length) {
				// Create a new list to hold recycled values
				int[] st2 = new int[removedStackCount*4];
				System.Buffer.BlockCopy(removedStack, 0, st2, 0, removedStackCount*sizeof(int));
				removedStack = st2;
			}

			removedStack[removedStackCount] = index;
			removedStackCount++;
		}
#endif

		public void AddLinkedSpan (int index, uint bottom, uint top, int area, int voxelWalkableClimb) {
#if ASTAR_RECAST_CLASS_BASED_LINKED_LIST
			cells[index].AddSpan(bottom, top, area, voxelWalkableClimb);
#else
			// linkedSpans[index] is the span with the lowest y-coordinate at the position x,z such that index=x+z*width
			// i.e linkedSpans is a 2D array laid out in a 1D array (for performance and simplicity)

			// Check if there is a root span, otherwise we can just add a new (valid) span and exit
			if (linkedSpans[index].bottom == InvalidSpanValue) {
				linkedSpans[index] = new LinkedVoxelSpan(bottom, top, area);
				return;
			}

			int prev = -1;

			// Original index, the first span we visited
			int oindex = index;

			while (index != -1) {
				if (linkedSpans[index].bottom > top) {
					// If the current span's bottom higher up than the span we want to insert's top, then they do not intersect
					// and we should just insert a new span here
					break;
				} else if (linkedSpans[index].top < bottom) {
					// The current span and the span we want to insert do not intersect
					// so just skip to the next span (it might intersect)
					prev = index;
					index = linkedSpans[index].next;
				} else {
					// Intersection! Merge the spans

					// Find the new bottom and top for the merged span
					bottom = System.Math.Min(linkedSpans[index].bottom, bottom);
					top = System.Math.Max(linkedSpans[index].top, top);

					// voxelWalkableClimb is flagMergeDistance, when a walkable flag is favored before an unwalkable one
					// So if a walkable span intersects an unwalkable span, the walkable span can be up to voxelWalkableClimb
					// below the unwalkable span and the merged span will still be walkable
					if (System.Math.Abs((int)top - (int)linkedSpans[index].top) <= voxelWalkableClimb) {
						// linkedSpans[index] is the lowest span, but we might use that span's area anyway if it is walkable
						area = System.Math.Max(area, linkedSpans[index].area);
					}

					// Find the next span in the linked list
					int next = linkedSpans[index].next;
					if (prev != -1) {
						// There is a previous span
						// Remove this span from the linked list
						linkedSpans[prev].next = next;

						// Add this span index to a list for recycling
						PushToSpanRemovedStack(index);

						// Move to the next span in the list
						index = next;
					} else if (next != -1) {
						// This was the root span and there is a span left in the linked list
						// Remove this span from the linked list by assigning the next span as the root span
						linkedSpans[oindex] = linkedSpans[next];

						// Recycle the old span index
						PushToSpanRemovedStack(next);

						// Move to the next span in the list
						// NOP since we just removed the current span, the next span
						// we want to visit will have the same index as we are on now (i.e oindex)
					} else {
						// This was the root span and there are no other spans in the linked list
						// Just replace the root span with the merged span and exit
						linkedSpans[oindex] = new LinkedVoxelSpan(bottom, top, area);
						return;
					}
				}
			}

			// We now have a merged span that needs to be inserted
			// and connected with the existing spans

			// The new merged span will be inserted right after 'prev' (if it exists, otherwise before index)

			// Make sure that we have enough room in our array
			if (linkedSpanCount >= linkedSpans.Length) {
				LinkedVoxelSpan[] tmp = linkedSpans;
				int count = linkedSpanCount;
				int popped = removedStackCount;
				linkedSpans = new LinkedVoxelSpan[linkedSpans.Length*2];
				ResetLinkedVoxelSpans();
				linkedSpanCount = count;
				removedStackCount = popped;
				for (int i = 0; i < linkedSpanCount; i++) {
					linkedSpans[i] = tmp[i];
				}
				Debug.Log("Layer estimate too low, doubling size of buffer.\nThis message is harmless.");
			}

			// Take a node from the recycling stack if possible
			// Otherwise create a new node (well, just a new index really)
			int nextIndex;
			if (removedStackCount > 0) {
				removedStackCount--;
				nextIndex = removedStack[removedStackCount];
			} else {
				nextIndex = linkedSpanCount;
				linkedSpanCount++;
			}

			if (prev != -1) {
				//span.next = prev.next;
				//prev.next = span;

				linkedSpans[nextIndex] = new LinkedVoxelSpan(bottom, top, area, linkedSpans[prev].next);
				linkedSpans[prev].next = nextIndex;
			} else {
				//span.next = firstSpan;
				//firstSpan = span;

				linkedSpans[nextIndex] = linkedSpans[oindex];
				linkedSpans[oindex] = new LinkedVoxelSpan(bottom, top, area, nextIndex);
			}
#endif
		}
	}

	public struct LinkedVoxelSpan {
		public uint bottom;
		public uint top;

		public int next;

		/*Area
		 * 0 is an unwalkable span (triangle face down)
		 * 1 is a walkable span (triangle face up)
		 */
		public int area;

		public LinkedVoxelSpan (uint bottom, uint top, int area) {
			this.bottom = bottom; this.top = top; this.area = area; this.next = -1;
		}

		public LinkedVoxelSpan (uint bottom, uint top, int area, int next) {
			this.bottom = bottom; this.top = top; this.area = area; this.next = next;
		}
	}

	/** Represents a mesh. \deprecated Use RasterizationMesh instead */
	[System.Obsolete("Use RasterizationMesh instead")]
	public class ExtraMesh : RasterizationMesh {
		public ExtraMesh (Vector3[] vertices, int[] triangles, Bounds bounds) : base(vertices, triangles, bounds) {}
		public ExtraMesh (Vector3[] vertices, int[] triangles, Bounds bounds, Matrix4x4 matrix) : base(vertices, triangles, bounds, matrix) {}
	}

	/** Represents a mesh which will be rasterized.
	 * The vertices will be multiplied with the matrix when rasterizing it to voxels.
	 * The vertices and triangles array may be used in multiple instances, it is not changed when voxelizing.
	 *
	 * \see SceneMesh
	 *
	 * \astarpro
	 */
	public class RasterizationMesh {
		/** Source of the mesh.
		 * May be null if the source was not a mesh filter
		 */
		public MeshFilter original;

		public int area;
		public Vector3[] vertices;
		public int[] triangles;

		/** Number of vertices in the #vertices array.
		 * The vertices array is often pooled and then it sometimes makes sense to use a larger array than is actually necessary.
		 */
		public int numVertices;

		/** Number of triangles in the #triangles array.
		 * The triangles array is often pooled and then it sometimes makes sense to use a larger array than is actually necessary.
		 */
		public int numTriangles;

		/** World bounds of the mesh. Assumed to already be multiplied with the matrix */
		public Bounds bounds;

		public Matrix4x4 matrix;

		/** If true, the vertex and triangle arrays will be pooled after they have been used.
		 * Should be used only if the vertex and triangle arrays were originally taken from a pool.
		 */
		public bool pool;

		public RasterizationMesh () {
		}

		public RasterizationMesh (Vector3[] vertices, int[] triangles, Bounds bounds) {
			matrix = Matrix4x4.identity;
			this.vertices = vertices;
			this.numVertices = vertices.Length;
			this.triangles = triangles;
			this.numTriangles = triangles.Length;
			this.bounds = bounds;
			original = null;
			area = 0;
		}

		public RasterizationMesh (Vector3[] vertices, int[] triangles, Bounds bounds, Matrix4x4 matrix) {
			this.matrix = matrix;
			this.vertices = vertices;
			this.numVertices = vertices.Length;
			this.triangles = triangles;
			this.numTriangles = triangles.Length;
			this.bounds = bounds;
			original = null;
			area = 0;
		}

		/** Recalculate the bounds based on #vertices and #matrix */
		public void RecalculateBounds () {
			Bounds b = new Bounds(matrix.MultiplyPoint3x4(vertices[0]), Vector3.zero);

			for (int i = 1; i < numVertices; i++) {
				b.Encapsulate(matrix.MultiplyPoint3x4(vertices[i]));
			}

			// Assigned here to avoid changing bounds if vertices would happen to be null
			bounds = b;
		}

		/** Pool the #vertices and #triangles arrays if the #pool field is true */
		public void Pool () {
			if (pool) {
				ArrayPool<int>.Release(ref triangles);
				ArrayPool<Vector3>.Release(ref vertices);
			}
		}
	}

	/** VoxelContourSet used for recast graphs.
	 * \astarpro
	 */
	public class VoxelContourSet {
		public List<VoxelContour> conts;        // Pointer to all contours.
		public Bounds bounds;                   // Bounding box of the heightfield.
	}

	/** VoxelContour used for recast graphs.
	 * \astarpro
	 */
	public struct VoxelContour {
		public int nverts;
		public int[] verts;         // Vertex coordinates, each vertex contains 4 components.
		public int[] rverts;        // Raw vertex coordinates, each vertex contains 4 components.

		public int reg;             // Region ID of the contour.
		public int area;            // Area ID of the contour.
	}

	/** VoxelMesh used for recast graphs.
	 * \astarpro
	 */
	public struct VoxelMesh {
		/** Vertices of the mesh */
		public Int3[] verts;

		/** Triangles of the mesh.
		 * Each element points to a vertex in the #verts array
		 */
		public int[] tris;

		/** Area index for each triangle */
		public int[] areas;
	}

	/** VoxelCell used for recast graphs.
	 * \astarpro
	 */
	public struct VoxelCell {
		public VoxelSpan firstSpan;

		//public System.Object lockObject;

		public void AddSpan (uint bottom, uint top, int area, int voxelWalkableClimb) {
			VoxelSpan span = new VoxelSpan(bottom, top, area);

			if (firstSpan == null) {
				firstSpan = span;
				return;
			}

			VoxelSpan prev = null;
			VoxelSpan cSpan = firstSpan;

			while (cSpan != null) {
				if (cSpan.bottom > span.top) {
					break;
				} else if (cSpan.top < span.bottom) {
					prev = cSpan;
					cSpan = cSpan.next;
				} else {
					if (cSpan.bottom < bottom) {
						span.bottom = cSpan.bottom;
					}
					if (cSpan.top > top) {
						span.top = cSpan.top;
					}

					//1 is flagMergeDistance, when a walkable flag is favored before an unwalkable one
					if (System.Math.Abs((int)span.top - (int)cSpan.top) <= voxelWalkableClimb) {
						span.area = System.Math.Max(span.area, cSpan.area);
					}

					VoxelSpan next = cSpan.next;
					if (prev != null) {
						prev.next = next;
					} else {
						firstSpan = next;
					}
					cSpan = next;
				}
			}

			if (prev != null) {
				span.next = prev.next;
				prev.next = span;
			} else {
				span.next = firstSpan;
				firstSpan = span;
			}
		}
	}

	/** CompactVoxelCell used for recast graphs.
	 * \astarpro
	 */
	public struct CompactVoxelCell {
		public uint index;
		public uint count;

		public CompactVoxelCell (uint i, uint c) {
			index = i;
			count = c;
		}
	}

	/** CompactVoxelSpan used for recast graphs.
	 * \astarpro
	 */
	public struct CompactVoxelSpan {
		public ushort y;
		public uint con;
		public uint h;
		public int reg;

		public CompactVoxelSpan (ushort bottom, uint height) {
			con = 24;
			y = bottom;
			h = height;
			reg = 0;
		}

		public void SetConnection (int dir, uint value) {
			int shift = dir*6;

			con  = (uint)((con & ~(0x3f << shift)) | ((value & 0x3f) << shift));
		}

		public int GetConnection (int dir) {
			return ((int)con >> dir*6) & 0x3f;
		}
	}

	/** VoxelSpan used for recast graphs.
	 * \astarpro
	 */
	public class VoxelSpan {
		public uint bottom;
		public uint top;

		public VoxelSpan next;

		/*Area
		 * 0 is an unwalkable span (triangle face down)
		 * 1 is a walkable span (triangle face up)
		 */
		public int area;

		public VoxelSpan (uint b, uint t, int area) {
			bottom = b;
			top = t;
			this.area = area;
		}
	}
}
