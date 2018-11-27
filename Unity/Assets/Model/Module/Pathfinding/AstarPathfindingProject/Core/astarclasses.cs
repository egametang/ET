using UnityEngine;
using System.Collections.Generic;
using PF;
using Mathf = UnityEngine.Mathf;

// Empty namespace declaration to avoid errors in the free version
// Which does not have any classes in the RVO namespace
namespace Pathfinding.RVO {}

namespace Pathfinding {
	using Pathfinding.Util;

#if UNITY_5_0
	/** Used in Unity 5.0 since the HelpURLAttribute was first added in Unity 5.1 */
	public class HelpURLAttribute : Attribute {
	}
#endif

	[System.Serializable]
	/** Stores editor colors */
	public class AstarColor {
		public Color _NodeConnection;
		public Color _UnwalkableNode;
		public Color _BoundsHandles;

		public Color _ConnectionLowLerp;
		public Color _ConnectionHighLerp;

		public Color _MeshEdgeColor;

		/** Holds user set area colors.
		 * Use GetAreaColor to get an area color */
		public Color[] _AreaColors;

		public static Color NodeConnection = new Color(1, 1, 1, 0.9F);
		public static Color UnwalkableNode = new Color(1, 0, 0, 0.5F);
		public static Color BoundsHandles = new Color(0.29F, 0.454F, 0.741F, 0.9F);

		public static Color ConnectionLowLerp = new Color(0, 1, 0, 0.5F);
		public static Color ConnectionHighLerp = new Color(1, 0, 0, 0.5F);

		public static Color MeshEdgeColor = new Color(0, 0, 0, 0.5F);

		/** Holds user set area colors.
		 * Use GetAreaColor to get an area color */
		private static Color[] AreaColors;

		/** Returns an color for an area, uses both user set ones and calculated.
		 * If the user has set a color for the area, it is used, but otherwise the color is calculated using Mathfx.IntToColor
		 * \see #AreaColors */
		public static Color GetAreaColor (uint area) {
			if (AreaColors == null || area >= AreaColors.Length) {
				return UnityHelper.IntToColor((int)area, 1F);
			}
			return AreaColors[(int)area];
		}

		/** Pushes all local variables out to static ones.
		 * This is done because that makes it so much easier to access the colors during Gizmo rendering
		 * and it has a positive performance impact as well (gizmo rendering is hot code).
		 */
		public void OnEnable () {
			NodeConnection = _NodeConnection;
			UnwalkableNode = _UnwalkableNode;
			BoundsHandles = _BoundsHandles;
			ConnectionLowLerp = _ConnectionLowLerp;
			ConnectionHighLerp = _ConnectionHighLerp;
			MeshEdgeColor = _MeshEdgeColor;
			AreaColors = _AreaColors;
		}

		public AstarColor () {
			// Set default colors
			_NodeConnection = new Color(1, 1, 1, 0.9F);
			_UnwalkableNode = new Color(1, 0, 0, 0.5F);
			_BoundsHandles = new Color(0.29F, 0.454F, 0.741F, 0.9F);
			_ConnectionLowLerp = new Color(0, 1, 0, 0.5F);
			_ConnectionHighLerp = new Color(1, 0, 0, 0.5F);
			_MeshEdgeColor = new Color(0, 0, 0, 0.5F);
		}
	}

	/** Progress info for e.g a progressbar.
	 * Used by the scan functions in the project
	 * \see #AstarPath.ScanAsync
	 */
	public struct Progress {
		/** Current progress as a value between 0 and 1 */
		public readonly float progress;
		/** Description of what is currently being done */
		public readonly string description;

		public Progress (float progress, string description) {
			this.progress = progress;
			this.description = description;
		}

		public Progress MapTo (float min, float max, string prefix = null) {
			return new Progress(Mathf.Lerp(min, max, progress), prefix + description);
		}

		public override string ToString () {
			return progress.ToString("0.0") + " " + description;
		}
	}

	/** Represents a collection of settings used to update nodes in a specific region of a graph.
	 * \see AstarPath.UpdateGraphs
	 * \see \ref graph-updates
	 */
	public class GraphUpdateObject {
		/** The bounds to update nodes within.
		 * Defined in world space.
		 */
		public Bounds bounds;

		/** Controlls if a flood fill will be carried out after this GUO has been applied.
		 * Disabling this can be used to gain a performance boost, but use with care.
		 * If you are sure that a GUO will not modify walkability or connections. You can set this to false.
		 * For example when only updating penalty values it can save processing power when setting this to false. Especially on large graphs.
		 * \note If you set this to false, even though it does change e.g walkability, it can lead to paths returning that they failed even though there is a path,
		 * or the try to search the whole graph for a path even though there is none, and will in the processes use wast amounts of processing power.
		 *
		 * If using the basic GraphUpdateObject (not a derived class), a quick way to check if it is going to need a flood fill is to check if #modifyWalkability is true or #updatePhysics is true.
		 *
		 */
		public bool requiresFloodFill = true;

		/** Use physics checks to update nodes.
		 * When updating a grid graph and this is true, the nodes' position and walkability will be updated using physics checks
		 * with settings from "Collision Testing" and "Height Testing".
		 *
		 * When updating a PointGraph, setting this to true will make it re-evaluate all connections in the graph which passes through the #bounds.
		 * This has no effect when updating GridGraphs if #modifyWalkability is turned on.
		 *
		 * On RecastGraphs, having this enabled will trigger a complete recalculation of all tiles intersecting the bounds.
		 * This is quite slow (but powerful). If you only want to update e.g penalty on existing nodes, leave it disabled.
		 */
		public bool updatePhysics = true;

		/** Reset penalties to their initial values when updating grid graphs and #updatePhysics is true.
		 * If you want to keep old penalties even when you update the graph you may want to disable this option.
		 *
		 * The images below shows two overlapping graph update objects, the right one happened to be applied before the left one. They both have updatePhysics = true and are
		 * set to increase the penalty of the nodes by some amount.
		 *
		 * The first image shows the result when resetPenaltyOnPhysics is false. Both penalties are added correctly.
		 * \shadowimage{resetPenaltyOnPhysics_False.png}
		 *
		 * This second image shows when resetPenaltyOnPhysics is set to true. The first GUO is applied correctly, but then the second one (the left one) is applied
		 * and during its updating, it resets the penalties first and then adds penalty to the nodes. The result is that the penalties from both GUOs are not added together.
		 * The green patch in at the border is there because physics recalculation (recalculation of the position of the node, checking for obstacles etc.) affects a slightly larger
		 * area than the original GUO bounds because of the Grid Graph -> Collision Testing -> Diameter setting (it is enlarged by that value). So some extra nodes have their penalties reset.
		 *
		 * \shadowimage{resetPenaltyOnPhysics_True.png}
		 */
		public bool resetPenaltyOnPhysics = true;

		/** Update Erosion for GridGraphs.
		 * When enabled, erosion will be recalculated for grid graphs
		 * after the GUO has been applied.
		 *
		 * In the below image you can see the different effects you can get with the different values.\n
		 * The first image shows the graph when no GUO has been applied. The blue box is not identified as an obstacle by the graph, the reason
		 * there are unwalkable nodes around it is because there is a height difference (nodes are placed on top of the box) so erosion will be applied (an erosion value of 2 is used in this graph).
		 * The orange box is identified as an obstacle, so the area of unwalkable nodes around it is a bit larger since both erosion and collision has made
		 * nodes unwalkable.\n
		 * The GUO used simply sets walkability to true, i.e making all nodes walkable.
		 *
		 * \shadowimage{updateErosion.png}
		 *
		 * When updateErosion=True, the reason the blue box still has unwalkable nodes around it is because there is still a height difference
		 * so erosion will still be applied. The orange box on the other hand has no height difference and all nodes are set to walkable.\n
		 * \n
		 * When updateErosion=False, all nodes walkability are simply set to be walkable in this example.
		 *
		 * \see Pathfinding.GridGraph
		 */
		public bool updateErosion = true;

		/** NNConstraint to use.
		 * The Pathfinding.NNConstraint.SuitableGraph function will be called on the NNConstraint to enable filtering of which graphs to update.\n
		 * \note As the Pathfinding.NNConstraint.SuitableGraph function is A* Pathfinding Project Pro only, this variable doesn't really affect anything in the free version.
		 *
		 *
		 * \astarpro */
		public NNConstraint nnConstraint = NNConstraint.None;

		/** Penalty to add to the nodes.
		 * A penalty of 1000 is equivalent to the cost of moving 1 world unit.
		 */
		public int addPenalty;

		/** If true, all nodes' \a walkable variable will be set to #setWalkability */
		public bool modifyWalkability;

		/** If #modifyWalkability is true, the nodes' \a walkable variable will be set to this value */
		public bool setWalkability;

		/** If true, all nodes' \a tag will be set to #setTag */
		public bool modifyTag;

		/** If #modifyTag is true, all nodes' \a tag will be set to this value */
		public int setTag;

		/** Track which nodes are changed and save backup data.
		 * Used internally to revert changes if needed.
		 */
		public bool trackChangedNodes;

		/** Nodes which were updated by this GraphUpdateObject.
		 * Will only be filled if #trackChangedNodes is true.
		 * \note It might take a few frames for graph update objects to be applied.
		 * If you need this info immediately, use #AstarPath.FlushGraphUpdates.
		 */
		public List<GraphNode> changedNodes;
		private List<uint> backupData;
		private List<Int3> backupPositionData;

		/** A shape can be specified if a bounds object does not give enough precision.
		 * Note that if you set this, you should set the bounds so that it encloses the shape
		 * because the bounds will be used as an initial fast check for which nodes that should
		 * be updated.
		 */
		public GraphUpdateShape shape;

		/** Should be called on every node which is updated with this GUO before it is updated.
		 * \param node The node to save fields for. If null, nothing will be done
		 * \see #trackChangedNodes
		 */
		public virtual void WillUpdateNode (GraphNode node) {
			if (trackChangedNodes && node != null) {
				if (changedNodes == null) { changedNodes = ListPool<GraphNode>.Claim(); backupData = ListPool<uint>.Claim(); backupPositionData = ListPool<Int3>.Claim(); }
				changedNodes.Add(node);
				backupPositionData.Add(node.position);
				backupData.Add(node.Penalty);
				backupData.Add(node.Flags);
			}
		}

		/** Reverts penalties and flags (which includes walkability) on every node which was updated using this GUO.
		 * Data for reversion is only saved if #trackChangedNodes is true.
		 *
		 * \note Not all data is saved. The saved data includes: penalties, walkability, tags, area, position and for grid graphs (not layered) it also includes connection data.
		 */
		public virtual void RevertFromBackup () {
			if (trackChangedNodes) {
				if (changedNodes == null) return;

				int counter = 0;
				for (int i = 0; i < changedNodes.Count; i++) {
					changedNodes[i].Penalty = backupData[counter];
					counter++;
					changedNodes[i].Flags = backupData[counter];
					counter++;
					changedNodes[i].position = backupPositionData[i];
				}

				ListPool<GraphNode>.Release(ref changedNodes);
				ListPool<uint>.Release(ref backupData);
				ListPool<Int3>.Release(ref backupPositionData);
			} else {
				throw new System.InvalidOperationException("Changed nodes have not been tracked, cannot revert from backup. Please set trackChangedNodes to true before applying the update.");
			}
		}

		/** Updates the specified node using this GUO's settings */
		public virtual void Apply (GraphNode node) {
			if (shape == null || shape.Contains(node)) {
				//Update penalty and walkability
				node.Penalty = (uint)(node.Penalty+addPenalty);
				if (modifyWalkability) {
					node.Walkable = setWalkability;
				}

				//Update tags
				if (modifyTag) node.Tag = (uint)setTag;
			}
		}

		public GraphUpdateObject () {
		}

		/** Creates a new GUO with the specified bounds */
		public GraphUpdateObject (Bounds b) {
			bounds = b;
		}
	}

	
	public delegate void OnGraphDelegate (NavGraph graph);

	public delegate void OnScanDelegate (AstarPath script);

	/** \deprecated */
	public delegate void OnScanStatus (Progress progress);
}
