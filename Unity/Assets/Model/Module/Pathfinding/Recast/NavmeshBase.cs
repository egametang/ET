using PF;
using System.Collections.Generic;

namespace PF {
	using System.IO;
	using Math = System.Math;
	using System.Linq;
	
	
	/** Returned by graph ray- or linecasts containing info about the hit.
 * This is the return value by the #Pathfinding.IRaycastableGraph.Linecast methods.
 * Some members will also be initialized even if nothing was hit, see the individual member descriptions for more info.
 *
 * \shadowimage{linecast.png}
 */
	public struct GraphHitInfo {
		/** Start of the line/ray.
		 * Note that the point passed to the Linecast method will be clamped to the closest point on the navmesh.
		 */
		public Vector3 origin;
		/** Hit point.
		 * In case no obstacle was hit then this will be set to the endpoint of the line.
		 */
		public Vector3 point;
		/** Node which contained the edge which was hit.
		 * If the linecast did not hit anything then this will be set to the last node along the line's path (the one which contains the endpoint).
		 *
		 * For layered grid graphs the linecast will return true (i.e: no free line of sight) if when walking the graph we ended up at X,Z coordinate for the end node
		 * but the end node was on a different level (e.g the floor below or above in a building). In this case no node edge was really hit so this field will still be null.
		 */
		public GraphNode node;
		/** Where the tangent starts. #tangentOrigin and #tangent together actually describes the edge which was hit.
		 * \shadowimage{linecast_tangent.png}
		 */
		public Vector3 tangentOrigin;
		/** Tangent of the edge which was hit.
		 * \shadowimage{linecast_tangent.png}
		 */
		public Vector3 tangent;

		/** Distance from #origin to #point */
		public float distance {
			get {
				return (point-origin).magnitude;
			}
		}

		public GraphHitInfo (Vector3 point) {
			tangentOrigin  = Vector3.zero;
			origin = Vector3.zero;
			this.point = point;
			node = null;
			tangent = Vector3.zero;
		}
	}

	/** Graph which supports the Linecast method */
	public interface IRaycastableGraph {
		bool Linecast (Vector3 start, Vector3 end);
		bool Linecast (Vector3 start, Vector3 end, GraphNode hint);
		bool Linecast (Vector3 start, Vector3 end, GraphNode hint, out GraphHitInfo hit);
		bool Linecast (Vector3 start, Vector3 end, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace);
	}
	
	/** Graph which has a well defined transformation from graph space to world space */
	public interface ITransformedGraph {
		GraphTransform transform { get; }
	}
	
	/** Base class for RecastGraph and NavMeshGraph */
	public abstract class NavmeshBase : NavGraph, INavmesh, INavmeshHolder, ITransformedGraph
		, IRaycastableGraph {
#if ASTAR_RECAST_LARGER_TILES
		// Larger tiles
		public const int VertexIndexMask = 0xFFFFF;

		public const int TileIndexMask = 0x7FF;
		public const int TileIndexOffset = 20;
#else
		// Larger worlds
		public const int VertexIndexMask = 0xFFF;

		public const int TileIndexMask = 0x7FFFF;
		public const int TileIndexOffset = 12;
#endif

		/** Size of the bounding box. */
		[JsonMember]
		public Vector3 forcedBoundsSize = new Vector3(100, 40, 100);

		/** Size of a tile in world units along the X axis */
		public abstract float TileWorldSizeX { get; }

		/** Size of a tile in world units along the Z axis */
		public abstract float TileWorldSizeZ { get; }

		/** Maximum (vertical) distance between the sides of two nodes for them to be connected across a tile edge.
		 * When tiles are connected to each other, the nodes sometimes do not line up perfectly
		 * so some allowance must be made to allow tiles that do not match exactly to be connected with each other.
		 */
		protected abstract float MaxTileConnectionEdgeDistance { get; }

		/** Show an outline of the polygons in the Unity Editor */
		[JsonMember]
		public bool showMeshOutline = true;

		/** Show the connections between the polygons in the Unity Editor */
		[JsonMember]
		public bool showNodeConnections;

		/** Show the surface of the navmesh */
		[JsonMember]
		public bool showMeshSurface;

		/** Number of tiles along the X-axis */
		public int tileXCount;
		/** Number of tiles along the Z-axis */
		public int tileZCount;

		/** All tiles.
		 *
		 * \see #GetTile
		 */
		public NavmeshTile[] tiles;

		/** Perform nearest node searches in XZ space only.
		 * Recomended for single-layered environments. Faster but can be inaccurate esp. in multilayered contexts.
		 * You should not use this if the graph is rotated since then the XZ plane no longer corresponds to the ground plane.
		 *
		 * This can be important on sloped surfaces. See the image below in which the closest point for each blue point is queried for:
		 * \shadowimage{distanceXZ2.png}
		 *
		 * You can also control this using a \link Pathfinding.NNConstraint.distanceXZ field on an NNConstraint object\endlink.
		 */
		[JsonMember]
		public bool nearestSearchOnlyXZ;

		/** Currently updating tiles in a batch */
		bool batchTileUpdate;

		/** Determines how the graph transforms graph space to world space.
		 * \see #CalculateTransform
		 */
		public GraphTransform transform = new GraphTransform(Matrix4x4.identity);

		GraphTransform ITransformedGraph.transform { get { return transform; } }

		/** \copydoc Pathfinding::NavMeshGraph::recalculateNormals */
		protected abstract bool RecalculateNormals { get; }

		/** Called when tiles have been completely recalculated.
		 * This is called after scanning the graph and after
		 * performing graph updates that completely recalculate tiles
		 * (not ones that simply modify e.g penalties).
		 * It is not called after NavmeshCut updates.
		 */
		public System.Action<NavmeshTile[]> OnRecalculatedTiles;

		/** Tile at the specified x, z coordinate pair.
		 * The first tile is at (0,0), the last tile at (tileXCount-1, tileZCount-1).
		 *
		 * \snippet MiscSnippets.cs NavmeshBase.GetTile
		 */
		public NavmeshTile GetTile (int x, int z) {
			return tiles[x + z * tileXCount];
		}

		/** Vertex coordinate for the specified vertex index.
		 *
		 * \throws IndexOutOfRangeException if the vertex index is invalid.
		 * \throws NullReferenceException if the tile the vertex is in is not calculated.
		 *
		 * \see NavmeshTile.GetVertex
		 */
		public Int3 GetVertex (int index) {
			int tileIndex = (index >> TileIndexOffset) & TileIndexMask;

			return tiles[tileIndex].GetVertex(index);
		}

		/** Vertex coordinate in graph space for the specified vertex index */
		public Int3 GetVertexInGraphSpace (int index) {
			int tileIndex = (index >> TileIndexOffset) & TileIndexMask;

			return tiles[tileIndex].GetVertexInGraphSpace(index);
		}

		/** Tile index from a vertex index */
		public static int GetTileIndex (int index) {
			return (index >> TileIndexOffset) & TileIndexMask;
		}

		public int GetVertexArrayIndex (int index) {
			return index & VertexIndexMask;
		}

		/** Tile coordinates from a tile index */
		public void GetTileCoordinates (int tileIndex, out int x, out int z) {
			//z = System.Math.DivRem (tileIndex, tileXCount, out x);
			z = tileIndex/tileXCount;
			x = tileIndex - z*tileXCount;
		}

		/** All tiles.
		 * \warning Do not modify this array
		 */
		public NavmeshTile[] GetTiles () {
			return tiles;
		}

		/** Returns the tile coordinate which contains the specified \a position.
		 * It is not necessarily a valid tile (i.e it could be out of bounds).
		 */
		public Int2 GetTileCoordinates (Vector3 position) {
			position = transform.InverseTransform(position);
			position.x /= TileWorldSizeX;
			position.z /= TileWorldSizeZ;
			return new Int2((int)position.x, (int)position.z);
		}

		protected override void OnDestroy () {
			base.OnDestroy();
#if !SERVER // 服务端不需要释放
			// Cleanup
			TriangleMeshNode.SetNavmeshHolder(AstarPath.active.data.GetGraphIndex(this), null);
#endif
			if (tiles != null) {
				for (int i = 0; i < tiles.Length; i++) {
					ObjectPool<BBTree>.Release(ref tiles[i].bbTree);
				}
			}
		}

		/** Creates a single new empty tile */
		protected NavmeshTile NewEmptyTile (int x, int z) {
			return new NavmeshTile {
					   x = x,
					   z = z,
					   w = 1,
					   d = 1,
					   verts = new Int3[0],
					   vertsInGraphSpace = new Int3[0],
					   tris = new int[0],
					   nodes = new TriangleMeshNode[0],
					   bbTree = ObjectPool<BBTree>.Claim(),
					   graph = this,
			};
		}

		public override void GetNodes (System.Action<GraphNode> action) {
			if (tiles == null) return;

			for (int i = 0; i < tiles.Length; i++) {
				if (tiles[i] == null || tiles[i].x+tiles[i].z*tileXCount != i) continue;
				TriangleMeshNode[] nodes = tiles[i].nodes;

				if (nodes == null) continue;

				for (int j = 0; j < nodes.Length; j++) action(nodes[j]);
			}
		}

		protected void ConnectTileWithNeighbours (NavmeshTile tile, bool onlyUnflagged = false) {
			if (tile.w != 1 || tile.d != 1) {
				throw new System.ArgumentException("Tile widths or depths other than 1 are not supported. The fields exist mainly for possible future expansions.");
			}

			// Loop through z and x offsets to adjacent tiles
			// _ x _
			// x _ x
			// _ x _
			for (int zo = -1; zo <= 1; zo++) {
				var z = tile.z + zo;
				if (z < 0 || z >= tileZCount) continue;

				for (int xo = -1; xo <= 1; xo++) {
					var x = tile.x + xo;
					if (x < 0 || x >= tileXCount) continue;

					// Ignore diagonals and the tile itself
					if ((xo == 0) == (zo == 0)) continue;

					var otherTile = tiles[x + z*tileXCount];
					if (!onlyUnflagged || !otherTile.flag) {
						ConnectTiles(otherTile, tile);
					}
				}
			}
		}


		static readonly NNConstraint NNConstraintDistanceXZ = new NNConstraint { distanceXZ = true };

		public override NNInfoInternal GetNearest (Vector3 position, NNConstraint constraint, GraphNode hint) {
			return GetNearestForce(position, constraint != null && constraint.distanceXZ ? NNConstraintDistanceXZ : null);
		}

		public override NNInfoInternal GetNearestForce (Vector3 position, NNConstraint constraint) {
			if (tiles == null) return new NNInfoInternal();

			var tileCoords = GetTileCoordinates(position);

			// Clamp to graph borders
			tileCoords.x = Mathf.Clamp(tileCoords.x, 0, tileXCount-1);
			tileCoords.y = Mathf.Clamp(tileCoords.y, 0, tileZCount-1);

			int wmax = Math.Max(tileXCount, tileZCount);

			var best = new NNInfoInternal();
			float bestDistance = float.PositiveInfinity;

			bool xzSearch = nearestSearchOnlyXZ || (constraint != null && constraint.distanceXZ);

			// Search outwards in a diamond pattern from the closest tile
			//     2
			//   2 1 2
			// 2 1 0 1 2  etc.
			//   2 1 2
			//     2
			for (int w = 0; w < wmax; w++) {
				// Stop the loop when we can guarantee that no nodes will be closer than the ones we have already searched
				if (bestDistance < (w-2)*Math.Max(TileWorldSizeX, TileWorldSizeX)) break;

				int zmax = Math.Min(w+tileCoords.y +1, tileZCount);
				for (int z = Math.Max(-w+tileCoords.y, 0); z < zmax; z++) {
					// Solve for z such that abs(x-tx) + abs(z-tx) == w
					// Delta X coordinate
					int originalDx = Math.Abs(w - Math.Abs(z-tileCoords.y));
					var dx = originalDx;
					// Solution is dx + tx and -dx + tx
					// This loop will first check +dx and then -dx
					// If dx happens to be zero, then it will not run twice
					do {
						// Absolute x coordinate
						int x = -dx + tileCoords.x;
						if (x >= 0 && x < tileXCount) {
							NavmeshTile tile = tiles[x + z*tileXCount];

							if (tile != null) {
								if (xzSearch) {
									best = tile.bbTree.QueryClosestXZ(position, constraint, ref bestDistance, best);
								} else {
									best = tile.bbTree.QueryClosest(position, constraint, ref bestDistance, best);
								}
							}
						}

						dx = -dx;
					} while (dx != originalDx);
				}
			}

			best.node = best.constrainedNode;
			best.constrainedNode = null;
			best.clampedPosition = best.constClampedPosition;

			return best;
		}

		/** Fills graph with tiles created by NewEmptyTile */
		public void FillWithEmptyTiles () {
			for (int z = 0; z < tileZCount; z++) {
				for (int x = 0; x < tileXCount; x++) {
					tiles[z*tileXCount + x] = NewEmptyTile(x, z);
				}
			}
		}

		/** Create connections between all nodes.
		 * \version Since 3.7.6 the implementation is thread safe
		 */
		public static void CreateNodeConnections (TriangleMeshNode[] nodes) {
			List<Connection> connections = ListPool<Connection>.Claim();

			var nodeRefs = ObjectPoolSimple<Dictionary<Int2, int> >.Claim();
			nodeRefs.Clear();

			// Build node neighbours
			for (int i = 0; i < nodes.Length; i++) {
				TriangleMeshNode node = nodes[i];

				int av = node.GetVertexCount();

				for (int a = 0; a < av; a++) {
					// Recast can in some very special cases generate degenerate triangles which are simply lines
					// In that case, duplicate keys might be added and thus an exception will be thrown
					// It is safe to ignore the second edge though... I think (only found one case where this happens)
					var key = new Int2(node.GetVertexIndex(a), node.GetVertexIndex((a+1) % av));
					if (!nodeRefs.ContainsKey(key)) {
						nodeRefs.Add(key, i);
					}
				}
			}

			for (int i = 0; i < nodes.Length; i++) {
				TriangleMeshNode node = nodes[i];

				connections.Clear();

				int av = node.GetVertexCount();

				for (int a = 0; a < av; a++) {
					int first = node.GetVertexIndex(a);
					int second = node.GetVertexIndex((a+1) % av);
					int connNode;

					if (nodeRefs.TryGetValue(new Int2(second, first), out connNode)) {
						TriangleMeshNode other = nodes[connNode];

						int bv = other.GetVertexCount();

						for (int b = 0; b < bv; b++) {
							/** \todo This will fail on edges which are only partially shared */
							if (other.GetVertexIndex(b) == second && other.GetVertexIndex((b+1) % bv) == first) {
								connections.Add(new Connection(
										other,
										(uint)(node.position - other.position).costMagnitude,
										(byte)a
										));
								break;
							}
						}
					}
				}

				node.connections = connections.ToArrayFromPool();
			}

			nodeRefs.Clear();
			ObjectPoolSimple<Dictionary<Int2, int> >.Release(ref nodeRefs);
			ListPool<Connection>.Release(ref connections);
		}

		/** Generate connections between the two tiles.
		 * The tiles must be adjacent.
		 */
		public void ConnectTiles (NavmeshTile tile1, NavmeshTile tile2) {
			if (tile1 == null || tile2 == null) return;

			if (tile1.nodes == null) throw new System.ArgumentException("tile1 does not contain any nodes");
			if (tile2.nodes == null) throw new System.ArgumentException("tile2 does not contain any nodes");

			int t1x = Mathf.Clamp(tile2.x, tile1.x, tile1.x+tile1.w-1);
			int t2x = Mathf.Clamp(tile1.x, tile2.x, tile2.x+tile2.w-1);
			int t1z = Mathf.Clamp(tile2.z, tile1.z, tile1.z+tile1.d-1);
			int t2z = Mathf.Clamp(tile1.z, tile2.z, tile2.z+tile2.d-1);

			int coord, altcoord;
			int t1coord, t2coord;

			float tileWorldSize;

			// Figure out which side that is shared between the two tiles
			// and what coordinate index is fixed along that edge (x or z)
			if (t1x == t2x) {
				coord = 2;
				altcoord = 0;
				t1coord = t1z;
				t2coord = t2z;
				tileWorldSize = TileWorldSizeZ;
			} else if (t1z == t2z) {
				coord = 0;
				altcoord = 2;
				t1coord = t1x;
				t2coord = t2x;
				tileWorldSize = TileWorldSizeX;
			} else {
				throw new System.ArgumentException("Tiles are not adjacent (neither x or z coordinates match)");
			}

			if (Math.Abs(t1coord-t2coord) != 1) {
				throw new System.ArgumentException("Tiles are not adjacent (tile coordinates must differ by exactly 1. Got '" + t1coord + "' and '" + t2coord + "')");
			}

			// Midpoint between the two tiles
			int midpoint = (int)Math.Round((Math.Max(t1coord, t2coord) * tileWorldSize) * Int3.Precision);

			#if ASTARDEBUG
			Vector3 v1 = new Vector3(-100, 0, -100);
			Vector3 v2 = new Vector3(100, 0, 100);
			v1[coord] = midpoint*Int3.PrecisionFactor;
			v2[coord] = midpoint*Int3.PrecisionFactor;

			Debug.DrawLine(v1, v2, Color.magenta);
			#endif

			TriangleMeshNode[] nodes1 = tile1.nodes;
			TriangleMeshNode[] nodes2 = tile2.nodes;

			// Find adjacent nodes on the border between the tiles
			for (int i = 0; i < nodes1.Length; i++) {
				TriangleMeshNode nodeA = nodes1[i];
				int aVertexCount = nodeA.GetVertexCount();

				// Loop through all *sides* of the node
				for (int a = 0; a < aVertexCount; a++) {
					// Vertices that the segment consists of
					Int3 aVertex1 = nodeA.GetVertexInGraphSpace(a);
					Int3 aVertex2 = nodeA.GetVertexInGraphSpace((a+1) % aVertexCount);

					// Check if it is really close to the tile border
					if (Math.Abs(aVertex1[coord] - midpoint) < 2 && Math.Abs(aVertex2[coord] - midpoint) < 2) {
						int minalt = Math.Min(aVertex1[altcoord], aVertex2[altcoord]);
						int maxalt = Math.Max(aVertex1[altcoord], aVertex2[altcoord]);

						// Degenerate edge
						if (minalt == maxalt) continue;

						for (int j = 0; j < nodes2.Length; j++) {
							TriangleMeshNode nodeB = nodes2[j];
							int bVertexCount = nodeB.GetVertexCount();
							for (int b = 0; b < bVertexCount; b++) {
								Int3 bVertex1 = nodeB.GetVertexInGraphSpace(b);
								Int3 bVertex2 = nodeB.GetVertexInGraphSpace((b+1) % aVertexCount);
								if (Math.Abs(bVertex1[coord] - midpoint) < 2 && Math.Abs(bVertex2[coord] - midpoint) < 2) {
									int minalt2 = Math.Min(bVertex1[altcoord], bVertex2[altcoord]);
									int maxalt2 = Math.Max(bVertex1[altcoord], bVertex2[altcoord]);

									// Degenerate edge
									if (minalt2 == maxalt2) continue;

									if (maxalt > minalt2 && minalt < maxalt2) {
										// The two nodes seem to be adjacent

										// Test shortest distance between the segments (first test if they are equal since that is much faster and pretty common)
										if ((aVertex1 == bVertex1 && aVertex2 == bVertex2) || (aVertex1 == bVertex2 && aVertex2 == bVertex1) ||
											VectorMath.SqrDistanceSegmentSegment((Vector3)aVertex1, (Vector3)aVertex2, (Vector3)bVertex1, (Vector3)bVertex2) < MaxTileConnectionEdgeDistance*MaxTileConnectionEdgeDistance) {
											uint cost = (uint)(nodeA.position - nodeB.position).costMagnitude;

											nodeA.AddConnection(nodeB, cost, a);
											nodeB.AddConnection(nodeA, cost, b);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		/** Temporary buffer used in #PrepareNodeRecycling */
		Dictionary<int, int> nodeRecyclingHashBuffer = new Dictionary<int, int>();

		/** Reuse nodes that keep the exact same vertices after a tile replacement.
		 * The reused nodes will be added to the \a recycledNodeBuffer array at the index corresponding to the
		 * indices in the triangle array that its vertices uses.
		 *
		 * All connections on the reused nodes will be removed except ones that go to other graphs.
		 * The reused nodes will be removed from the tile by replacing it with a null slot in the node array.
		 *
		 * \see #ReplaceTile
		 */
		void PrepareNodeRecycling (int x, int z, Int3[] verts, int[] tris, TriangleMeshNode[] recycledNodeBuffer) {
			NavmeshTile tile = GetTile(x, z);

			if (tile == null || tile.nodes.Length == 0) return;
			var nodes = tile.nodes;
			var recycling = nodeRecyclingHashBuffer;
			for (int i = 0, j = 0; i < tris.Length; i += 3, j++) {
				recycling[verts[tris[i+0]].GetHashCode() + verts[tris[i+1]].GetHashCode() + verts[tris[i+2]].GetHashCode()] = j;
			}
			var connectionsToKeep = ListPool<Connection>.Claim();

			for (int i = 0; i < nodes.Length; i++) {
				var node = nodes[i];
				Int3 v0, v1, v2;
				node.GetVerticesInGraphSpace(out v0, out v1, out v2);
				var hash = v0.GetHashCode() + v1.GetHashCode() + v2.GetHashCode();
				int newNodeIndex;
				if (recycling.TryGetValue(hash, out newNodeIndex)) {
					// Technically we should check for a cyclic permutations of the vertices (e.g node a,b,c could become node b,c,a)
					// but in almost all cases the vertices will keep the same order. Allocating one or two extra nodes isn't such a big deal.
					if (verts[tris[3*newNodeIndex+0]] == v0 && verts[tris[3*newNodeIndex+1]] == v1 && verts[tris[3*newNodeIndex+2]] == v2) {
						recycledNodeBuffer[newNodeIndex] = node;
						// Remove the node from the tile
						nodes[i] = null;
						// Only keep connections to nodes on other graphs
						// Usually there are no connections to nodes to other graphs and this is faster than removing all connections them one by one
						for (int j = 0; j < node.connections.Length; j++) {
							if (node.connections[j].node.GraphIndex != node.GraphIndex) {
								connectionsToKeep.Add(node.connections[j]);
							}
						}
						ArrayPool<Connection>.Release(ref node.connections, true);
						if (connectionsToKeep.Count > 0) {
							node.connections = connectionsToKeep.ToArrayFromPool();
							connectionsToKeep.Clear();
						}
					}
				}
			}

			recycling.Clear();
			ListPool<Connection>.Release(ref connectionsToKeep);
		}

		public void CreateNodes (TriangleMeshNode[] buffer, int[] tris, int tileIndex, uint graphIndex) {
			if (buffer == null || buffer.Length < tris.Length/3) throw new System.ArgumentException("buffer must be non null and at least as large as tris.Length/3");
			// This index will be ORed to the triangle indices
			tileIndex <<= TileIndexOffset;

			// Create nodes and assign vertex indices
			for (int i = 0; i < buffer.Length; i++) {
				var node = buffer[i];
				// Allow the buffer to be partially filled in already to allow for recycling nodes
				if (node == null) node = buffer[i] = new TriangleMeshNode();

				// Reset all relevant fields on the node (even on recycled nodes to avoid exposing internal implementation details)
				node.Walkable = true;
				node.Tag = 0;
				node.Penalty = initialPenalty;
				node.GraphIndex = graphIndex;
				// The vertices stored on the node are composed
				// out of the triangle index and the tile index
				node.v0 = tris[i*3+0] | tileIndex;
				node.v1 = tris[i*3+1] | tileIndex;
				node.v2 = tris[i*3+2] | tileIndex;

				// Make sure the triangle is clockwise in graph space (it may not be in world space since the graphs can be rotated)
				if (RecalculateNormals && !VectorMath.IsClockwiseXZ(node.GetVertexInGraphSpace(0), node.GetVertexInGraphSpace(1), node.GetVertexInGraphSpace(2))) {
					Memory.Swap(ref node.v0, ref node.v2);
				}

				node.UpdatePositionFromVertices();
			}
		}

		/** Returns if there is an obstacle between \a origin and \a end on the graph.
		 * This is not the same as Physics.Linecast, this function traverses the \b graph and looks for collisions instead of checking for collider intersection.
		 *
		 * \astarpro
		 *
		 * \shadowimage{linecast.png}
		 */
		public bool Linecast (Vector3 origin, Vector3 end) {
			return Linecast(origin, end, GetNearest(origin, NNConstraint.None).node);
		}

		/** Returns if there is an obstacle between \a origin and \a end on the graph.
		 * \param [in] origin Point to linecast from
		 * \param [in] end Point to linecast to
		 * \param [out] hit Contains info on what was hit, see GraphHitInfo
		 * \param [in] hint You need to pass the node closest to the start point
		 *
		 * This is not the same as Physics.Linecast, this function traverses the \b graph and looks for collisions instead of checking for collider intersection.
		 * \astarpro
		 *
		 * \shadowimage{linecast.png}
		 */
		public bool Linecast (Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit) {
			return Linecast(this, origin, end, hint, out hit, null);
		}

		/** Returns if there is an obstacle between \a origin and \a end on the graph.
		 * \param [in] origin Point to linecast from
		 * \param [in] end Point to linecast to
		 * \param [in] hint You need to pass the node closest to the start point
		 *
		 * This is not the same as Physics.Linecast, this function traverses the \b graph and looks for collisions instead of checking for collider intersection.
		 * \astarpro
		 *
		 * \shadowimage{linecast.png}
		 */
		public bool Linecast (Vector3 origin, Vector3 end, GraphNode hint) {
			GraphHitInfo hit;

			return Linecast(this, origin, end, hint, out hit, null);
		}

		/** Returns if there is an obstacle between \a origin and \a end on the graph.
		 * \param [in] origin Point to linecast from
		 * \param [in] end Point to linecast to
		 * \param [out] hit Contains info on what was hit, see GraphHitInfo
		 * \param [in] hint You need to pass the node closest to the start point
		 * \param trace If a list is passed, then it will be filled with all nodes the linecast traverses
		 *
		 * This is not the same as Physics.Linecast, this function traverses the \b graph and looks for collisions instead of checking for collider intersection.
		 * \astarpro
		 *
		 * \shadowimage{linecast.png}
		 */
		public bool Linecast (Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace) {
			return Linecast(this, origin, end, hint, out hit, trace);
		}

		/** Returns if there is an obstacle between \a origin and \a end on the graph.
		 * \param [in] graph The graph to perform the search on
		 * \param [in] origin Point to start from
		 * \param [in] end Point to linecast to
		 * \param [out] hit Contains info on what was hit, see GraphHitInfo
		 * \param [in] hint You need to pass the node closest to the start point, if null, a search for the closest node will be done
		 *
		 * This is not the same as Physics.Linecast, this function traverses the \b graph and looks for collisions instead of checking for collider intersection.
		 * \astarpro
		 *
		 * \shadowimage{linecast.png}
		 */
		public static bool Linecast (NavmeshBase graph, Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit) {
			return Linecast(graph, origin, end, hint, out hit, null);
		}

		/** Cached \link Pathfinding.NNConstraint.None NNConstraint.None\endlink to reduce allocations */
		static readonly NNConstraint NNConstraintNone = NNConstraint.None;

		/** Used to optimize linecasts by precomputing some values */
		static readonly byte[] LinecastShapeEdgeLookup;

		static NavmeshBase () {
			// Want want to figure out which side of a triangle that a ray exists using.
			// There are only 3*3*3 = 27 different options for the [left/right/colinear] options for the 3 vertices of a triangle.
			// So we can precompute the result to improve the performance of linecasts.
			// For simplicity we reserve 2 bits for each side which means that we have 4*4*4 = 64 entries in the lookup table.
			LinecastShapeEdgeLookup = new byte[64];
			Side[] sideOfLine = new Side[3];
			for (int i = 0; i < LinecastShapeEdgeLookup.Length; i++) {
				sideOfLine[0] = (Side)((i >> 0) & 0x3);
				sideOfLine[1] = (Side)((i >> 2) & 0x3);
				sideOfLine[2] = (Side)((i >> 4) & 0x3);
				LinecastShapeEdgeLookup[i] = 0xFF;
				// Value 3 is an invalid value. So we just skip it.
				if (sideOfLine[0] != (Side)3 && sideOfLine[1] != (Side)3 && sideOfLine[2] != (Side)3) {
					// Figure out the side of the triangle that the line exits.
					// In case the line passes through one of the vertices of the triangle
					// there may be multiple alternatives. In that case pick the edge
					// which contains the fewest vertices that lie on the line.
					// This prevents a potential infinite loop when a linecast is done colinear
					// to the edge of a triangle.
					int bestBadness = int.MaxValue;
					for (int j = 0; j < 3; j++) {
						if ((sideOfLine[j] == Side.Left || sideOfLine[j] == Side.Colinear) && (sideOfLine[(j+1)%3] == Side.Right || sideOfLine[(j+1)%3] == Side.Colinear)) {
							var badness = (sideOfLine[j] == Side.Colinear ? 1 : 0) + (sideOfLine[(j+1)%3] == Side.Colinear ? 1 : 0);
							if (badness < bestBadness) {
								LinecastShapeEdgeLookup[i] = (byte)j;
								bestBadness = badness;
							}
						}
					}
				}
			}
		}

		/** Returns if there is an obstacle between \a origin and \a end on the graph.
		 * \param [in] graph The graph to perform the search on
		 * \param [in] origin Point to start from. This point should be on the navmesh. It will be snapped to the closest point on the navmesh otherwise.
		 * \param [in] end Point to linecast to
		 * \param [out] hit Contains info on what was hit, see GraphHitInfo
		 * \param [in] hint If you already know the node which contains the \a origin point, you may pass it here for slighly improved performance. If null, a search for the closest node will be done.
		 * \param trace If a list is passed, then it will be filled with all nodes along the line up until it hits an obstacle or reaches the end.
		 *
		 * This is not the same as Physics.Linecast, this function traverses the \b graph and looks for collisions instead of checking for collider intersections.
		 *
		 * \note This method only makes sense for graphs in which there is a definite 'up' direction. For example it does not make sense for e.g spherical graphs,
		 * navmeshes in which characters can walk on walls/ceilings or other curved worlds. If you try to use this method on such navmeshes it may output nonsense.
		 *
		 * \astarpro
		 *
		 * \shadowimage{linecast.png}
		 */
		public static bool Linecast (NavmeshBase graph, Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace) {
			hit = new GraphHitInfo();

			if (float.IsNaN(origin.x + origin.y + origin.z)) throw new System.ArgumentException("origin is NaN");
			if (float.IsNaN(end.x + end.y + end.z)) throw new System.ArgumentException("end is NaN");

			var node = hint as TriangleMeshNode;
			if (node == null) {
				node = graph.GetNearest(origin, NNConstraintNone).node as TriangleMeshNode;

				if (node == null) {
#if !SERVER
					UnityEngine.Debug.LogError("Could not find a valid node to start from");
#endif
					hit.origin = origin;
					hit.point = origin;
					return true;
				}
			}

			// Snap the origin to the navmesh
			var i3originInGraphSpace = node.ClosestPointOnNodeXZInGraphSpace(origin);
			hit.origin = graph.transform.Transform((Vector3)i3originInGraphSpace);

			if (!node.Walkable) {
				hit.node = node;
				hit.point = hit.origin;
				hit.tangentOrigin = hit.origin;
				return true;
			}

			var endInGraphSpace = graph.transform.InverseTransform(end);
			var i3endInGraphSpace = (Int3)endInGraphSpace;

			// Fast early out check
			if (i3originInGraphSpace == i3endInGraphSpace) {
				hit.point = hit.origin;
				hit.node = node;
				return false;
			}

			int counter = 0;
			while (true) {
				counter++;
				if (counter > 2000) {
#if !SERVER
					UnityEngine.Debug.LogError("Linecast was stuck in infinite loop. Breaking.");
#endif
					return true;
				}

				if (trace != null) trace.Add(node);

				Int3 a0, a1, a2;
				node.GetVerticesInGraphSpace(out a0, out a1, out a2);
				int sideOfLine = (byte)VectorMath.SideXZ(i3originInGraphSpace, i3endInGraphSpace, a0);
				sideOfLine |= (byte)VectorMath.SideXZ(i3originInGraphSpace, i3endInGraphSpace, a1) << 2;
				sideOfLine |= (byte)VectorMath.SideXZ(i3originInGraphSpace, i3endInGraphSpace, a2) << 4;
				// Use a lookup table to figure out which side of this triangle that the ray exits
				int shapeEdgeA = (int)LinecastShapeEdgeLookup[sideOfLine];
				// The edge consists of the vertex with index 'sharedEdgeA' and the next vertex after that (index '(sharedEdgeA+1)%3')

				var sideNodeExit = VectorMath.SideXZ(shapeEdgeA == 0 ? a0 : (shapeEdgeA == 1 ? a1 : a2), shapeEdgeA == 0 ? a1 : (shapeEdgeA == 1 ? a2 : a0), i3endInGraphSpace);
				if (sideNodeExit != Side.Left) {
					// Ray stops before it leaves the current node.
					// The endpoint must be inside the current node.
					hit.point = end;
					hit.node = node;
					return false;
				}

				if (shapeEdgeA == 0xFF) {
					// Line does not intersect node at all?
					// This may theoretically happen if the origin was not properly snapped to the inside of the triangle, but is instead a tiny distance outside the node.
#if !SERVER
					UnityEngine.Debug.LogError("Line does not intersect node at all");
#endif
					hit.node = node;
					hit.point = hit.tangentOrigin = hit.origin;
					return true;
				} else {
					bool success = false;
					var nodeConnections = node.connections;
					for (int i = 0; i < nodeConnections.Length; i++) {
						if (nodeConnections[i].shapeEdge == shapeEdgeA) {
							// This might be the next node that we enter

							var neighbour = nodeConnections[i].node as TriangleMeshNode;
							if (neighbour == null || !neighbour.Walkable) continue;

							var neighbourConnections = neighbour.connections;
							int shapeEdgeB = -1;
							for (int j = 0; j < neighbourConnections.Length; j++) {
								if (neighbourConnections[j].node == node) {
									shapeEdgeB = neighbourConnections[j].shapeEdge;
									break;
								}
							}

							if (shapeEdgeB == -1) {
								// Connection was mono-directional!
								// This shouldn't normally not happen on navmeshes happen on navmeshes (when the shapeEdge matches at least) unless a user has done something strange to the navmesh.
								continue;
							}

							var side1 = VectorMath.SideXZ(i3originInGraphSpace, i3endInGraphSpace, neighbour.GetVertexInGraphSpace(shapeEdgeB));
							var side2 = VectorMath.SideXZ(i3originInGraphSpace, i3endInGraphSpace, neighbour.GetVertexInGraphSpace((shapeEdgeB+1) % 3));

							// Check if the line enters this edge
							success = (side1 == Side.Right || side1 == Side.Colinear) && (side2 == Side.Left || side2 == Side.Colinear);

							if (!success) continue;

							// Ray has entered the neighbouring node.
							// After the first node, it is possible to prove the loop invariant that shapeEdgeA will *never* end up as -1 (checked above)
							// Since side = Colinear acts essentially as a wildcard. side1 and side2 can be the most restricted if they are side1=right, side2=left.
							// Then when we get to the next node we know that the sideOfLine array is either [*, Right, Left], [Left, *, Right] or [Right, Left, *], where * is unknown.
							// We are looking for the sequence [Left, Right] (possibly including Colinear as wildcard). We will always find this sequence regardless of the value of *.
							node = neighbour;
							break;
						}
					}

					if (!success) {
						// Node did not enter any neighbours
						// It must have hit the border of the navmesh
						var hitEdgeStartInGraphSpace = (Vector3)(shapeEdgeA == 0 ? a0 : (shapeEdgeA == 1 ? a1 : a2));
						var hitEdgeEndInGraphSpace = (Vector3)(shapeEdgeA == 0 ? a1 : (shapeEdgeA == 1 ? a2 : a0));
						var intersectionInGraphSpace = VectorMath.LineIntersectionPointXZ(hitEdgeStartInGraphSpace, hitEdgeEndInGraphSpace, (Vector3)i3originInGraphSpace, (Vector3)i3endInGraphSpace);
						hit.point = graph.transform.Transform(intersectionInGraphSpace);
						hit.node = node;
						var hitEdgeStart = graph.transform.Transform(hitEdgeStartInGraphSpace);
						var hitEdgeEnd = graph.transform.Transform(hitEdgeEndInGraphSpace);
						hit.tangent = hitEdgeEnd - hitEdgeStart;
						hit.tangentOrigin = hitEdgeStart;
						return true;
					}
				}
			}
		}

		/** Serializes Node Info.
		 * Should serialize:
		 * - Base
		 *    - Node Flags
		 *    - Node Penalties
		 *    - Node
		 * - Node Positions (if applicable)
		 * - Any other information necessary to load the graph in-game
		 * All settings marked with json attributes (e.g JsonMember) have already been
		 * saved as graph settings and do not need to be handled here.
		 *
		 * It is not necessary for this implementation to be forward or backwards compatible.
		 *
		 * \see
		 */
		protected override void SerializeExtraInfo (GraphSerializationContext ctx) {
			BinaryWriter writer = ctx.writer;

			if (tiles == null) {
				writer.Write(-1);
				return;
			}
			writer.Write(tileXCount);
			writer.Write(tileZCount);

			for (int z = 0; z < tileZCount; z++) {
				for (int x = 0; x < tileXCount; x++) {
					NavmeshTile tile = tiles[x + z*tileXCount];

					if (tile == null) {
						throw new System.Exception("NULL Tile");
						//writer.Write (-1);
						//continue;
					}

					writer.Write(tile.x);
					writer.Write(tile.z);

					if (tile.x != x || tile.z != z) continue;

					writer.Write(tile.w);
					writer.Write(tile.d);

					writer.Write(tile.tris.Length);

					for (int i = 0; i < tile.tris.Length; i++) writer.Write(tile.tris[i]);

					writer.Write(tile.verts.Length);
					for (int i = 0; i < tile.verts.Length; i++) {
						ctx.SerializeInt3(tile.verts[i]);
					}

					writer.Write(tile.vertsInGraphSpace.Length);
					for (int i = 0; i < tile.vertsInGraphSpace.Length; i++) {
						ctx.SerializeInt3(tile.vertsInGraphSpace[i]);
					}

					writer.Write(tile.nodes.Length);
					for (int i = 0; i < tile.nodes.Length; i++) {
						tile.nodes[i].SerializeNode(ctx);
					}
				}
			}
		}

		public abstract GraphTransform CalculateTransform();

		protected override void DeserializeExtraInfo (GraphSerializationContext ctx) {
			BinaryReader reader = ctx.reader;

			tileXCount = reader.ReadInt32();

			if (tileXCount < 0) return;

			tileZCount = reader.ReadInt32();
			transform = CalculateTransform();

			tiles = new NavmeshTile[tileXCount * tileZCount];

			//Make sure mesh nodes can reference this graph
			TriangleMeshNode.SetNavmeshHolder((int)ctx.graphIndex, this);

			for (int z = 0; z < tileZCount; z++) {
				for (int x = 0; x < tileXCount; x++) {
					int tileIndex = x + z*tileXCount;
					int tx = reader.ReadInt32();
					if (tx < 0) throw new System.Exception("Invalid tile coordinates (x < 0)");

					int tz = reader.ReadInt32();
					if (tz < 0) throw new System.Exception("Invalid tile coordinates (z < 0)");

					// This is not the origin of a large tile. Refer back to that tile.
					if (tx != x || tz != z) {
						tiles[tileIndex] = tiles[tz*tileXCount + tx];
						continue;
					}

					var tile = tiles[tileIndex] = new NavmeshTile {
						x = tx,
						z = tz,
						w = reader.ReadInt32(),
						d = reader.ReadInt32(),
						bbTree = ObjectPool<BBTree>.Claim(),
						graph = this,
					};

					int trisCount = reader.ReadInt32();

					if (trisCount % 3 != 0) throw new System.Exception("Corrupt data. Triangle indices count must be divisable by 3. Read " + trisCount);

					tile.tris = new int[trisCount];
					for (int i = 0; i < tile.tris.Length; i++) tile.tris[i] = reader.ReadInt32();

					tile.verts = new Int3[reader.ReadInt32()];
					for (int i = 0; i < tile.verts.Length; i++) {
						tile.verts[i] = ctx.DeserializeInt3();
					}

					if (ctx.meta.version.Major >= 4) {
						tile.vertsInGraphSpace = new Int3[reader.ReadInt32()];
						if (tile.vertsInGraphSpace.Length != tile.verts.Length) throw new System.Exception("Corrupt data. Array lengths did not match");
						for (int i = 0; i < tile.verts.Length; i++) {
							tile.vertsInGraphSpace[i] = ctx.DeserializeInt3();
						}
					} else {
						// Compatibility
						tile.vertsInGraphSpace = new Int3[tile.verts.Length];
						tile.verts.CopyTo(tile.vertsInGraphSpace, 0);
						transform.InverseTransform(tile.vertsInGraphSpace);
					}

					int nodeCount = reader.ReadInt32();
					tile.nodes = new TriangleMeshNode[nodeCount];

					// Prepare for storing in vertex indices
					tileIndex <<= TileIndexOffset;

					for (int i = 0; i < tile.nodes.Length; i++) {
						var node = new TriangleMeshNode();
						tile.nodes[i] = node;

						node.DeserializeNode(ctx);

						node.v0 = tile.tris[i*3+0] | tileIndex;
						node.v1 = tile.tris[i*3+1] | tileIndex;
						node.v2 = tile.tris[i*3+2] | tileIndex;
						node.UpdatePositionFromVertices();
					}

					tile.bbTree.RebuildFrom(tile.nodes);
				}
			}
		}

		protected override void PostDeserialization (GraphSerializationContext ctx) {
			// Compatibility
			if (ctx.meta.version < AstarSerializer.V4_1_0 && tiles != null) {
				Dictionary<TriangleMeshNode, Connection[]> conns = tiles.SelectMany(s => s.nodes).ToDictionary(n => n, n => n.connections ?? new Connection[0]);
				// We need to recalculate all connections when upgrading data from earlier than 4.1.0
				// as the connections now need information about which edge was used.
				// This may remove connections for e.g off-mesh links.
				foreach (var tile in tiles) CreateNodeConnections(tile.nodes);
				foreach (var tile in tiles) ConnectTileWithNeighbours(tile);

				// Restore any connections that were contained in the serialized file but didn't get added by the method calls above
				GetNodes(node => {
					var triNode = node as TriangleMeshNode;
					foreach (var conn in conns[triNode].Where(conn => !triNode.ContainsConnection(conn.node)).ToList()) {
						triNode.AddConnection(conn.node, conn.cost, conn.shapeEdge);
					}
				});
			}

			// Make sure that the transform is up to date.
			// It is assumed that the current graph settings correspond to the correct
			// transform as it is not serialized itself.
			transform = CalculateTransform();
		}
	}
}
