using UnityEngine;
using System.Collections.Generic;
using PF;
using Color32 = UnityEngine.Color32;
using Mathf = UnityEngine.Mathf;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace Pathfinding.Util {
	/** Helper for drawing Gizmos in a performant way.
	 * This is a replacement for the Unity Gizmos class as that is not very performant
	 * when drawing very large amounts of geometry (for example a large grid graph).
	 * These gizmos can be persistent, so if the data does not change, the gizmos
	 * do not need to be updated.
	 *
	 * How to use
	 * - Create a Hasher object and hash whatever data you will be using to draw the gizmos
	 *      Could be for example the positions of the vertices or something. Just as long as
	 *      if the gizmos should change, then the hash changes as well.
	 * - Check if a cached mesh exists for that hash
	 * - If not, then create a Builder object and call the drawing methods until you are done
	 *      and then call Finalize with a reference to a gizmos class and the hash you calculated before.
	 * - Call gizmos.Draw with the hash.
	 * - When you are done with drawing gizmos for this frame, call gizmos.FinalizeDraw
	 *
	 * \code
	 * var a = Vector3.zero;
	 * var b = Vector3.one;
	 * var color = Color.red;
	 * var hasher = new RetainedGizmos.Hasher();
	 * hasher.AddHash(a.GetHashCode());
	 * hasher.AddHash(b.GetHashCode());
	 * hasher.AddHash(color.GetHashCode());
	 * if (!gizmos.Draw(hasher)) {
	 *     using (var helper = gizmos.GetGizmoHelper(active, hasher)) {
	 *         builder.DrawLine(a, b, color);
	 *         builder.Finalize(gizmos, hasher);
	 *     }
	 * }
	 * \endcode
	 */
	public class RetainedGizmos {
		/** Combines hashes into a single hash value */
		public struct Hasher {
			ulong hash;
			bool includePathSearchInfo;
			PathHandler debugData;

			public Hasher (AstarPath active) {
				hash = 0;
				this.debugData = active.debugPathData;
				includePathSearchInfo = debugData != null && (active.debugMode == GraphDebugMode.F || active.debugMode == GraphDebugMode.G || active.debugMode == GraphDebugMode.H || active.showSearchTree);
				AddHash((int)active.debugMode);
				AddHash(active.debugFloor.GetHashCode());
				AddHash(active.debugRoof.GetHashCode());
			}

			public void AddHash (int hash) {
				this.hash = (1572869UL * this.hash) ^ (ulong)hash;
			}

			public void HashNode (GraphNode node) {
				AddHash(node.GetGizmoHashCode());

				if (includePathSearchInfo) {
					var pathNode = debugData.GetPathNode(node.NodeIndex);
					AddHash((int)pathNode.pathID);
					AddHash(pathNode.pathID == debugData.PathID ? 1 : 0);
					AddHash((int) pathNode.F);
				}
			}

			public ulong Hash {
				get {
					return hash;
				}
			}
		}

		/** Helper for drawing gizmos */
		public class Builder : IAstarPooledObject {
			List<Vector3> lines = new List<Vector3>();
			List<Color32> lineColors = new List<Color32>();
			List<Mesh> meshes = new List<Mesh>();

			public void DrawMesh (RetainedGizmos gizmos, Vector3[] vertices, List<int> triangles, Color[] colors) {
				var mesh = gizmos.GetMesh();

				// Set all data on the mesh
				mesh.vertices = vertices;
				mesh.SetTriangles(triangles, 0);
				mesh.colors = colors;

				// Upload all data and mark the mesh as unreadable
				mesh.UploadMeshData(true);
				meshes.Add(mesh);
			}

			/** Draws a wire cube after being transformed the specified transformation */
			public void DrawWireCube (GraphTransform tr, Bounds bounds, Color color) {
				var min = bounds.min;
				var max = bounds.max;

				DrawLine(tr.Transform(new Vector3(min.x, min.y, min.z)), tr.Transform(new Vector3(max.x, min.y, min.z)), color);
				DrawLine(tr.Transform(new Vector3(max.x, min.y, min.z)), tr.Transform(new Vector3(max.x, min.y, max.z)), color);
				DrawLine(tr.Transform(new Vector3(max.x, min.y, max.z)), tr.Transform(new Vector3(min.x, min.y, max.z)), color);
				DrawLine(tr.Transform(new Vector3(min.x, min.y, max.z)), tr.Transform(new Vector3(min.x, min.y, min.z)), color);

				DrawLine(tr.Transform(new Vector3(min.x, max.y, min.z)), tr.Transform(new Vector3(max.x, max.y, min.z)), color);
				DrawLine(tr.Transform(new Vector3(max.x, max.y, min.z)), tr.Transform(new Vector3(max.x, max.y, max.z)), color);
				DrawLine(tr.Transform(new Vector3(max.x, max.y, max.z)), tr.Transform(new Vector3(min.x, max.y, max.z)), color);
				DrawLine(tr.Transform(new Vector3(min.x, max.y, max.z)), tr.Transform(new Vector3(min.x, max.y, min.z)), color);

				DrawLine(tr.Transform(new Vector3(min.x, min.y, min.z)), tr.Transform(new Vector3(min.x, max.y, min.z)), color);
				DrawLine(tr.Transform(new Vector3(max.x, min.y, min.z)), tr.Transform(new Vector3(max.x, max.y, min.z)), color);
				DrawLine(tr.Transform(new Vector3(max.x, min.y, max.z)), tr.Transform(new Vector3(max.x, max.y, max.z)), color);
				DrawLine(tr.Transform(new Vector3(min.x, min.y, max.z)), tr.Transform(new Vector3(min.x, max.y, max.z)), color);
			}

			public void DrawLine (Vector3 start, Vector3 end, Color color) {
				lines.Add(start);
				lines.Add(end);
				var col32 = (Color32)color;
				lineColors.Add(col32);
				lineColors.Add(col32);
			}

			public void Submit (RetainedGizmos gizmos, Hasher hasher) {
				SubmitLines(gizmos, hasher.Hash);
				SubmitMeshes(gizmos, hasher.Hash);
			}

			void SubmitMeshes (RetainedGizmos gizmos, ulong hash) {
				for (int i = 0; i < meshes.Count; i++) {
					gizmos.meshes.Add(new MeshWithHash { hash = hash, mesh = meshes[i], lines = false });
					gizmos.existingHashes.Add(hash);
				}
			}

			void SubmitLines (RetainedGizmos gizmos, ulong hash) {
				// Unity only supports 65535 vertices per mesh. 65532 used because MaxLineEndPointsPerBatch needs to be even.
				const int MaxLineEndPointsPerBatch = 65532/2;
				int batches = (lines.Count + MaxLineEndPointsPerBatch - 1)/MaxLineEndPointsPerBatch;

				for (int batch = 0; batch < batches; batch++) {
					int startIndex = MaxLineEndPointsPerBatch * batch;
					int endIndex = Mathf.Min(startIndex + MaxLineEndPointsPerBatch, lines.Count);
					int lineEndPointCount = endIndex - startIndex;
					UnityEngine.Assertions.Assert.IsTrue(lineEndPointCount % 2 == 0);

					// Use pooled lists to avoid excessive allocations
					var vertices = ListPool<Vector3>.Claim(lineEndPointCount*2);
					var colors = ListPool<Color32>.Claim(lineEndPointCount*2);
					var normals = ListPool<Vector3>.Claim(lineEndPointCount*2);
					var uv = ListPool<Vector2>.Claim(lineEndPointCount*2);
					var tris = ListPool<int>.Claim(lineEndPointCount*3);
					// Loop through each endpoint of the lines
					// and add 2 vertices for each
					for (int j = startIndex; j < endIndex; j++) {
						var vertex = (Vector3)lines[j];
						vertices.Add(vertex);
						vertices.Add(vertex);

						var color = (Color32)lineColors[j];
						colors.Add(color);
						colors.Add(color);
						uv.Add(new Vector2(0, 0));
						uv.Add(new Vector2(1, 0));
					}

					// Loop through each line and add
					// one normal for each vertex
					for (int j = startIndex; j < endIndex; j += 2) {
						var lineDir = (Vector3)(lines[j+1] - lines[j]);
						// Store the line direction in the normals.
						// A line consists of 4 vertices. The line direction will be used to
						// offset the vertices to create a line with a fixed pixel thickness
						normals.Add(lineDir);
						normals.Add(lineDir);
						normals.Add(lineDir);
						normals.Add(lineDir);
					}

					// Setup triangle indices
					// A triangle consists of 3 indices
					// A line (4 vertices) consists of 2 triangles, so 6 triangle indices
					for (int j = 0, v = 0; j < lineEndPointCount*3; j += 6, v += 4) {
						// First triangle
						tris.Add(v+0);
						tris.Add(v+1);
						tris.Add(v+2);

						// Second triangle
						tris.Add(v+1);
						tris.Add(v+3);
						tris.Add(v+2);
					}

					var mesh = gizmos.GetMesh();

					// Set all data on the mesh
					mesh.SetVertices(vertices);
					mesh.SetTriangles(tris, 0);
					mesh.SetColors(colors);
					mesh.SetNormals(normals);
					mesh.SetUVs(0, uv);

					// Upload all data and mark the mesh as unreadable
					mesh.UploadMeshData(true);

					// Release the lists back to the pool
					ListPool<Vector3>.Release(ref vertices);
					ListPool<Color32>.Release(ref colors);
					ListPool<Vector3>.Release(ref normals);
					ListPool<Vector2>.Release(ref uv);
					ListPool<int>.Release(ref tris);

					gizmos.meshes.Add(new MeshWithHash { hash = hash, mesh = mesh, lines = true });
					gizmos.existingHashes.Add(hash);
				}
			}

			void IAstarPooledObject.OnEnterPool () {
				lines.Clear();
				lineColors.Clear();
				meshes.Clear();
			}
		}

		struct MeshWithHash {
			public ulong hash;
			public Mesh mesh;
			public bool lines;
		}

		List<MeshWithHash> meshes = new List<MeshWithHash>();
		HashSet<ulong> usedHashes = new HashSet<ulong>();
		HashSet<ulong> existingHashes = new HashSet<ulong>();
		Stack<Mesh> cachedMeshes = new Stack<Mesh>();

		public GraphGizmoHelper GetSingleFrameGizmoHelper () {
			var uniqHash = new RetainedGizmos.Hasher();

			uniqHash.AddHash(Time.realtimeSinceStartup.GetHashCode());
			Draw(uniqHash);
			return GetGizmoHelper(uniqHash);
		}

		public GraphGizmoHelper GetGizmoHelper (Hasher hasher) {
			var helper = ObjectPool<GraphGizmoHelper>.Claim();

			helper.Init(hasher, this);
			return helper;
		}

		void PoolMesh (Mesh mesh) {
			mesh.Clear();
			cachedMeshes.Push(mesh);
		}

		Mesh GetMesh () {
			if (cachedMeshes.Count > 0) {
				return cachedMeshes.Pop();
			} else {
				return new Mesh {
						   hideFlags = HideFlags.DontSave
				};
			}
		}

		/** Material to use for the navmesh in the editor */
		public Material surfaceMaterial;

		/** Material to use for the navmesh outline in the editor */
		public Material lineMaterial;

		/** True if there already is a mesh with the specified hash */
		public bool HasCachedMesh (Hasher hasher) {
			return existingHashes.Contains(hasher.Hash);
		}

		/** Schedules the meshes for the specified hash to be drawn.
		 * \returns False if there is no cached mesh for this hash, you may want to
		 *  submit one in that case. The draw command will be issued regardless of the return value.
		 */
		public bool Draw (Hasher hasher) {
			usedHashes.Add(hasher.Hash);
			return HasCachedMesh(hasher);
		}

		/** Schedules all meshes that were drawn the last frame (last time FinalizeDraw was called) to be drawn again.
		 * Also draws any new meshes that have been added since FinalizeDraw was last called.
		 */
		public void DrawExisting () {
			for (int i = 0; i < meshes.Count; i++) {
				usedHashes.Add(meshes[i].hash);
			}
		}

		/** Call after all #Draw commands for the frame have been done to draw everything */
		public void FinalizeDraw () {
			RemoveUnusedMeshes(meshes);

#if UNITY_EDITOR
			// Make sure the material references are correct
			if (surfaceMaterial == null) surfaceMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath(EditorResourceHelper.editorAssets + "/Materials/Navmesh.mat", typeof(Material)) as Material;
			if (lineMaterial == null) lineMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath(EditorResourceHelper.editorAssets + "/Materials/NavmeshOutline.mat", typeof(Material)) as Material;
#endif

			var cam = Camera.current;
			var planes = GeometryUtility.CalculateFrustumPlanes(cam);

			// Silently do nothing if the materials are not set
			if (surfaceMaterial == null || lineMaterial == null) return;

			Profiler.BeginSample("Draw Retained Gizmos");
			// First surfaces, then lines
			for (int matIndex = 0; matIndex <= 1; matIndex++) {
				var mat = matIndex == 0 ? surfaceMaterial : lineMaterial;
				for (int pass = 0; pass < mat.passCount; pass++) {
					mat.SetPass(pass);
					for (int i = 0; i < meshes.Count; i++) {
						if (meshes[i].lines == (mat == lineMaterial) && GeometryUtility.TestPlanesAABB(planes, meshes[i].mesh.bounds)) {
							Graphics.DrawMeshNow(meshes[i].mesh, Matrix4x4.identity);
						}
					}
				}
			}

			usedHashes.Clear();
			Profiler.EndSample();
		}

		/** Destroys all cached meshes.
		 * Used to make sure that no memory leaks happen in the Unity Editor.
		 */
		public void ClearCache () {
			usedHashes.Clear();
			RemoveUnusedMeshes(meshes);

			while (cachedMeshes.Count > 0) {
				Mesh.DestroyImmediate(cachedMeshes.Pop());
			}

			UnityEngine.Assertions.Assert.IsTrue(meshes.Count == 0);
		}

		void RemoveUnusedMeshes (List<MeshWithHash> meshList) {
			// Walk the array with two pointers
			// i pointing to the entry that should be filled with something
			// and j pointing to the entry that is a potential candidate for
			// filling the entry at i.
			// When j reaches the end of the list it will be reduced in size
			for (int i = 0, j = 0; i < meshList.Count; ) {
				if (j == meshList.Count) {
					j--;
					meshList.RemoveAt(j);
				} else if (usedHashes.Contains(meshList[j].hash)) {
					meshList[i] = meshList[j];
					i++;
					j++;
				} else {
					PoolMesh(meshList[j].mesh);
					existingHashes.Remove(meshList[j].hash);
					j++;
				}
			}
		}
	}
}
