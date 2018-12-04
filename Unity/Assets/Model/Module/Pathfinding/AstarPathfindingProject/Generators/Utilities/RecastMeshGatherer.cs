using UnityEngine;
using System.Collections.Generic;
using PF;
using Mathf = UnityEngine.Mathf;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Pathfinding.Recast {
	using System;
	using Pathfinding;
	using Pathfinding.Voxels;

	internal class RecastMeshGatherer {
		readonly int terrainSampleSize;
		readonly LayerMask mask;
		readonly List<string> tagMask;
		readonly float colliderRasterizeDetail;
		readonly Bounds bounds;

		public RecastMeshGatherer (Bounds bounds, int terrainSampleSize, LayerMask mask, List<string> tagMask, float colliderRasterizeDetail) {
			// Clamp to at least 1 since that's the resolution of the heightmap
			terrainSampleSize = Math.Max(terrainSampleSize, 1);

			this.bounds = bounds;
			this.terrainSampleSize = terrainSampleSize;
			this.mask = mask;
			this.tagMask = tagMask ?? new List<string>();
			this.colliderRasterizeDetail = colliderRasterizeDetail;
		}

		static List<MeshFilter> FilterMeshes (MeshFilter[] meshFilters, List<string> tagMask, LayerMask layerMask) {
			var filtered = new List<MeshFilter>(meshFilters.Length / 3);

			for (int i = 0; i < meshFilters.Length; i++) {
				MeshFilter filter = meshFilters[i];
				Renderer rend = filter.GetComponent<Renderer>();

				if (rend != null && filter.sharedMesh != null && rend.enabled && (((1 << filter.gameObject.layer) & layerMask) != 0 || tagMask.Contains(filter.tag))) {
					if (filter.GetComponent<RecastMeshObj>() == null) {
						filtered.Add(filter);
					}
				}
			}

			return filtered;
		}

		public void CollectSceneMeshes (List<RasterizationMesh> meshes) {
			if (tagMask.Count > 0 || mask != 0) {
				// This is unfortunately the fastest way to find all mesh filters.. and it is not particularly fast.
				var meshFilters = GameObject.FindObjectsOfType<MeshFilter>();
				var filteredMeshes = FilterMeshes(meshFilters, tagMask, mask);

				var cachedVertices = new Dictionary<Mesh, Vector3[]>();
				var cachedTris = new Dictionary<Mesh, int[]>();

				bool containedStatic = false;

				for (int i = 0; i < filteredMeshes.Count; i++) {
					MeshFilter filter = filteredMeshes[i];

					// Note, guaranteed to have a renderer
					Renderer rend = filter.GetComponent<Renderer>();

					if (rend.isPartOfStaticBatch) {
						// Statically batched meshes cannot be used due to Unity limitations
						// log a warning about this
						containedStatic = true;
					} else {
						// Only include it if it intersects with the graph
						if (rend.bounds.Intersects(bounds)) {
							Mesh mesh = filter.sharedMesh;
							RasterizationMesh smesh;

							// Check the cache to avoid allocating
							// a new array unless necessary
							if (cachedVertices.ContainsKey(mesh)) {
								smesh = new RasterizationMesh(cachedVertices[mesh], cachedTris[mesh], rend.bounds);
							} else {
								smesh = new RasterizationMesh(mesh.vertices, mesh.triangles, rend.bounds);
								cachedVertices[mesh] = smesh.vertices;
								cachedTris[mesh] = smesh.triangles;
							}

							smesh.matrix = rend.localToWorldMatrix;
							smesh.original = filter;
							meshes.Add(smesh);
						}
					}

					if (containedStatic)
						Debug.LogWarning("Some meshes were statically batched. These meshes can not be used for navmesh calculation" +
							" due to technical constraints.\nDuring runtime scripts cannot access the data of meshes which have been statically batched.\n" +
							"One way to solve this problem is to use cached startup (Save & Load tab in the inspector) to only calculate the graph when the game is not playing.");
				}

				#if ASTARDEBUG
				int y = 0;
				foreach (RasterizationMesh smesh in meshes) {
					y++;
					Vector3[] vecs = smesh.vertices;
					int[] tris = smesh.triangles;

					for (int i = 0; i < tris.Length; i += 3) {
						Vector3 p1 = smesh.matrix.MultiplyPoint3x4(vecs[tris[i+0]]);
						Vector3 p2 = smesh.matrix.MultiplyPoint3x4(vecs[tris[i+1]]);
						Vector3 p3 = smesh.matrix.MultiplyPoint3x4(vecs[tris[i+2]]);

						Debug.DrawLine(p1, p2, Color.red, 1);
						Debug.DrawLine(p2, p3, Color.red, 1);
						Debug.DrawLine(p3, p1, Color.red, 1);
					}
				}
				#endif
			}
		}

		/** Find all relevant RecastMeshObj components and create ExtraMeshes for them */
		public void CollectRecastMeshObjs (List<RasterizationMesh> buffer) {
			var buffer2 = ListPool<RecastMeshObj>.Claim();

			// Get all recast mesh objects inside the bounds
			RecastMeshObj.GetAllInBounds(buffer2, bounds);

			var cachedVertices = new Dictionary<Mesh, Vector3[]>();
			var cachedTris = new Dictionary<Mesh, int[]>();

			// Create an RasterizationMesh object
			// for each RecastMeshObj
			for (int i = 0; i < buffer2.Count; i++) {
				MeshFilter filter = buffer2[i].GetMeshFilter();
				Renderer rend = filter != null ? filter.GetComponent<Renderer>() : null;

				if (filter != null && rend != null) {
					Mesh mesh = filter.sharedMesh;
					RasterizationMesh smesh;

					// Don't read the vertices and triangles from the
					// mesh if we have seen the same mesh previously
					if (cachedVertices.ContainsKey(mesh)) {
						smesh = new RasterizationMesh(cachedVertices[mesh], cachedTris[mesh], rend.bounds);
					} else {
						smesh = new RasterizationMesh(mesh.vertices, mesh.triangles, rend.bounds);
						cachedVertices[mesh] = smesh.vertices;
						cachedTris[mesh] = smesh.triangles;
					}

					smesh.matrix = rend.localToWorldMatrix;
					smesh.original = filter;
					smesh.area = buffer2[i].area;
					buffer.Add(smesh);
				} else {
					Collider coll = buffer2[i].GetCollider();

					if (coll == null) {
						Debug.LogError("RecastMeshObject ("+buffer2[i].gameObject.name +") didn't have a collider or MeshFilter+Renderer attached", buffer2[i].gameObject);
						continue;
					}

					RasterizationMesh smesh = RasterizeCollider(coll);

					// Make sure a valid RasterizationMesh was returned
					if (smesh != null) {
						smesh.area = buffer2[i].area;
						buffer.Add(smesh);
					}
				}
			}

			// Clear cache to avoid memory leak
			capsuleCache.Clear();

			ListPool<RecastMeshObj>.Release(ref buffer2);
		}

		public void CollectTerrainMeshes (bool rasterizeTrees, float desiredChunkSize, List<RasterizationMesh> result) {
			// Find all terrains in the scene
			var terrains = Terrain.activeTerrains;

			if (terrains.Length > 0) {
				// Loop through all terrains in the scene
				for (int j = 0; j < terrains.Length; j++) {
					if (terrains[j].terrainData == null) continue;

					GenerateTerrainChunks(terrains[j], bounds, desiredChunkSize, result);

					if (rasterizeTrees) {
						// Rasterize all tree colliders on this terrain object
						CollectTreeMeshes(terrains[j], result);
					}
				}
			}
		}

		void GenerateTerrainChunks (Terrain terrain, Bounds bounds, float desiredChunkSize, List<RasterizationMesh> result) {
			var terrainData = terrain.terrainData;

			if (terrainData == null)
				throw new System.ArgumentException("Terrain contains no terrain data");

			Vector3 offset = terrain.GetPosition();
			Vector3 center = offset + terrainData.size * 0.5F;

			// Figure out the bounds of the terrain in world space
			var terrainBounds = new Bounds(center, terrainData.size);

			// Only include terrains which intersects the graph
			if (!terrainBounds.Intersects(bounds))
				return;

			// Original heightmap size
			int heightmapWidth = terrainData.heightmapWidth;
			int heightmapDepth = terrainData.heightmapHeight;

			// Sample the terrain heightmap
			float[, ] heights = terrainData.GetHeights(0, 0, heightmapWidth, heightmapDepth);

			Vector3 sampleSize = terrainData.heightmapScale;
			sampleSize.y = terrainData.size.y;

			// Make chunks at least 12 quads wide
			// since too small chunks just decreases performance due
			// to the overhead of checking for bounds and similar things
			const int MinChunkSize = 12;

			// Find the number of samples along each edge that corresponds to a world size of desiredChunkSize
			// Then round up to the nearest multiple of terrainSampleSize
			var chunkSizeAlongX = Mathf.CeilToInt(Mathf.Max(desiredChunkSize / (sampleSize.x * terrainSampleSize), MinChunkSize)) * terrainSampleSize;
			var chunkSizeAlongZ = Mathf.CeilToInt(Mathf.Max(desiredChunkSize / (sampleSize.z * terrainSampleSize), MinChunkSize)) * terrainSampleSize;

			for (int z = 0; z < heightmapDepth; z += chunkSizeAlongZ) {
				for (int x = 0; x < heightmapWidth; x += chunkSizeAlongX) {
					var width = Mathf.Min(chunkSizeAlongX, heightmapWidth - x);
					var depth = Mathf.Min(chunkSizeAlongZ, heightmapDepth - z);
					var chunkMin = offset + new Vector3(z * sampleSize.x, 0, x * sampleSize.z);
					var chunkMax = offset + new Vector3((z + depth) * sampleSize.x, sampleSize.y, (x + width) * sampleSize.z);
					var chunkBounds = new Bounds();
					chunkBounds.SetMinMax(chunkMin, chunkMax);

					// Skip chunks that are not inside the desired bounds
					if (chunkBounds.Intersects(bounds)) {
						var chunk = GenerateHeightmapChunk(heights, sampleSize, offset, x, z, width, depth, terrainSampleSize);
						result.Add(chunk);
					}
				}
			}
		}

		/** Returns ceil(lhs/rhs), i.e lhs/rhs rounded up */
		static int CeilDivision (int lhs, int rhs) {
			return (lhs + rhs - 1)/rhs;
		}

		/** Generates a terrain chunk mesh */
		RasterizationMesh GenerateHeightmapChunk (float[, ] heights, Vector3 sampleSize, Vector3 offset, int x0, int z0, int width, int depth, int stride) {
			// Downsample to a smaller mesh (full resolution will take a long time to rasterize)
			// Round up the width to the nearest multiple of terrainSampleSize and then add 1
			// (off by one because there are vertices at the edge of the mesh)
			int resultWidth = CeilDivision(width, terrainSampleSize) + 1;
			int resultDepth = CeilDivision(depth, terrainSampleSize) + 1;

			var heightmapWidth = heights.GetLength(0);
			var heightmapDepth = heights.GetLength(1);

			// Create a mesh from the heightmap
			var numVerts = resultWidth * resultDepth;
			var terrainVertices = ArrayPool<Vector3>.Claim(numVerts);

			// Create lots of vertices
			for (int z = 0; z < resultDepth; z++) {
				for (int x = 0; x < resultWidth; x++) {
					int sampleX = Math.Min(x0 + x*stride, heightmapWidth-1);
					int sampleZ = Math.Min(z0 + z*stride, heightmapDepth-1);

					terrainVertices[z*resultWidth + x] = new Vector3(sampleZ * sampleSize.x, heights[sampleX, sampleZ]*sampleSize.y, sampleX * sampleSize.z) + offset;
				}
			}

			// Create the mesh by creating triangles in a grid like pattern
			int numTris = (resultWidth-1)*(resultDepth-1)*2*3;
			var tris = ArrayPool<int>.Claim(numTris);
			int triangleIndex = 0;
			for (int z = 0; z < resultDepth-1; z++) {
				for (int x = 0; x < resultWidth-1; x++) {
					tris[triangleIndex]   = z*resultWidth + x;
					tris[triangleIndex+1] = z*resultWidth + x+1;
					tris[triangleIndex+2] = (z+1)*resultWidth + x+1;
					triangleIndex += 3;
					tris[triangleIndex]   = z*resultWidth + x;
					tris[triangleIndex+1] = (z+1)*resultWidth + x+1;
					tris[triangleIndex+2] = (z+1)*resultWidth + x;
					triangleIndex += 3;
				}
			}

#if ASTARDEBUG
			var color = AstarMath.IntToColor(x0 + 7 * z0, 0.7f);
			for (int i = 0; i < numTris; i += 3) {
				Debug.DrawLine(terrainVertices[tris[i]], terrainVertices[tris[i+1]], color, 40);
				Debug.DrawLine(terrainVertices[tris[i+1]], terrainVertices[tris[i+2]], color, 40);
				Debug.DrawLine(terrainVertices[tris[i+2]], terrainVertices[tris[i]], color, 40);
			}
#endif

			var mesh = new RasterizationMesh(terrainVertices, tris, new Bounds());
			mesh.numVertices = numVerts;
			mesh.numTriangles = numTris;
			mesh.pool = true;
			// Could probably calculate these bounds in a faster way
			mesh.RecalculateBounds();
			return mesh;
		}

		void CollectTreeMeshes (Terrain terrain, List<RasterizationMesh> result) {
			TerrainData data = terrain.terrainData;

			for (int i = 0; i < data.treeInstances.Length; i++) {
				TreeInstance instance = data.treeInstances[i];
				TreePrototype prot = data.treePrototypes[instance.prototypeIndex];

				// Make sure that the tree prefab exists
				if (prot.prefab == null) {
					continue;
				}

				var collider = prot.prefab.GetComponent<Collider>();
				var treePosition = terrain.transform.position +  Vector3.Scale(instance.position, data.size);

				if (collider == null) {
					var instanceBounds = new Bounds(terrain.transform.position + Vector3.Scale(instance.position, data.size), new Vector3(instance.widthScale, instance.heightScale, instance.widthScale));

					Matrix4x4 matrix = Matrix4x4.TRS(treePosition, Quaternion.identity, new Vector3(instance.widthScale, instance.heightScale, instance.widthScale)*0.5f);

					var mesh = new RasterizationMesh(BoxColliderVerts, BoxColliderTris, instanceBounds, matrix);

					result.Add(mesh);
				} else {
					// The prefab has a collider, use that instead
					var scale = new Vector3(instance.widthScale, instance.heightScale, instance.widthScale);

					// Generate a mesh from the collider
					RasterizationMesh mesh = RasterizeCollider(collider, Matrix4x4.TRS(treePosition, Quaternion.identity, scale));

					// Make sure a valid mesh was generated
					if (mesh != null) {
						// The bounds are incorrectly based on collider.bounds.
						// It is incorrect because the collider is on the prefab, not on the tree instance
						// so we need to recalculate the bounds based on the actual vertex positions
						mesh.RecalculateBounds();
						result.Add(mesh);
					}
				}
			}
		}

		public void CollectColliderMeshes (List<RasterizationMesh> result) {
			/** \todo Use Physics.OverlapBox on newer Unity versions */
			// Find all colliders that could possibly be inside the bounds
			var colls = Physics.OverlapSphere(bounds.center, bounds.size.magnitude, -1, QueryTriggerInteraction.Ignore);

			if (tagMask.Count > 0 || mask != 0) {
				for (int i = 0; i < colls.Length; i++) {
					Collider collider = colls[i];

					if ((((mask >> collider.gameObject.layer) & 1) != 0 || tagMask.Contains(collider.tag)) && collider.enabled && !collider.isTrigger && collider.bounds.Intersects(bounds) && collider.GetComponent<RecastMeshObj>() == null) {
						RasterizationMesh emesh = RasterizeCollider(collider);
						//Make sure a valid RasterizationMesh was returned
						if (emesh != null)
							result.Add(emesh);
					}
				}
			}

			// Clear cache to avoid memory leak
			capsuleCache.Clear();
		}

		/** Box Collider triangle indices can be reused for multiple instances.
		 * \warning This array should never be changed
		 */
		private readonly static int[] BoxColliderTris = {
			0, 1, 2,
			0, 2, 3,

			6, 5, 4,
			7, 6, 4,

			0, 5, 1,
			0, 4, 5,

			1, 6, 2,
			1, 5, 6,

			2, 7, 3,
			2, 6, 7,

			3, 4, 0,
			3, 7, 4
		};

		/** Box Collider vertices can be reused for multiple instances.
		 * \warning This array should never be changed
		 */
		private readonly static Vector3[] BoxColliderVerts = {
			new Vector3(-1, -1, -1),
			new Vector3(1, -1, -1),
			new Vector3(1, -1, 1),
			new Vector3(-1, -1, 1),

			new Vector3(-1, 1, -1),
			new Vector3(1, 1, -1),
			new Vector3(1, 1, 1),
			new Vector3(-1, 1, 1),
		};

		/** Holds meshes for capsules to avoid generating duplicate capsule meshes for identical capsules */
		private List<CapsuleCache> capsuleCache = new List<CapsuleCache>();

		class CapsuleCache {
			public int rows;
			public float height;
			public Vector3[] verts;
			public int[] tris;
		}

		/** Rasterizes a collider to a mesh.
		 * This will pass the col.transform.localToWorldMatrix to the other overload of this function.
		 */
		RasterizationMesh RasterizeCollider (Collider col) {
			return RasterizeCollider(col, col.transform.localToWorldMatrix);
		}

		/** Rasterizes a collider to a mesh assuming it's vertices should be multiplied with the matrix.
		 * Note that the bounds of the returned RasterizationMesh is based on collider.bounds. So you might want to
		 * call myExtraMesh.RecalculateBounds on the returned mesh to recalculate it if the collider.bounds would
		 * not give the correct value.
		 */
		RasterizationMesh RasterizeCollider (Collider col, Matrix4x4 localToWorldMatrix) {
			RasterizationMesh result = null;

			if (col is BoxCollider) {
				result = RasterizeBoxCollider(col as BoxCollider, localToWorldMatrix);
			} else if (col is SphereCollider || col is CapsuleCollider) {
				var scollider = col as SphereCollider;
				var ccollider = col as CapsuleCollider;

				float radius = (scollider != null ? scollider.radius : ccollider.radius);
				float height = scollider != null ? 0 : (ccollider.height*0.5f/radius) - 1;
				Quaternion rot = Quaternion.identity;
				// Capsule colliders can be aligned along the X, Y or Z axis
				if (ccollider != null) rot = Quaternion.Euler(ccollider.direction == 2 ? 90 : 0, 0, ccollider.direction == 0 ? 90 : 0);
				Matrix4x4 matrix = Matrix4x4.TRS(scollider != null ? scollider.center : ccollider.center, rot, Vector3.one*radius);

				matrix = localToWorldMatrix * matrix;

				result = RasterizeCapsuleCollider(radius, height, col.bounds, matrix);
			} else if (col is MeshCollider) {
				var collider = col as MeshCollider;

				if (collider.sharedMesh != null) {
					result = new RasterizationMesh(collider.sharedMesh.vertices, collider.sharedMesh.triangles, collider.bounds, localToWorldMatrix);
				}
			}

			#if ASTARDEBUG
			for (int i = 0; i < result.triangles.Length; i += 3) {
				Debug.DrawLine(result.matrix.MultiplyPoint3x4(result.vertices[result.triangles[i]]), result.matrix.MultiplyPoint3x4(result.vertices[result.triangles[i+1]]), Color.yellow);
				Debug.DrawLine(result.matrix.MultiplyPoint3x4(result.vertices[result.triangles[i+2]]), result.matrix.MultiplyPoint3x4(result.vertices[result.triangles[i+1]]), Color.yellow);
				Debug.DrawLine(result.matrix.MultiplyPoint3x4(result.vertices[result.triangles[i]]), result.matrix.MultiplyPoint3x4(result.vertices[result.triangles[i+2]]), Color.yellow);
			}
			#endif

			return result;
		}

		RasterizationMesh RasterizeBoxCollider (BoxCollider collider, Matrix4x4 localToWorldMatrix) {
			Matrix4x4 matrix = Matrix4x4.TRS(collider.center, Quaternion.identity, collider.size*0.5f);

			matrix = localToWorldMatrix * matrix;

			return new RasterizationMesh(BoxColliderVerts, BoxColliderTris, collider.bounds, matrix);
		}

		RasterizationMesh RasterizeCapsuleCollider (float radius, float height, Bounds bounds, Matrix4x4 localToWorldMatrix) {
			// Calculate the number of rows to use
			// grows as sqrt(x) to the radius of the sphere/capsule which I have found works quite well
			int rows = Mathf.Max(4, Mathf.RoundToInt(colliderRasterizeDetail*Mathf.Sqrt(localToWorldMatrix.MultiplyVector(Vector3.one).magnitude)));

			if (rows > 100) {
				Debug.LogWarning("Very large detail for some collider meshes. Consider decreasing Collider Rasterize Detail (RecastGraph)");
			}

			int cols = rows;

			Vector3[] verts;
			int[] trisArr;

			// Check if we have already calculated a similar capsule
			CapsuleCache cached = null;
			for (int i = 0; i < capsuleCache.Count; i++) {
				CapsuleCache c = capsuleCache[i];
				if (c.rows == rows && Mathf.Approximately(c.height, height)) {
					cached = c;
				}
			}

			if (cached == null) {
				// Generate a sphere/capsule mesh

				verts = new Vector3[(rows)*cols + 2];

				var tris = new List<int>();
				verts[verts.Length-1] = Vector3.up;

				for (int r = 0; r < rows; r++) {
					for (int c = 0; c < cols; c++) {
						verts[c + r*cols] = new Vector3(Mathf.Cos(c*Mathf.PI*2/cols)*Mathf.Sin((r*Mathf.PI/(rows-1))), Mathf.Cos((r*Mathf.PI/(rows-1))) + (r < rows/2 ? height : -height), Mathf.Sin(c*Mathf.PI*2/cols)*Mathf.Sin((r*Mathf.PI/(rows-1))));
					}
				}

				verts[verts.Length-2] = Vector3.down;

				for (int i = 0, j = cols-1; i < cols; j = i++) {
					tris.Add(verts.Length-1);
					tris.Add(0*cols + j);
					tris.Add(0*cols + i);
				}

				for (int r = 1; r < rows; r++) {
					for (int i = 0, j = cols-1; i < cols; j = i++) {
						tris.Add(r*cols + i);
						tris.Add(r*cols + j);
						tris.Add((r-1)*cols + i);

						tris.Add((r-1)*cols + j);
						tris.Add((r-1)*cols + i);
						tris.Add(r*cols + j);
					}
				}

				for (int i = 0, j = cols-1; i < cols; j = i++) {
					tris.Add(verts.Length-2);
					tris.Add((rows-1)*cols + j);
					tris.Add((rows-1)*cols + i);
				}

				// Add calculated mesh to the cache
				cached = new CapsuleCache();
				cached.rows = rows;
				cached.height = height;
				cached.verts = verts;
				cached.tris = tris.ToArray();
				capsuleCache.Add(cached);
			}

			// Read from cache
			verts = cached.verts;
			trisArr = cached.tris;

			return new RasterizationMesh(verts, trisArr, bounds, localToWorldMatrix);
		}
	}
}
