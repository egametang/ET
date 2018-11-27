namespace PF {
	/** Interface for something that holds a triangle based navmesh */
	public interface INavmeshHolder : ITransformedGraph, INavmesh {
		/** Position of vertex number i in the world */
		Int3 GetVertex (int i);

		/** Position of vertex number i in coordinates local to the graph.
		 * The up direction is always the +Y axis for these coordinates.
		 */
		Int3 GetVertexInGraphSpace (int i);

		int GetVertexArrayIndex (int index);

		/** Transforms coordinates from graph space to world space */
		void GetTileCoordinates (int tileIndex, out int x, out int z);
	}

	/** Node represented by a triangle */
	public class TriangleMeshNode : MeshNode {

		/** Internal vertex index for the first vertex */
		public int v0;

		/** Internal vertex index for the second vertex */
		public int v1;

		/** Internal vertex index for the third vertex */
		public int v2;

		/** Holds INavmeshHolder references for all graph indices to be able to access them in a performant manner */
		protected static INavmeshHolder[] _navmeshHolders = new INavmeshHolder[0];

		/** Used for synchronised access to the #_navmeshHolders array */
		protected static readonly System.Object lockObject = new System.Object();

		public static INavmeshHolder GetNavmeshHolder (uint graphIndex) {
			return _navmeshHolders[(int)graphIndex];
		}

		/** Sets the internal navmesh holder for a given graph index.
		 * \warning Internal method
		 */
		public static void SetNavmeshHolder (int graphIndex, INavmeshHolder graph) {
			// We need to lock to make sure that
			// the resize operation is thread safe
			lock (lockObject) {
				if (graphIndex >= _navmeshHolders.Length) {
					var gg = new INavmeshHolder[graphIndex+1];
					_navmeshHolders.CopyTo(gg, 0);
					_navmeshHolders = gg;
				}
				_navmeshHolders[graphIndex] = graph;
			}
		}

		/** Set the position of this node to the average of its 3 vertices */
		public void UpdatePositionFromVertices () {
			Int3 a, b, c;

			GetVertices(out a, out b, out c);
			position = (a + b + c) * 0.333333f;
		}

		/** Return a number identifying a vertex.
		 * This number does not necessarily need to be a index in an array but two different vertices (in the same graph) should
		 * not have the same vertex numbers.
		 */
		public int GetVertexIndex (int i) {
			return i == 0 ? v0 : (i == 1 ? v1 : v2);
		}

		/** Return a number specifying an index in the source vertex array.
		 * The vertex array can for example be contained in a recast tile, or be a navmesh graph, that is graph dependant.
		 * This is slower than GetVertexIndex, if you only need to compare vertices, use GetVertexIndex.
		 */
		public int GetVertexArrayIndex (int i) {
			return GetNavmeshHolder(GraphIndex).GetVertexArrayIndex(i == 0 ? v0 : (i == 1 ? v1 : v2));
		}

		/** Returns all 3 vertices of this node in world space */
		public void GetVertices (out Int3 v0, out Int3 v1, out Int3 v2) {
			// Get the object holding the vertex data for this node
			// This is usually a graph or a recast graph tile
			var holder = GetNavmeshHolder(GraphIndex);

			v0 = holder.GetVertex(this.v0);
			v1 = holder.GetVertex(this.v1);
			v2 = holder.GetVertex(this.v2);
		}

		/** Returns all 3 vertices of this node in graph space */
		public void GetVerticesInGraphSpace (out Int3 v0, out Int3 v1, out Int3 v2) {
			// Get the object holding the vertex data for this node
			// This is usually a graph or a recast graph tile
			var holder = GetNavmeshHolder(GraphIndex);

			v0 = holder.GetVertexInGraphSpace(this.v0);
			v1 = holder.GetVertexInGraphSpace(this.v1);
			v2 = holder.GetVertexInGraphSpace(this.v2);
		}

		public override Int3 GetVertex (int i) {
			return GetNavmeshHolder(GraphIndex).GetVertex(GetVertexIndex(i));
		}

		public Int3 GetVertexInGraphSpace (int i) {
			return GetNavmeshHolder(GraphIndex).GetVertexInGraphSpace(GetVertexIndex(i));
		}

		public override int GetVertexCount () {
			// A triangle has 3 vertices
			return 3;
		}

		public override Vector3 ClosestPointOnNode (Vector3 p) {
			Int3 a, b, c;

			GetVertices(out a, out b, out c);
			return Polygon.ClosestPointOnTriangle((Vector3)a, (Vector3)b, (Vector3)c, p);
		}

		/** Closest point on the node when seen from above.
		 * This method is mostly for internal use as the #Pathfinding.NavmeshBase.Linecast methods use it.
		 *
		 * - The returned point is the closest one on the node to \a p when seen from above (relative to the graph).
		 *   This is important mostly for sloped surfaces.
		 * - The returned point is an Int3 point in graph space.
		 * - It is guaranteed to be inside the node, so if you call #ContainsPointInGraphSpace with the return value from this method the result is guaranteed to be true.
		 *
		 * This method is slower than e.g #ClosestPointOnNode or #ClosestPointOnNodeXZ.
		 * However they do not have the same guarantees as this method has.
		 */
		internal Int3 ClosestPointOnNodeXZInGraphSpace (Vector3 p) {
			// Get the vertices that make up the triangle
			Int3 a, b, c;

			GetVerticesInGraphSpace(out a, out b, out c);

			// Convert p to graph space
			p = GetNavmeshHolder(GraphIndex).transform.InverseTransform(p);

			// Find the closest point on the triangle to p when looking at the triangle from above (relative to the graph)
			var closest = Polygon.ClosestPointOnTriangleXZ((Vector3)a, (Vector3)b, (Vector3)c, p);

			// Make sure the point is actually inside the node
			var i3closest = (Int3)closest;
			if (ContainsPointInGraphSpace(i3closest)) {
				// Common case
				return i3closest;
			} else {
				// Annoying...
				// The closest point when converted from floating point coordinates to integer coordinates
				// is not actually inside the node. It needs to be inside the node for some methods
				// (like for example Linecast) to work properly.

				// Try the 8 integer coordinates around the closest point
				// and check if any one of them are completely inside the node.
				// This will most likely succeed as it should be very close.
				for (int dx = -1; dx <= 1; dx++) {
					for (int dz = -1; dz <= 1; dz++) {
						if ((dx != 0 || dz != 0)) {
							var candidate = new Int3(i3closest.x + dx, i3closest.y, i3closest.z + dz);
							if (ContainsPointInGraphSpace(candidate)) return candidate;
						}
					}
				}

				// Happens veery rarely.
				// Pick the closest vertex of the triangle.
				// The vertex is guaranteed to be inside the triangle.
				var da = (a - i3closest).sqrMagnitudeLong;
				var db = (b - i3closest).sqrMagnitudeLong;
				var dc = (c - i3closest).sqrMagnitudeLong;
				return da < db ? (da < dc ? a : c) : (db < dc ? b : c);
			}
		}

		public override Vector3 ClosestPointOnNodeXZ (Vector3 p) {
			// Get all 3 vertices for this node
			Int3 tp1, tp2, tp3;

			GetVertices(out tp1, out tp2, out tp3);
			return Polygon.ClosestPointOnTriangleXZ((Vector3)tp1, (Vector3)tp2, (Vector3)tp3, p);
		}

		public override bool ContainsPoint (Vector3 p) {
			return ContainsPointInGraphSpace((Int3)GetNavmeshHolder(GraphIndex).transform.InverseTransform(p));
		}

		public override bool ContainsPointInGraphSpace (Int3 p) {
			// Get all 3 vertices for this node
			Int3 a, b, c;

			GetVerticesInGraphSpace(out a, out b, out c);

			if ((long)(b.x - a.x) * (long)(p.z - a.z) - (long)(p.x - a.x) * (long)(b.z - a.z) > 0) return false;

			if ((long)(c.x - b.x) * (long)(p.z - b.z) - (long)(p.x - b.x) * (long)(c.z - b.z) > 0) return false;

			if ((long)(a.x - c.x) * (long)(p.z - c.z) - (long)(p.x - c.x) * (long)(a.z - c.z) > 0) return false;

			return true;
			// Equivalent code, but the above code is faster
			//return Polygon.IsClockwiseMargin (a,b, p) && Polygon.IsClockwiseMargin (b,c, p) && Polygon.IsClockwiseMargin (c,a, p);

			//return Polygon.ContainsPoint(g.GetVertex(v0),g.GetVertex(v1),g.GetVertex(v2),p);
		}

		public override void UpdateRecursiveG (Path path, PathNode pathNode, PathHandler handler) {
			pathNode.UpdateG(path);

			handler.heap.Add(pathNode);

			if (connections == null) return;

			for (int i = 0; i < connections.Length; i++) {
				GraphNode other = connections[i].node;
				PathNode otherPN = handler.GetPathNode(other);
				if (otherPN.parent == pathNode && otherPN.pathID == handler.PathID) other.UpdateRecursiveG(path, otherPN, handler);
			}
		}

		public override void Open (Path path, PathNode pathNode, PathHandler handler) {
			if (connections == null) return;

			// Flag2 indicates if this node needs special treatment
			// with regard to connection costs
			bool flag2 = pathNode.flag2;

			// Loop through all connections
			for (int i = connections.Length-1; i >= 0; i--) {
				var conn = connections[i];
				var other = conn.node;

				// Make sure we can traverse the neighbour
				if (path.CanTraverse(conn.node)) {
					PathNode pathOther = handler.GetPathNode(conn.node);

					// Fast path out, worth it for triangle mesh nodes since they usually have degree 2 or 3
					if (pathOther == pathNode.parent) {
						continue;
					}

					uint cost = conn.cost;

					if (flag2 || pathOther.flag2) {
						// Get special connection cost from the path
						// This is used by the start and end nodes
						cost = path.GetConnectionSpecialCost(this, conn.node, cost);
					}

					// Test if we have seen the other node before
					if (pathOther.pathID != handler.PathID) {
						// We have not seen the other node before
						// So the path from the start through this node to the other node
						// must be the shortest one so far

						// Might not be assigned
						pathOther.node = conn.node;

						pathOther.parent = pathNode;
						pathOther.pathID = handler.PathID;

						pathOther.cost = cost;

						pathOther.H = path.CalculateHScore(other);
						pathOther.UpdateG(path);

						handler.heap.Add(pathOther);
					} else {
						// If not we can test if the path from this node to the other one is a better one than the one already used
						if (pathNode.G + cost + path.GetTraversalCost(other) < pathOther.G) {
							pathOther.cost = cost;
							pathOther.parent = pathNode;

							other.UpdateRecursiveG(path, pathOther, handler);
						}
					}
				}
			}
		}

		/** Returns the edge which is shared with \a other.
		 * If no edge is shared, -1 is returned.
		 * If there is a connection with the other node, but the connection is not marked as using a particular edge of the shape of the node
		 * then 0xFF will be returned.
		 *
		 * The vertices in the edge can be retrieved using
		 * \code
		 * var edge = node.SharedEdge(other);
		 * var a = node.GetVertex(edge);
		 * var b = node.GetVertex((edge+1) % node.GetVertexCount());
		 * \endcode
		 *
		 * \see #GetPortal which also handles edges that are shared over tile borders and some types of node links
		 */
		public int SharedEdge (GraphNode other) {
			var edge = -1;

			for (int i = 0; i < connections.Length; i++) {
				if (connections[i].node == other) edge = connections[i].shapeEdge;
			}
			return edge;
		}

		public override bool GetPortal (GraphNode toNode, System.Collections.Generic.List<Vector3> left, System.Collections.Generic.List<Vector3> right, bool backwards) {
			int aIndex, bIndex;

			return GetPortal(toNode, left, right, backwards, out aIndex, out bIndex);
		}

		public bool GetPortal (GraphNode toNode, System.Collections.Generic.List<Vector3> left, System.Collections.Generic.List<Vector3> right, bool backwards, out int aIndex, out int bIndex) {
			aIndex = -1;
			bIndex = -1;

			//If the nodes are in different graphs, this function has no idea on how to find a shared edge.
			if (backwards || toNode.GraphIndex != GraphIndex) return false;

			// Since the nodes are in the same graph, they are both TriangleMeshNodes
			// So we don't need to care about other types of nodes
			var toTriNode = toNode as TriangleMeshNode;
			var edge = SharedEdge(toTriNode);

			// A connection was found, but it specifically didn't use an edge
			if (edge == 0xFF) return false;

			// No connection was found between the nodes
			// Check if there is a node link that connects them
			if (edge == -1) {
				return false;
			}

			aIndex = edge;
			bIndex = (edge + 1) % GetVertexCount();

			// Get the vertices of the shared edge for the first node
			Int3 v1a = GetVertex(edge);
			Int3 v1b = GetVertex((edge+1) % GetVertexCount());

			// Get tile indices
			int tileIndex1 = (GetVertexIndex(0) >> NavmeshBase.TileIndexOffset) & NavmeshBase.TileIndexMask;
			int tileIndex2 = (toTriNode.GetVertexIndex(0) >> NavmeshBase.TileIndexOffset) & NavmeshBase.TileIndexMask;

			if (tileIndex1 != tileIndex2) {
				// When the nodes are in different tiles, the edges might not be completely identical
				// so another technique is needed.

				// Get the tile coordinates, from them we can figure out which edge is going to be shared
				int x1, x2, z1, z2, coord;
				INavmeshHolder nm = GetNavmeshHolder(GraphIndex);
				nm.GetTileCoordinates(tileIndex1, out x1, out z1);
				nm.GetTileCoordinates(tileIndex2, out x2, out z2);

				if (System.Math.Abs(x1-x2) == 1) coord = 2;
				else if (System.Math.Abs(z1-z2) == 1) coord = 0;
				else return false; // Tiles are not adjacent. This is likely a custom connection between two nodes.

				var otherEdge = toTriNode.SharedEdge(this);

				// A connection was found, but it specifically didn't use an edge. This is odd since the connection in the other direction did use an edge
				if (otherEdge == 0xFF) throw new System.Exception("Connection used edge in one direction, but not in the other direction. Has the wrong overload of AddConnection been used?");

				// If it is -1 then it must be a one-way connection. Fall back to using the whole edge
				if (otherEdge != -1) {
					// When the nodes are in different tiles, they might not share exactly the same edge
					// so we clamp the portal to the segment of the edges which they both have.
					int mincoord = System.Math.Min(v1a[coord], v1b[coord]);
					int maxcoord = System.Math.Max(v1a[coord], v1b[coord]);

					// Get the vertices of the shared edge for the second node
					Int3 v2a = toTriNode.GetVertex(otherEdge);
					Int3 v2b = toTriNode.GetVertex((otherEdge+1) % toTriNode.GetVertexCount());

					mincoord = System.Math.Max(mincoord, System.Math.Min(v2a[coord], v2b[coord]));
					maxcoord = System.Math.Min(maxcoord, System.Math.Max(v2a[coord], v2b[coord]));

					if (v1a[coord] < v1b[coord]) {
						v1a[coord] = mincoord;
						v1b[coord] = maxcoord;
					} else {
						v1a[coord] = maxcoord;
						v1b[coord] = mincoord;
					}
				}
			}

			if (left != null) {
				// All triangles should be laid out in clockwise order so v1b is the rightmost vertex (seen from this node)
				left.Add((Vector3)v1a);
				right.Add((Vector3)v1b);
			}
			return true;
		}

		/** \todo This is the area in XZ space, use full 3D space for higher correctness maybe? */
		public override float SurfaceArea () {
			var holder = GetNavmeshHolder(GraphIndex);

			return System.Math.Abs(VectorMath.SignedTriangleAreaTimes2XZ(holder.GetVertex(v0), holder.GetVertex(v1), holder.GetVertex(v2))) * 0.5f;
		}

		public override void SerializeNode (GraphSerializationContext ctx) {
			base.SerializeNode(ctx);
			ctx.writer.Write(v0);
			ctx.writer.Write(v1);
			ctx.writer.Write(v2);
		}

		public override void DeserializeNode (GraphSerializationContext ctx) {
			base.DeserializeNode(ctx);
			v0 = ctx.reader.ReadInt32();
			v1 = ctx.reader.ReadInt32();
			v2 = ctx.reader.ReadInt32();
		}
	}
}
