using System.Collections.Generic;
using PF;
using Vector3 = UnityEngine.Vector3;

namespace Pathfinding {
	using Pathfinding.Util;

	/** Contains utility methods for getting useful information out of graph.
	 * This class works a lot with the #Pathfinding.GraphNode class, a useful function to get nodes is #AstarPath.GetNearest.
	 *
	 * \see #AstarPath.GetNearest
	 * \see #Pathfinding.GraphUpdateUtilities
	 * \see #Pathfinding.PathUtilities
	 *
	 * \ingroup utils
	 */
	public static class GraphUtilities
	{
		/** Convenience method to get a list of all segments of the contours of a graph.
		 * \returns A list of segments. Every 2 elements form a line segment. The first segment is (result[0], result[1]), the second one is (result[2], result[3]) etc.
		 * The line segments are oriented so that the navmesh is on the right side of the segments when seen from above.
		 *
		 * This method works for navmesh, recast, grid graphs and layered grid graphs. For other graph types it will return an empty list.
		 *
		 * If you need more information about how the contours are connected you can take a look at the other variants of this method.
		 *
		 * \snippet MiscSnippets.cs GraphUtilities.GetContours2
		 *
		 * \shadowimage{navmesh_contour.png}
		 * \shadowimage{grid_contour.png}
		 */
		public static List<Vector3> GetContours(NavGraph graph)
		{
			List<Vector3> result = ListPool<Vector3>.Claim();
			if (graph is INavmesh)
			{
				GetContours(graph as INavmesh, (vertices, cycle) =>
				{
					for (int j = cycle? vertices.Count - 1 : 0, i = 0; i < vertices.Count; j = i, i++)
					{
						result.Add((Vector3) vertices[j]);
						result.Add((Vector3) vertices[i]);
					}
				});
			}

			return result;
		}

		/** Traces the contour of a navmesh.
		 * \param navmesh The navmesh-like object to trace. This can be a recast or navmesh graph or it could be a single tile in one such graph.
		 * \param results Will be called once for each contour with the contour as a parameter as well as a boolean indicating if the contour is a cycle or a chain (see second image).
		 *
		 * \shadowimage{navmesh_contour.png}
		 *
		 * This image is just used to illustrate the difference between chains and cycles. That it shows a grid graph is not relevant.
		 * \shadowimage{grid_contour_compressed.png}
		 *
		 * \see #GetContours(NavGraph)
		 */
		public static void GetContours(INavmesh navmesh, System.Action<List<Int3>, bool> results)
		{
			// Assume 3 vertices per node
			var uses = new bool[3];

			var outline = new Dictionary<int, int>();
			var vertexPositions = new Dictionary<int, Int3>();
			var hasInEdge = new HashSet<int>();

			navmesh.GetNodes(_node =>
			{
				var node = _node as TriangleMeshNode;

				uses[0] = uses[1] = uses[2] = false;

				if (node != null)
				{
					// Find out which edges are shared with other nodes
					for (int j = 0; j < node.connections.Length; j++)
					{
						var other = node.connections[j].node as TriangleMeshNode;

						// Not necessarily a TriangleMeshNode
						if (other != null)
						{
							int a = node.SharedEdge(other);
							if (a != -1) uses[a] = true;
						}
					}

					// Loop through all edges on the node
					for (int j = 0; j < 3; j++)
					{
						// The edge is not shared with any other node
						// I.e it is an exterior edge on the mesh
						if (!uses[j])
						{
							var i1 = j;
							var i2 = (j + 1) % node.GetVertexCount();

							outline[node.GetVertexIndex(i1)] = node.GetVertexIndex(i2);
							hasInEdge.Add(node.GetVertexIndex(i2));
							vertexPositions[node.GetVertexIndex(i1)] = node.GetVertex(i1);
							vertexPositions[node.GetVertexIndex(i2)] = node.GetVertex(i2);
						}
					}
				}
			});

			Polygon.TraceContours(outline, hasInEdge, (chain, cycle) =>
			{
				List<Int3> vertices = ListPool<Int3>.Claim();
				for (int i = 0; i < chain.Count; i++) vertices.Add(vertexPositions[chain[i]]);
				results(vertices, cycle);
			});
		}

	}
}
