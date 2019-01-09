using PF;
using UnityEngine;
using Mathf = UnityEngine.Mathf;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Pathfinding {
	[AddComponentMenu("Pathfinding/GraphUpdateScene")]
	/** Helper class for easily updating graphs.
	 *
	 * The GraphUpdateScene component is really easy to use. Create a new empty GameObject and add the component to it, it can be found in Components-->Pathfinding-->GraphUpdateScene.\n
	 * When you have added the component, you should see something like the image below.
	 * \shadowimage{graphUpdateScene.png}
	 * The region which the component will affect is defined by creating a polygon in the scene.
	 * If you make sure you have the Position tool enabled (top-left corner of the Unity window) you can shift+click in the scene view to add more points to the polygon.
	 * You can remove points using shift+alt+click.
	 * By clicking on the points you can bring up a positioning tool. You can also open the "points" array in the inspector to set each point's coordinates manually.
	 * \shadowimage{graphUpdateScenePoly.png}
	 * In the inspector there are a number of variables. The first one is named "Convex", it sets if the convex hull of the points should be calculated or if the polygon should be used as-is.
	 * Using the convex hull is faster when applying the changes to the graph, but with a non-convex polygon you can specify more complicated areas.\n
	 * The next two variables, called "Apply On Start" and "Apply On Scan" determine when to apply the changes. If the object is in the scene from the beginning, both can be left on, it doesn't
	 * matter since the graph is also scanned at start. However if you instantiate it later in the game, you can make it apply it's setting directly, or wait until the next scan (if any).
	 * If the graph is rescanned, all GraphUpdateScene components which have the Apply On Scan variable toggled will apply their settings again to the graph since rescanning clears all previous changes.\n
	 * You can also make it apply it's changes using scripting.
	 * \code GetComponent<GraphUpdateScene>().Apply (); \endcode
	 * The above code will make it apply its changes to the graph (assuming a GraphUpdateScene component is attached to the same GameObject).
	 *
	 * Next there is "Modify Walkability" and "Set Walkability" (which appears when "Modify Walkability" is toggled).
	 * If Modify Walkability is set, then all nodes inside the area will either be set to walkable or unwalkable depending on the value of the "Set Walkability" variable.
	 *
	 * Penalty can also be applied to the nodes. A higher penalty (aka weight) makes the nodes harder to traverse so it will try to avoid those areas.
	 *
	 * The tagging variables can be read more about on this page: \ref tags "Working with tags".
	 *
	 * \note The Y (up) axis of the transform that this component is attached to should be in the same direction as the up direction of the graph.
	 * So if you for example have a grid in the XY plane then the transform should have the rotation (-90,0,0).
	 */
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_graph_update_scene.php")]
	public class GraphUpdateScene : GraphModifier {
		/** Points which define the region to update */
		public PF.Vector3[] points;

		/** Private cached convex hull of the #points */
		private PF.Vector3[] convexPoints;

		/** Use the convex hull of the points instead of the original polygon.
		 *
		 * \see https://en.wikipedia.org/wiki/Convex_hull
		 */
		public bool convex = true;

		/** Minumum height of the bounds of the resulting Graph Update Object.
		 * Useful when all points are laid out on a plane but you still need a bounds with a height greater than zero since a
		 * zero height graph update object would usually result in no nodes being updated.
		 */
		public float minBoundsHeight = 1;

		/** Penalty to add to nodes.
		 * Usually you need quite large values, at least 1000-10000. A higher penalty means that agents will try to avoid those nodes more.
		 *
		 * Be careful when setting negative values since if a node gets a negative penalty it will underflow and instead get
		 * really large. In most cases a warning will be logged if that happens.
		 *
		 * \see \ref tags for another way of applying penalties.
		 */
		public int penaltyDelta;

		/** If true, then all affected nodes will be made walkable or unwalkable according to #setWalkability */
		public bool modifyWalkability;

		/** Nodes will be made walkable or unwalkable according to this value if #modifyWalkability is true */
		public bool setWalkability;

		/** Apply this graph update object on start */
		public bool applyOnStart = true;

		/** Apply this graph update object whenever a graph is rescanned */
		public bool applyOnScan = true;

		/** Update node's walkability and connectivity using physics functions.
		 * For grid graphs, this will update the node's position and walkability exactly like when doing a scan of the graph.
		 * If enabled for grid graphs, #modifyWalkability will be ignored.
		 *
		 * For Point Graphs, this will recalculate all connections which passes through the bounds of the resulting Graph Update Object
		 * using raycasts (if enabled).
		 *
		 */
		public bool updatePhysics;

		/** \copydoc Pathfinding::GraphUpdateObject::resetPenaltyOnPhysics */
		public bool resetPenaltyOnPhysics = true;

		/** \copydoc Pathfinding::GraphUpdateObject::updateErosion */
		public bool updateErosion = true;

		/** Should the tags of the nodes be modified.
		 * If enabled, set all nodes' tags to #setTag
		 */
		public bool modifyTag;

		/** If #modifyTag is enabled, set all nodes' tags to this value */
		public int setTag;

		/** Emulates behavior from before version 4.0 */
		[HideInInspector]
		public bool legacyMode = false;

		/** Private cached inversion of #setTag.
		 * Used for InvertSettings() */
		private int setTagInvert;

		/** Has apply been called yet.
		 * Used to prevent applying twice when both applyOnScan and applyOnStart are enabled */
		private bool firstApplied;

		[SerializeField]
		private int serializedVersion = 0;

		/** Use world space for coordinates.
		 * If true, the shape will not follow when moving around the transform.
		 *
		 * \see #ToggleUseWorldSpace
		 */
		[SerializeField]
		[UnityEngine.Serialization.FormerlySerializedAs("useWorldSpace")]
		private bool legacyUseWorldSpace;

		/** Do some stuff at start */
		public void Start () {
			if (!Application.isPlaying) return;

			// If firstApplied is true, that means the graph was scanned during Awake.
			// So we shouldn't apply it again because then we would end up applying it two times
			if (!firstApplied && applyOnStart) {
				Apply();
			}
		}

		public override void OnPostScan () {
			if (applyOnScan) Apply();
		}

		/** Inverts all invertable settings for this GUS.
		 * Namely: penalty delta, walkability, tags.
		 *
		 * Penalty delta will be changed to negative penalty delta.\n
		 * #setWalkability will be inverted.\n
		 * #setTag will be stored in a private variable, and the new value will be 0. When calling this function again, the saved
		 * value will be the new value.
		 *
		 * Calling this function an even number of times without changing any settings in between will be identical to no change in settings.
		 */
		public virtual void InvertSettings () {
			setWalkability = !setWalkability;
			penaltyDelta = -penaltyDelta;
			if (setTagInvert == 0) {
				setTagInvert = setTag;
				setTag = 0;
			} else {
				setTag = setTagInvert;
				setTagInvert = 0;
			}
		}

		/** Recalculate convex hull.
		 * Will not do anything if #convex is disabled.
		 */
		public void RecalcConvex () {
			convexPoints = convex ? Polygon.ConvexHullXZ(points) : null;
		}

		/** Switches between using world space and using local space.
		 * \deprecated World space can no longer be used as it does not work well with rotated graphs. Use transform.InverseTransformPoint to transform points to local space.
		 */
		[System.ObsoleteAttribute("World space can no longer be used as it does not work well with rotated graphs. Use transform.InverseTransformPoint to transform points to local space.", true)]
		void ToggleUseWorldSpace () {
		}

		/** Lock all points to a specific Y value.
		 * \deprecated The Y coordinate is no longer important. Use the position of the object instead.
		 */
		[System.ObsoleteAttribute("The Y coordinate is no longer important. Use the position of the object instead", true)]
		public void LockToY () {
		}

		/** Calculates the bounds for this component.
		 * This is a relatively expensive operation, it needs to go through all points and
		 * run matrix multiplications.
		 */
		public Bounds GetBounds () {
			if (points == null || points.Length == 0) {
				Bounds bounds;
				var coll = GetComponent<Collider>();
				var coll2D = GetComponent<Collider2D>();
				var rend = GetComponent<Renderer>();

				if (coll != null) bounds = coll.bounds;
				else if (coll2D != null) {
					bounds = coll2D.bounds;
					bounds.size = new Vector3(bounds.size.x, bounds.size.y, Mathf.Max(bounds.size.z, 1f));
				} else if (rend != null) {
					bounds = rend.bounds;
				} else {
					return new Bounds(Vector3.zero, Vector3.zero);
				}

				if (legacyMode && bounds.size.y < minBoundsHeight) bounds.size = new Vector3(bounds.size.x, minBoundsHeight, bounds.size.z);
				return bounds;
			} else {
				return GraphUpdateShape.GetBounds(convex ? convexPoints : points, legacyMode && legacyUseWorldSpace ? Matrix4x4.identity : transform.localToWorldMatrix, minBoundsHeight);
			}
		}

		/** Updates graphs with a created GUO.
		 * Creates a Pathfinding.GraphUpdateObject with a Pathfinding.GraphUpdateShape
		 * representing the polygon of this object and update all graphs using AstarPath.UpdateGraphs.
		 * This will not update graphs immediately. See AstarPath.UpdateGraph for more info.
		 */
		public void Apply () {
			if (AstarPath.active == null) {
				Debug.LogError("There is no AstarPath object in the scene", this);
				return;
			}

			GraphUpdateObject guo;

			if (points == null || points.Length == 0) {
				var polygonCollider = GetComponent<PolygonCollider2D>();
				if (polygonCollider != null) {
					var points2D = polygonCollider.points;
					Vector3[] pts = new Vector3[points2D.Length];
					for (int i = 0; i < pts.Length; i++) {
						var p = points2D[i] + polygonCollider.offset;
						pts[i] = new Vector3(p.x, 0, p.y);
					}

					var mat = transform.localToWorldMatrix * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 0, 0), Vector3.one);
					var shape = new GraphUpdateShape(points, convex, mat, minBoundsHeight);
					guo = new GraphUpdateObject(GetBounds());
					guo.shape = shape;
				} else {
					var bounds = GetBounds();
					if (bounds.center == Vector3.zero && bounds.size == Vector3.zero) {
						Debug.LogError("Cannot apply GraphUpdateScene, no points defined and no renderer or collider attached", this);
						return;
					}

					guo = new GraphUpdateObject(bounds);
				}
			} else {
				GraphUpdateShape shape;
				if (legacyMode && !legacyUseWorldSpace) {
					// Used for compatibility with older versions
					var worldPoints = new PF.Vector3[points.Length];
					for (int i = 0; i < points.Length; i++) worldPoints[i] = transform.TransformPoint(points[i]);
					shape = new GraphUpdateShape(worldPoints, convex, Matrix4x4.identity, minBoundsHeight);
				} else {
					shape = new GraphUpdateShape(points, convex, legacyMode && legacyUseWorldSpace ? Matrix4x4.identity : transform.localToWorldMatrix, minBoundsHeight);
				}
				var bounds = shape.GetBounds();
				guo = new GraphUpdateObject(bounds);
				guo.shape = shape;
			}

			firstApplied = true;

			guo.modifyWalkability = modifyWalkability;
			guo.setWalkability = setWalkability;
			guo.addPenalty = penaltyDelta;
			guo.updatePhysics = updatePhysics;
			guo.updateErosion = updateErosion;
			guo.resetPenaltyOnPhysics = resetPenaltyOnPhysics;

			guo.modifyTag = modifyTag;
			guo.setTag = setTag;
		}

		/** Draws some gizmos */
		void OnDrawGizmos () {
			OnDrawGizmos(false);
		}

		/** Draws some gizmos */
		void OnDrawGizmosSelected () {
			OnDrawGizmos(true);
		}

		/** Draws some gizmos */
		void OnDrawGizmos (bool selected) {
			Color c = selected ? new Color(227/255f, 61/255f, 22/255f, 1.0f) : new Color(227/255f, 61/255f, 22/255f, 0.9f);

			if (selected) {
				Gizmos.color = Color.Lerp(c, new Color(1, 1, 1, 0.2f), 0.9f);

				Bounds b = GetBounds();
				Gizmos.DrawCube(b.center, b.size);
				Gizmos.DrawWireCube(b.center, b.size);
			}

			if (points == null) return;

			if (convex) c.a *= 0.5f;

			Gizmos.color = c;

			Matrix4x4 matrix = legacyMode && legacyUseWorldSpace ? Matrix4x4.identity : transform.localToWorldMatrix;

			if (convex) {
				c.r -= 0.1f;
				c.g -= 0.2f;
				c.b -= 0.1f;

				Gizmos.color = c;
			}

			if (selected || !convex) {
				for (int i = 0; i < points.Length; i++) {
					Gizmos.DrawLine(matrix.MultiplyPoint3x4(points[i]), matrix.MultiplyPoint3x4(points[(i+1)%points.Length]));
				}
			}

			if (convex) {
				if (convexPoints == null) RecalcConvex();

				Gizmos.color = selected ? new Color(227/255f, 61/255f, 22/255f, 1.0f) : new Color(227/255f, 61/255f, 22/255f, 0.9f);

				for (int i = 0; i < convexPoints.Length; i++) {
					Gizmos.DrawLine(matrix.MultiplyPoint3x4(convexPoints[i]), matrix.MultiplyPoint3x4(convexPoints[(i+1)%convexPoints.Length]));
				}
			}

			// Draw the full 3D shape
			var pts = convex ? convexPoints : points;
			if (selected && pts != null && pts.Length > 0) {
				Gizmos.color = new Color(1, 1, 1, 0.2f);
				float miny = pts[0].y, maxy = pts[0].y;
				for (int i = 0; i < pts.Length; i++) {
					miny = Mathf.Min(miny, pts[i].y);
					maxy = Mathf.Max(maxy, pts[i].y);
				}
				var extraHeight = Mathf.Max(minBoundsHeight - (maxy - miny), 0) * 0.5f;
				miny -= extraHeight;
				maxy += extraHeight;

				for (int i = 0; i < pts.Length; i++) {
					var next = (i+1) % pts.Length;
					var p1 = matrix.MultiplyPoint3x4(pts[i] + PF.Vector3.up*(miny - pts[i].y));
					var p2 = matrix.MultiplyPoint3x4(pts[i] + PF.Vector3.up*(maxy - pts[i].y));
					var p1n = matrix.MultiplyPoint3x4(pts[next] + PF.Vector3.up*(miny - pts[next].y));
					var p2n = matrix.MultiplyPoint3x4(pts[next] + PF.Vector3.up*(maxy - pts[next].y));
					Gizmos.DrawLine(p1, p2);
					Gizmos.DrawLine(p1, p1n);
					Gizmos.DrawLine(p2, p2n);
				}
			}
		}

		/** Disables legacy mode if it is enabled.
		 * Legacy mode is automatically enabled for components when upgrading from an earlier version than 3.8.6.
		 */
		public void DisableLegacyMode () {
			if (legacyMode) {
				legacyMode = false;
				if (legacyUseWorldSpace) {
					legacyUseWorldSpace = false;
					for (int i = 0; i < points.Length; i++) {
						points[i] = transform.InverseTransformPoint(points[i]);
					}
					RecalcConvex();
				}
			}
		}

		protected override void Awake () {
			if (serializedVersion == 0) {
				// Use the old behavior if some points are already set
				if (points != null && points.Length > 0) legacyMode = true;
				serializedVersion = 1;
			}
			base.Awake();
		}
	}
}
