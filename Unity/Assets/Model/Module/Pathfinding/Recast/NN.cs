using PF;

namespace PF
{

	/** Nearest node constraint. Constrains which nodes will be returned by the \link AstarPath.GetNearest GetNearest\endlink function */
	public class NNConstraint {
		/** Graphs treated as valid to search on.
		 * This is a bitmask meaning that bit 0 specifies whether or not the first graph in the graphs list should be able to be included in the search,
		 * bit 1 specifies whether or not the second graph should be included and so on.
		 * \code
		 * // Enables the first and third graphs to be included, but not the rest
		 * myNNConstraint.graphMask = (1 << 0) | (1 << 2);
		 * \endcode
		 * \note This does only affect which nodes are returned from a \link AstarPath.GetNearest GetNearest\endlink call, if a valid graph is connected to an invalid graph using a node link then it might be searched anyway.
		 *
		 * \see #AstarPath.GetNearest
		 * \see #SuitableGraph
		 * \see \ref bitmasks
		 */
		public int graphMask = -1;

		/** Only treat nodes in the area #area as suitable. Does not affect anything if #area is less than 0 (zero) */
		public bool constrainArea;

		/** Area ID to constrain to. Will not affect anything if less than 0 (zero) or if #constrainArea is false */
		public int area = -1;

		/** Constrain the search to only walkable or unwalkable nodes depending on #walkable. */
		public bool constrainWalkability = true;

		/** Only search for walkable or unwalkable nodes if #constrainWalkability is enabled.
		 * If true, only walkable nodes will be searched for, otherwise only unwalkable nodes will be searched for.
		 * Does not affect anything if #constrainWalkability if false.
		 */
		public bool walkable = true;

		/** if available, do an XZ check instead of checking on all axes.
		 * The navmesh/recast graph supports this.
		 *
		 * This can be important on sloped surfaces. See the image below in which the closest point for each blue point is queried for:
		 * \shadowimage{distanceXZ2.png}
		 *
		 * The navmesh/recast graphs also contain a global option for this: \link Pathfinding.NavmeshBase.nearestSearchOnlyXZ nearestSearchOnlyXZ\endlink.
		 */
		public bool distanceXZ;

		/** Sets if tags should be constrained.
		 * \see #tags
		 */
		public bool constrainTags = true;

		/** Nodes which have any of these tags set are suitable.
		 * This is a bitmask, i.e bit 0 indicates that tag 0 is good, bit 3 indicates tag 3 is good etc.
		 * \see #constrainTags
		 * \see #graphMask
		 * \see \ref bitmasks
		 */
		public int tags = -1;

		/** Constrain distance to node.
		 * Uses distance from #AstarPath.maxNearestNodeDistance.
		 * If this is false, it will completely ignore the distance limit.
		 *
		 * If there are no suitable nodes within the distance limit then the search will terminate with a null node as a result.
		 * \note This value is not used in this class, it is used by the AstarPath.GetNearest function.
		 */
		public bool constrainDistance = true;

		/** Returns whether or not the graph conforms to this NNConstraint's rules.
		 * Note that only the first 31 graphs are considered using this function.
		 * If the #graphMask has bit 31 set (i.e the last graph possible to fit in the mask), all graphs
		 * above index 31 will also be considered suitable.
		 */
		public virtual bool SuitableGraph (int graphIndex, NavGraph graph) {
			return ((graphMask >> graphIndex) & 1) != 0;
		}

		/** Returns whether or not the node conforms to this NNConstraint's rules */
		public virtual bool Suitable (GraphNode node) {
			if (constrainWalkability && node.Walkable != walkable) return false;

			if (constrainArea && area >= 0 && node.Area != area) return false;

			if (constrainTags && ((tags >> (int)node.Tag) & 0x1) == 0) return false;

			return true;
		}

		/** The default NNConstraint.
		 * Equivalent to new NNConstraint ().
		 * This NNConstraint has settings which works for most, it only finds walkable nodes
		 * and it constrains distance set by A* Inspector -> Settings -> Max Nearest Node Distance */
		public static NNConstraint Default {
			get {
				return new NNConstraint();
			}
		}

		/** Returns a constraint which does not filter the results */
		public static NNConstraint None {
			get {
				return new NNConstraint {
						   constrainWalkability = false,
						   constrainArea = false,
						   constrainTags = false,
						   constrainDistance = false,
						   graphMask = -1,
				};
			}
		}

		/** Default constructor. Equals to the property #Default */
		public NNConstraint () {
		}
	}

	/** A special NNConstraint which can use different logic for the start node and end node in a path.
	 * A PathNNConstraint can be assigned to the Path.nnConstraint field, the path will first search for the start node, then it will call #SetStart and proceed with searching for the end node (nodes in the case of a MultiTargetPath).\n
	 * The default PathNNConstraint will constrain the end point to lie inside the same area as the start point.
	 */
	public class PathNNConstraint : NNConstraint {
		public static new PathNNConstraint Default {
			get {
				return new PathNNConstraint {
						   constrainArea = true
				};
			}
		}

		/** Called after the start node has been found. This is used to get different search logic for the start and end nodes in a path */
		public virtual void SetStart (GraphNode node) {
			if (node != null) {
				area = (int)node.Area;
			} else {
				constrainArea = false;
			}
		}
	}

	/** Internal result of a nearest node query.
	 * \see NNInfo
	 */
	public struct NNInfoInternal {
		/** Closest node found.
		 * This node is not necessarily accepted by any NNConstraint passed.
		 * \see constrainedNode
		 */
		public GraphNode node;

		/** Optional to be filled in.
		 * If the search will be able to find the constrained node without any extra effort it can fill it in. */
		public GraphNode constrainedNode;

		/** The position clamped to the closest point on the #node.
		 */
		public Vector3 clampedPosition;

		/** Clamped position for the optional constrainedNode */
		public Vector3 constClampedPosition;

		public NNInfoInternal (GraphNode node) {
			this.node = node;
			constrainedNode = null;
			clampedPosition = Vector3.zero;
			constClampedPosition = Vector3.zero;

			UpdateInfo();
		}

		/** Updates #clampedPosition and #constClampedPosition from node positions */
		public void UpdateInfo () {
			clampedPosition = node != null ? (Vector3)node.position : Vector3.zero;
			constClampedPosition = constrainedNode != null ? (Vector3)constrainedNode.position : Vector3.zero;
		}
	}

	/** Result of a nearest node query */
	public struct NNInfo {
		/** Closest node */
		public readonly GraphNode node;

		/** Closest point on the navmesh.
		 * This is the query position clamped to the closest point on the #node.
		 */
		public readonly Vector3 position;

		/** Closest point on the navmesh.
		 * \deprecated This field has been renamed to #position
		 */
		[System.Obsolete("This field has been renamed to 'position'")]
		public Vector3 clampedPosition {
			get {
				return position;
			}
		}

		public NNInfo (NNInfoInternal internalInfo) {
			node = internalInfo.node;
			position = internalInfo.clampedPosition;
		}

		public static explicit operator Vector3 (NNInfo ob) {
			return ob.position;
		}

		public static explicit operator GraphNode (NNInfo ob) {
			return ob.node;
		}
	}
}