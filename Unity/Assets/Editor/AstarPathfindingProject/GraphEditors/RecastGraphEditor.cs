using PF;
using UnityEngine;
using UnityEditor;
using Mathf = UnityEngine.Mathf;
using Vector3 = UnityEngine.Vector3;

namespace Pathfinding {
	/**
	 * Editor for the RecastGraph.
	 * \astarpro
	 */
	[CustomGraphEditor(typeof(RecastGraph), "Recast Graph")]
	public class RecastGraphEditor : GraphEditor {
		public static bool tagMaskFoldout;

		public enum UseTiles {
			UseTiles = 0,
			DontUseTiles = 1
		}

		public override void OnInspectorGUI (NavGraph target) {
			var graph = target as RecastGraph;

			bool preEnabled = GUI.enabled;

			System.Int64 estWidth = Mathf.RoundToInt(Mathf.Ceil(graph.forcedBoundsSize.x / graph.cellSize));
			System.Int64 estDepth = Mathf.RoundToInt(Mathf.Ceil(graph.forcedBoundsSize.z / graph.cellSize));

			// Show a warning if the number of voxels is too large
			if (estWidth*estDepth >= 1024*1024 || estDepth >= 1024*1024 || estWidth >= 1024*1024) {
				GUIStyle helpBox = GUI.skin.FindStyle("HelpBox") ?? GUI.skin.FindStyle("Box");

				Color preColor = GUI.color;
				if (estWidth*estDepth >= 2048*2048 || estDepth >= 2048*2048 || estWidth >= 2048*2048) {
					GUI.color = Color.red;
				} else {
					GUI.color = Color.yellow;
				}

				GUILayout.Label("Warning : Might take some time to calculate", helpBox);
				GUI.color = preColor;
			}

			GUI.enabled = false;
			EditorGUILayout.LabelField(new GUIContent("Width (voxels)", "Based on the cell size and the bounding box"), new GUIContent(estWidth.ToString()));

			EditorGUILayout.LabelField(new GUIContent("Depth (voxels)", "Based on the cell size and the bounding box"), new GUIContent(estDepth.ToString()));
			GUI.enabled = preEnabled;

			graph.cellSize = EditorGUILayout.FloatField(new GUIContent("Cell Size", "Size of one voxel in world units"), graph.cellSize);
			if (graph.cellSize < 0.001F) graph.cellSize = 0.001F;

			graph.useTiles = (UseTiles)EditorGUILayout.EnumPopup("Use Tiles", graph.useTiles ? UseTiles.UseTiles : UseTiles.DontUseTiles) == UseTiles.UseTiles;

			if (graph.useTiles) {
				EditorGUI.indentLevel++;
				graph.editorTileSize = EditorGUILayout.IntField(new GUIContent("Tile Size", "Size in voxels of a single tile.\n" +
						"This is the width of the tile.\n" +
						"\n" +
						"A large tile size can be faster to initially scan (but beware of out of memory issues if you try with a too large tile size in a large world)\n" +
						"smaller tile sizes are (much) faster to update.\n" +
						"\n" +
						"Different tile sizes can affect the quality of paths. It is often good to split up huge open areas into several tiles for\n" +
						"better quality paths, but too small tiles can lead to effects looking like invisible obstacles."), graph.editorTileSize);
				EditorGUI.indentLevel--;
			}

			graph.minRegionSize = EditorGUILayout.FloatField(new GUIContent("Min Region Size", "Small regions will be removed. In square world units"), graph.minRegionSize);

			graph.walkableHeight = EditorGUILayout.FloatField(new GUIContent("Walkable Height", "Minimum distance to the roof for an area to be walkable"), graph.walkableHeight);
			graph.walkableHeight = Mathf.Max(graph.walkableHeight, 0);

			graph.walkableClimb = EditorGUILayout.FloatField(new GUIContent("Walkable Climb", "How high can the character climb"), graph.walkableClimb);

			// A walkableClimb higher than this can cause issues when generating the navmesh since then it can in some cases
			// Both be valid for a character to walk under an obstacle and climb up on top of it (and that cannot be handled with a navmesh without links)
			if (graph.walkableClimb >= graph.walkableHeight) {
				graph.walkableClimb = graph.walkableHeight;
				EditorGUILayout.HelpBox("Walkable climb should be less than walkable height. Clamping to " + graph.walkableHeight+".", MessageType.Warning);
			} else if (graph.walkableClimb < 0) {
				graph.walkableClimb = 0;
			}

			graph.characterRadius = EditorGUILayout.FloatField(new GUIContent("Character Radius", "Radius of the character. It's good to add some margin.\nIn world units."), graph.characterRadius);
			graph.characterRadius = Mathf.Max(graph.characterRadius, 0);

			if (graph.characterRadius < graph.cellSize * 2) {
				EditorGUILayout.HelpBox("For best navmesh quality, it is recommended to keep the character radius at least 2 times as large as the cell size. Smaller cell sizes will give you higher quality navmeshes, but it will take more time to scan the graph.", MessageType.Warning);
			}

			graph.maxSlope = EditorGUILayout.Slider(new GUIContent("Max Slope", "Approximate maximum slope"), graph.maxSlope, 0F, 90F);
			graph.maxEdgeLength = EditorGUILayout.FloatField(new GUIContent("Max Border Edge Length", "Maximum length of one border edge in the completed navmesh before it is split. A lower value can often yield better quality graphs, but don't use so low values so that you get a lot of thin triangles."), graph.maxEdgeLength);
			graph.maxEdgeLength = graph.maxEdgeLength < graph.cellSize ? graph.cellSize : graph.maxEdgeLength;

			graph.contourMaxError = EditorGUILayout.FloatField(new GUIContent("Max Edge Error", "Amount of simplification to apply to edges.\nIn world units."), graph.contourMaxError);

			graph.rasterizeTerrain = EditorGUILayout.Toggle(new GUIContent("Rasterize Terrain", "Should a rasterized terrain be included"), graph.rasterizeTerrain);
			if (graph.rasterizeTerrain) {
				EditorGUI.indentLevel++;
				graph.rasterizeTrees = EditorGUILayout.Toggle(new GUIContent("Rasterize Trees", "Rasterize tree colliders on terrains. " +
						"If the tree prefab has a collider, that collider will be rasterized. " +
						"Otherwise a simple box collider will be used and the script will " +
						"try to adjust it to the tree's scale, it might not do a very good job though so " +
						"an attached collider is preferable."), graph.rasterizeTrees);
				if (graph.rasterizeTrees) {
					EditorGUI.indentLevel++;
					graph.colliderRasterizeDetail = EditorGUILayout.FloatField(new GUIContent("Collider Detail", "Controls the detail of the generated collider meshes. "+
							"Increasing does not necessarily yield better navmeshes, but lowering will speed up scan.\n"+
							"Spheres and capsule colliders will be converted to meshes in order to be able to rasterize them, a higher value will increase the number of triangles in those meshes."), graph.colliderRasterizeDetail);
					EditorGUI.indentLevel--;
				}

				graph.terrainSampleSize = EditorGUILayout.IntField(new GUIContent("Terrain Sample Size", "Size of terrain samples. A lower value is better, but slower"), graph.terrainSampleSize);
				graph.terrainSampleSize = graph.terrainSampleSize < 1 ? 1 : graph.terrainSampleSize;//Clamp to at least 1
				EditorGUI.indentLevel--;
			}

			graph.rasterizeMeshes = EditorGUILayout.Toggle(new GUIContent("Rasterize Meshes", "Should meshes be rasterized and used for building the navmesh"), graph.rasterizeMeshes);
			graph.rasterizeColliders = EditorGUILayout.Toggle(new GUIContent("Rasterize Colliders", "Should colliders be rasterized and used for building the navmesh"), graph.rasterizeColliders);
			if (graph.rasterizeColliders) {
				EditorGUI.indentLevel++;
				graph.colliderRasterizeDetail = EditorGUILayout.FloatField(new GUIContent("Collider Detail", "Controls the detail of the generated collider meshes. "+
						"Increasing does not necessarily yield better navmeshes, but lowering will speed up scan.\n"+
						"Spheres and capsule colliders will be converted to meshes in order to be able to rasterize them, a higher value will increase the number of triangles in those meshes."), graph.colliderRasterizeDetail);
				EditorGUI.indentLevel--;
			}

			if (graph.rasterizeMeshes && graph.rasterizeColliders) {
				EditorGUILayout.HelpBox("You are rasterizing both meshes and colliders, this might just be duplicating the work that is done if the colliders and meshes are similar in shape. You can use the RecastMeshObj component" +
					" to always include some specific objects regardless of what the above settings are set to.", MessageType.Info);
			}

			Separator();

			graph.forcedBoundsCenter = EditorGUILayout.Vector3Field("Center", graph.forcedBoundsCenter);
			graph.forcedBoundsSize = EditorGUILayout.Vector3Field("Size", graph.forcedBoundsSize);
			// Make sure the bounding box is not infinitely thin along any axis
			graph.forcedBoundsSize = Vector3.Max(graph.forcedBoundsSize, Vector3.one * 0.001f);
			graph.rotation = EditorGUILayout.Vector3Field("Rotation", graph.rotation);

			if (GUILayout.Button(new GUIContent("Snap bounds to scene", "Will snap the bounds of the graph to exactly contain all meshes that the bounds currently touches"))) {
				graph.SnapForceBoundsToScene();
				GUI.changed = true;
			}

			Separator();

			EditorGUILayout.HelpBox("Objects contained in any of these masks will be rasterized", MessageType.None);
			graph.mask = EditorGUILayoutx.LayerMaskField("Layer Mask", graph.mask);
			tagMaskFoldout = EditorGUILayoutx.UnityTagMaskList(new GUIContent("Tag Mask"), tagMaskFoldout, graph.tagMask);

			Separator();

			GUILayout.BeginHorizontal();
			GUILayout.Space(18);
			graph.showMeshSurface = GUILayout.Toggle(graph.showMeshSurface, new GUIContent("Show surface", "Toggles gizmos for drawing the surface of the mesh"), EditorStyles.miniButtonLeft);
			graph.showMeshOutline = GUILayout.Toggle(graph.showMeshOutline, new GUIContent("Show outline", "Toggles gizmos for drawing an outline of the nodes"), EditorStyles.miniButtonMid);
			graph.showNodeConnections = GUILayout.Toggle(graph.showNodeConnections, new GUIContent("Show connections", "Toggles gizmos for drawing node connections"), EditorStyles.miniButtonRight);
			GUILayout.EndHorizontal();


			Separator();
			GUILayout.Label(new GUIContent("Advanced"), EditorStyles.boldLabel);

			if (GUILayout.Button("Export to .obj file")) {
				ExportToFile(graph);
			}

			graph.relevantGraphSurfaceMode = (RecastGraph.RelevantGraphSurfaceMode)EditorGUILayout.EnumPopup(new GUIContent("Relevant Graph Surface Mode",
					"Require every region to have a RelevantGraphSurface component inside it.\n" +
					"A RelevantGraphSurface component placed in the scene specifies that\n" +
					"the navmesh region it is inside should be included in the navmesh.\n\n" +
					"If this is set to OnlyForCompletelyInsideTile\n" +
					"a navmesh region is included in the navmesh if it\n" +
					"has a RelevantGraphSurface inside it, or if it\n" +
					"is adjacent to a tile border. This can leave some small regions\n" +
					"which you didn't want to have included because they are adjacent\n" +
					"to tile borders, but it removes the need to place a component\n" +
					"in every single tile, which can be tedious (see below).\n\n" +
					"If this is set to RequireForAll\n" +
					"a navmesh region is included only if it has a RelevantGraphSurface\n" +
					"inside it. Note that even though the navmesh\n" +
					"looks continous between tiles, the tiles are computed individually\n" +
					"and therefore you need a RelevantGraphSurface component for each\n" +
					"region and for each tile."),
				graph.relevantGraphSurfaceMode);

			graph.nearestSearchOnlyXZ = EditorGUILayout.Toggle(new GUIContent("Nearest node queries in XZ space",
					"Recomended for single-layered environments.\nFaster but can be inacurate esp. in multilayered contexts."), graph.nearestSearchOnlyXZ);

			if (graph.nearestSearchOnlyXZ && (Mathf.Abs(graph.rotation.x) > 1 || Mathf.Abs(graph.rotation.z) > 1)) {
				EditorGUILayout.HelpBox("Nearest node queries in XZ space is not recommended for rotated graphs since XZ space no longer corresponds to the ground plane", MessageType.Warning);
			}
		}

		/** Exports the INavmesh graph to a .obj file */
		public static void ExportToFile (RecastGraph target) {
			//INavmesh graph = (INavmesh)target;
			if (target == null) return;

			NavmeshTile[] tiles = target.GetTiles();

			if (tiles == null) {
				if (EditorUtility.DisplayDialog("Scan graph before exporting?", "The graph does not contain any mesh data. Do you want to scan it?", "Ok", "Cancel")) {
					AstarPathEditor.MenuScan();
					tiles = target.GetTiles();
					if (tiles == null) return;
				} else {
					return;
				}
			}

			string path = EditorUtility.SaveFilePanel("Export .obj", "", "navmesh.obj", "obj");
			if (path == "") return;

			//Generate .obj
			var sb = new System.Text.StringBuilder();

			string name = System.IO.Path.GetFileNameWithoutExtension(path);

			sb.Append("g ").Append(name).AppendLine();

			//Vertices start from 1
			int vCount = 1;

			//Define single texture coordinate to zero
			sb.Append("vt 0 0\n");

			for (int t = 0; t < tiles.Length; t++) {
				NavmeshTile tile = tiles[t];

				if (tile == null) continue;

				Int3[] vertices = tile.verts;

				//Write vertices
				for (int i = 0; i < vertices.Length; i++) {
					var v = (Vector3)vertices[i];
					sb.Append(string.Format("v {0} {1} {2}\n", -v.x, v.y, v.z));
				}

				//Write triangles
				TriangleMeshNode[] nodes = tile.nodes;
				for (int i = 0; i < nodes.Length; i++) {
					TriangleMeshNode node = nodes[i];
					if (node == null) {
						Debug.LogError("Node was null or no TriangleMeshNode. Critical error. Graph type " + target.GetType().Name);
						return;
					}
					if (node.GetVertexArrayIndex(0) < 0 || node.GetVertexArrayIndex(0) >= vertices.Length) throw new System.Exception("ERR");

					sb.Append(string.Format("f {0}/1 {1}/1 {2}/1\n", (node.GetVertexArrayIndex(0) + vCount), (node.GetVertexArrayIndex(1) + vCount), (node.GetVertexArrayIndex(2) + vCount)));
				}

				vCount += vertices.Length;
			}

			string obj = sb.ToString();

			using (var sw = new System.IO.StreamWriter(path))
			{
				sw.Write(obj);
			}
		}
	}
}
