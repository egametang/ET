using System;
using System.Collections.Generic;
using Pathfinding.Recast;
using Pathfinding.Util;
using Pathfinding.Voxels;
using PF;
using UnityEngine;
using UnityEngine.Profiling;
using Mathf = UnityEngine.Mathf;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Vector3 = UnityEngine.Vector3;

namespace Pathfinding
{
    public static class UnityHelper
    {
	    /** Called for each graph before they are scanned */
	    public static OnGraphDelegate OnGraphPreScan;
    
	    /** Called for each graph after they have been scanned. All other graphs might not have been scanned yet. */
	    public static OnGraphDelegate OnGraphPostScan;
        
            
	    /** Called before starting the scanning */
	    public static OnScanDelegate OnPreScan;
    
	    /** Called after scanning. This is called before applying links, flood-filling the graphs and other post processing. */
	    public static OnScanDelegate OnPostScan;
    
	    /** Called after scanning has completed fully. This is called as the last thing in the Scan function. */
	    public static OnScanDelegate OnLatePostScan;
    
	    /** Called when any graphs are updated. Register to for example recalculate the path whenever a graph changes. */
	    public static OnScanDelegate OnGraphsUpdated;
	    
	    
	    public static void Close()
	    {
		    OnGraphPreScan          = null;
		    OnGraphPostScan         = null;
		    OnPreScan               = null;
		    OnPostScan              = null;
		    OnLatePostScan          = null;
		    OnGraphsUpdated         = null;
	    }
	    
	    public static void OnDrawGizmos (this EuclideanEmbedding embedding) {
		    if (embedding.pivots != null) {
			    for (int i = 0; i < embedding.pivots.Length; i++) {
				    Gizmos.color = new Color(159/255.0f, 94/255.0f, 194/255.0f, 0.8f);

				    if (embedding.pivots[i] != null && !embedding.pivots[i].Destroyed) {
					    Gizmos.DrawCube((Vector3)embedding.pivots[i].position, Vector3.one);
				    }
			    }
		    }
	    }
	    
	    /** Returns the graph which contains the specified node.
* The graph must be in the #graphs array.
*
* \returns Returns the graph which contains the node. Null if the graph wasn't found
*/
	    public static NavGraph GetGraph (GraphNode node) {
		    if (node.Destroyed)
		    {
			    return null;
		    }
		    if (node == null) return null;

		    AstarPath script = AstarPath.active;
		    if (script == null) return null;

		    AstarData data = script.data;
		    if (data == null || data.graphs == null) return null;

		    uint graphIndex = node.GraphIndex;

		    if (graphIndex >= data.graphs.Length) {
			    return null;
		    }

		    return data.graphs[(int)graphIndex];
	    }
	    
	    
        /** Returns a bounds object with the bounding box of a group of tiles.
         * The bounding box is defined in world space.
         */
        public static Bounds GetTileBounds (this NavmeshBase navmeshBase, IntRect rect) {
            return navmeshBase.GetTileBounds(rect.xmin, rect.ymin, rect.Width, rect.Height);
        }

        /** Returns a bounds object with the bounding box of a group of tiles.
         * The bounding box is defined in world space.
         */
        public static Bounds GetTileBounds (this NavmeshBase navmeshBase, int x, int z, int width = 1, int depth = 1) {
            return navmeshBase.transform.Transform(navmeshBase.GetTileBoundsInGraphSpace(x, z, width, depth));
        }

        public static Bounds GetTileBoundsInGraphSpace (this NavmeshBase navmeshBase, IntRect rect) {
            return navmeshBase.GetTileBoundsInGraphSpace(rect.xmin, rect.ymin, rect.Width, rect.Height);
        }

        /** Returns an XZ bounds object with the bounds of a group of tiles in graph space */
        public static Bounds GetTileBoundsInGraphSpace (this NavmeshBase navmeshBase, int x, int z, int width = 1, int depth = 1) {
            var b = new Bounds();

            b.SetMinMax(
                        new Vector3(x*navmeshBase.TileWorldSizeX, 0, z*navmeshBase.TileWorldSizeZ),
                        new Vector3((x+width)*navmeshBase.TileWorldSizeX, navmeshBase.forcedBoundsSize.y, (z+depth)*navmeshBase.TileWorldSizeZ)
                       );
            return b;
        }
        
        public static void OnDrawGizmos (this NavmeshBase navmeshBase, Pathfinding.Util.RetainedGizmos gizmos, bool drawNodes) {
            if (!drawNodes) {
                return;
            }

            using (var helper = gizmos.GetSingleFrameGizmoHelper()) {
                var bounds = new Bounds();
                bounds.SetMinMax(Vector3.zero, navmeshBase.forcedBoundsSize);
                // Draw a write cube using the latest transform
                // (this makes the bounds update immediately if some field is changed in the editor)
                helper.builder.DrawWireCube(navmeshBase.CalculateTransform(), bounds, Color.white);
            }

            if (navmeshBase.tiles != null) {
                // Update navmesh vizualizations for
                // the tiles that have been changed
                for (int i = 0; i < navmeshBase.tiles.Length; i++) {
                    // This may happen if an exception has been thrown when the graph was scanned.
                    // We don't want the gizmo code to start to throw exceptions as well then as
                    // that would obscure the actual source of the error.
                    if (navmeshBase.tiles[i] == null) continue;

                    // Calculate a hash of the tile
                    var hasher = new RetainedGizmos.Hasher(AstarPath.active);
                    hasher.AddHash(navmeshBase.showMeshOutline ? 1 : 0);
                    hasher.AddHash(navmeshBase.showMeshSurface ? 1 : 0);
                    hasher.AddHash(navmeshBase.showNodeConnections ? 1 : 0);

                    var nodes = navmeshBase.tiles[i].nodes;
                    for (int j = 0; j < nodes.Length; j++) {
                        hasher.HashNode(nodes[j]);
                    }

                    if (!gizmos.Draw(hasher)) {
                        using (var helper = gizmos.GetGizmoHelper(hasher)) {
                            if (navmeshBase.showMeshSurface || navmeshBase.showMeshOutline) 
                                navmeshBase.CreateNavmeshSurfaceVisualization(navmeshBase.tiles[i], helper);
                            if (navmeshBase.showMeshSurface || navmeshBase.showMeshOutline) 
	                            CreateNavmeshOutlineVisualization(navmeshBase.tiles[i], helper);

                            if (navmeshBase.showNodeConnections) {
                                for (int j = 0; j < nodes.Length; j++) {
                                    helper.DrawConnections(nodes[j]);
                                }
                            }
                        }
                    }

                    gizmos.Draw(hasher);
                }
            }

            if (AstarPath.active.showUnwalkableNodes) 
	            navmeshBase.DrawUnwalkableNodes(AstarPath.active.unwalkableNodeDebugSize);
        }
        
        /** Creates a mesh of the surfaces of the navmesh for use in OnDrawGizmos in the editor */
		public static void CreateNavmeshSurfaceVisualization (this NavmeshBase navmeshBase, NavmeshTile tile, GraphGizmoHelper helper) {
			// Vertex array might be a bit larger than necessary, but that's ok
			var vertices = ArrayPool<Vector3>.Claim(tile.nodes.Length*3);
			var colors = ArrayPool<Color>.Claim(tile.nodes.Length*3);

			for (int j = 0; j < tile.nodes.Length; j++) {
				var node = tile.nodes[j];
				Int3 v0, v1, v2;
				node.GetVertices(out v0, out v1, out v2);
				vertices[j*3 + 0] = (Vector3)v0;
				vertices[j*3 + 1] = (Vector3)v1;
				vertices[j*3 + 2] = (Vector3)v2;

				var color = helper.NodeColor(node);
				colors[j*3 + 0] = colors[j*3 + 1] = colors[j*3 + 2] = color;
			}

			if (navmeshBase.showMeshSurface) helper.DrawTriangles(vertices, colors, tile.nodes.Length);
			if (navmeshBase.showMeshOutline) helper.DrawWireTriangles(vertices, colors, tile.nodes.Length);

			// Return lists to the pool
			ArrayPool<Vector3>.Release(ref vertices);
			ArrayPool<Color>.Release(ref colors);
		}

		/** Creates an outline of the navmesh for use in OnDrawGizmos in the editor */
		public static void CreateNavmeshOutlineVisualization (NavmeshTile tile, GraphGizmoHelper helper) {
			var sharedEdges = new bool[3];

			for (int j = 0; j < tile.nodes.Length; j++) {
				sharedEdges[0] = sharedEdges[1] = sharedEdges[2] = false;

				var node = tile.nodes[j];
				for (int c = 0; c < node.connections.Length; c++) {
					var other = node.connections[c].node as TriangleMeshNode;

					// Loop through neighbours to figure out which edges are shared
					if (other != null && other.GraphIndex == node.GraphIndex) {
						for (int v = 0; v < 3; v++) {
							for (int v2 = 0; v2 < 3; v2++) {
								if (node.GetVertexIndex(v) == other.GetVertexIndex((v2+1)%3) && node.GetVertexIndex((v+1)%3) == other.GetVertexIndex(v2)) {
									// Found a shared edge with the other node
									sharedEdges[v] = true;
									v = 3;
									break;
								}
							}
						}
					}
				}

				var color = helper.NodeColor(node);
				for (int v = 0; v < 3; v++) {
					if (!sharedEdges[v]) {
						helper.builder.DrawLine((Vector3)node.GetVertex(v), (Vector3)node.GetVertex((v+1)%3), color);
					}
				}
			}
		}

        public static void DrawUnwalkableNodes (this NavmeshBase navmeshBase, float size) {
            Gizmos.color = AstarColor.UnwalkableNode;
	        navmeshBase.GetNodes(node => {
                if (!node.Walkable) Gizmos.DrawCube((Vector3)node.position, Vector3.one*size);
            });
        }
	    
	    
	    public static IEnumerable<Progress> ScanAllTiles (this RecastGraph self) {
		    self.transform = self.CalculateTransform();
			self.InitializeTileInfo();

			// If this is true, just fill the graph with empty tiles
			if (self.scanEmptyGraph) {
				self.FillWithEmptyTiles();
				yield break;
			}

			// A walkableClimb higher than walkableHeight can cause issues when generating the navmesh since then it can in some cases
			// Both be valid for a character to walk under an obstacle and climb up on top of it (and that cannot be handled with navmesh without links)
			// The editor scripts also enforce this but we enforce it here too just to be sure
		    self.walkableClimb = Mathf.Min(self.walkableClimb, self.walkableHeight);

			yield return new Progress(0, "Finding Meshes");
			var bounds = self.transform.Transform(new Bounds(self.forcedBoundsSize*0.5f, self.forcedBoundsSize));
			var meshes = self.CollectMeshes(bounds);
			var buckets = self.PutMeshesIntoTileBuckets(meshes);

			Queue<Int2> tileQueue = new Queue<Int2>();

			// Put all tiles in the queue
			for (int z = 0; z < self.tileZCount; z++) {
				for (int x = 0; x < self.tileXCount; x++) {
					tileQueue.Enqueue(new Int2(x, z));
				}
			}

			var workQueue = new ParallelWorkQueue<Int2>(tileQueue);
			// Create the voxelizers and set all settings (one for each thread)
			var voxelizers = new Voxelize[workQueue.threadCount];
			for (int i = 0; i < voxelizers.Length; i++) voxelizers[i] = new Voxelize(self.CellHeight, self.cellSize, self.walkableClimb, self.walkableHeight, self.maxSlope, self.maxEdgeLength);
			workQueue.action = (tile, threadIndex) => {
				voxelizers[threadIndex].inputMeshes = buckets[tile.x + tile.y*self.tileXCount];
				self.tiles[tile.x + tile.y*self.tileXCount] = self.BuildTileMesh(voxelizers[threadIndex], tile.x, tile.y, threadIndex);
			};

			// Prioritize responsiveness while playing
			// but when not playing prioritize throughput
			// (the Unity progress bar is also pretty slow to update)
			int timeoutMillis = Application.isPlaying ? 1 : 200;

			// Scan all tiles in parallel
			foreach (var done in workQueue.Run(timeoutMillis)) {
				yield return new Progress(Mathf.Lerp(0.1f, 0.9f, done / (float)self.tiles.Length), "Calculated Tiles: " + done + "/" + self.tiles.Length);
			}

			yield return new Progress(0.9f, "Assigning Graph Indices");

			// Assign graph index to nodes
			uint graphIndex = (uint)AstarPath.active.data.GetGraphIndex(self);

		    self.GetNodes(node => node.GraphIndex = graphIndex);

			// First connect all tiles with an EVEN coordinate sum
			// This would be the white squares on a chess board.
			// Then connect all tiles with an ODD coordinate sum (which would be all black squares on a chess board).
			// This will prevent the different threads that do all
			// this in parallel from conflicting with each other.
			// The directions are also done separately
			// first they are connected along the X direction and then along the Z direction.
			// Looping over 0 and then 1
			for (int coordinateSum = 0; coordinateSum <= 1; coordinateSum++) {
				for (int direction = 0; direction <= 1; direction++) {
					for (int i = 0; i < self.tiles.Length; i++) {
						if ((self.tiles[i].x + self.tiles[i].z) % 2 == coordinateSum) {
							tileQueue.Enqueue(new Int2(self.tiles[i].x, self.tiles[i].z));
						}
					}

					workQueue = new ParallelWorkQueue<Int2>(tileQueue);
					workQueue.action = (tile, threadIndex) => {
						// Connect with tile at (x+1,z) and (x,z+1)
						if (direction == 0 && tile.x < self.tileXCount - 1)
							self.ConnectTiles(self.tiles[tile.x + tile.y * self.tileXCount], self.tiles[tile.x + 1 + tile.y * self.tileXCount]);
						if (direction == 1 && tile.y < self.tileZCount - 1)
							self.ConnectTiles(self.tiles[tile.x + tile.y * self.tileXCount], self.tiles[tile.x + (tile.y + 1) * self.tileXCount]);
					};

					var numTilesInQueue = tileQueue.Count;
					// Connect all tiles in parallel
					foreach (var done in workQueue.Run(timeoutMillis)) {
						yield return new Progress(0.95f, "Connected Tiles " + (numTilesInQueue - done) + "/" + numTilesInQueue + " (Phase " + (direction + 1 + 2*coordinateSum) + " of 4)");
					}
				}
			}

			for (int i = 0; i < meshes.Count; i++) meshes[i].Pool();
			ListPool<RasterizationMesh>.Release(ref meshes);

			// This may be used by the TileHandlerHelper script to update the tiles
			// while taking NavmeshCuts into account after the graph has been completely recalculated.
			if (self.OnRecalculatedTiles != null) {
				self.OnRecalculatedTiles(self.tiles.Clone() as NavmeshTile[]);
			}
		}
	    
	    /** Creates a list for every tile and adds every mesh that touches a tile to the corresponding list */
	    public static List<RasterizationMesh>[] PutMeshesIntoTileBuckets (this RecastGraph self, List<RasterizationMesh> meshes) {
		    var result = new List<RasterizationMesh>[self.tiles.Length];
		    var borderExpansion = new Vector3(1, 0, 1)*self.TileBorderSizeInWorldUnits*2;

		    for (int i = 0; i < result.Length; i++) {
			    result[i] = ListPool<RasterizationMesh>.Claim();
		    }

		    for (int i = 0; i < meshes.Count; i++) {
			    var mesh = meshes[i];
			    var bounds = mesh.bounds;
			    // Expand borderSize voxels on each side
			    bounds.Expand(borderExpansion);

			    var rect = self.GetTouchingTiles(bounds);
			    for (int z = rect.ymin; z <= rect.ymax; z++) {
				    for (int x = rect.xmin; x <= rect.xmax; x++) {
					    result[x + z*self.tileXCount].Add(mesh);
				    }
			    }
		    }

		    return result;
	    }
	    
	    public static List<RasterizationMesh> CollectMeshes (this RecastGraph self, Bounds bounds) {
		    Profiler.BeginSample("Find Meshes for rasterization");
		    var result = ListPool<RasterizationMesh>.Claim();

		    var meshGatherer = new RecastMeshGatherer(bounds, self.terrainSampleSize, self.mask, self.tagMask, self.colliderRasterizeDetail);

		    if (self.rasterizeMeshes) {
			    Profiler.BeginSample("Find meshes");
			    meshGatherer.CollectSceneMeshes(result);
			    Profiler.EndSample();
		    }

		    Profiler.BeginSample("Find RecastMeshObj components");
		    meshGatherer.CollectRecastMeshObjs(result);
		    Profiler.EndSample();

		    if (self.rasterizeTerrain) {
			    Profiler.BeginSample("Find terrains");
			    // Split terrains up into meshes approximately the size of a single chunk
			    var desiredTerrainChunkSize = self.cellSize*Math.Max(self.tileSizeX, self.tileSizeZ);
			    meshGatherer.CollectTerrainMeshes(self.rasterizeTrees, desiredTerrainChunkSize, result);
			    Profiler.EndSample();
		    }

		    if (self.rasterizeColliders) {
			    Profiler.BeginSample("Find colliders");
			    meshGatherer.CollectColliderMeshes(result);
			    Profiler.EndSample();
		    }

		    if (result.Count == 0) {
			    Debug.LogWarning("No MeshFilters were found contained in the layers specified by the 'mask' variables");
		    }

		    Profiler.EndSample();
		    return result;
	    }
	    
	    
		public static Bounds CalculateTileBoundsWithBorder (this RecastGraph self, int x, int z) {
			var bounds = new Bounds();

			bounds.SetMinMax(new Vector3(x*self.TileWorldSizeX, 0, z*self.TileWorldSizeZ),
				new Vector3((x+1)*self.TileWorldSizeX, self.forcedBoundsSize.y, (z+1)*self.TileWorldSizeZ)
				);

			// Expand borderSize voxels on each side
			bounds.Expand(new Vector3(1, 0, 1)*self.TileBorderSizeInWorldUnits*2);
			return bounds;
		}

		public static NavmeshTile BuildTileMesh (this RecastGraph self, Voxelize vox, int x, int z, int threadIndex = 0) {
			AstarProfiler.StartProfile("Build Tile");
			AstarProfiler.StartProfile("Init");

			vox.borderSize = self.TileBorderSizeInVoxels;
			vox.forcedBounds = self.CalculateTileBoundsWithBorder(x, z);
			vox.width = self.tileSizeX + vox.borderSize*2;
			vox.depth = self.tileSizeZ + vox.borderSize*2;

			if (!self.useTiles && self.relevantGraphSurfaceMode == RecastGraph.RelevantGraphSurfaceMode.OnlyForCompletelyInsideTile) {
				// This best reflects what the user would actually want
				vox.relevantGraphSurfaceMode = RecastGraph.RelevantGraphSurfaceMode.RequireForAll;
			} else {
				vox.relevantGraphSurfaceMode = self.relevantGraphSurfaceMode;
			}

			vox.minRegionSize = Mathf.RoundToInt(self.minRegionSize / (self.cellSize*self.cellSize));

			AstarProfiler.EndProfile("Init");


			// Init voxelizer
			vox.Init();
			vox.VoxelizeInput(self.transform, self.CalculateTileBoundsWithBorder(x, z));

			AstarProfiler.StartProfile("Filter Ledges");


			vox.FilterLedges(vox.voxelWalkableHeight, vox.voxelWalkableClimb, vox.cellSize, vox.cellHeight);

			AstarProfiler.EndProfile("Filter Ledges");

			AstarProfiler.StartProfile("Filter Low Height Spans");
			vox.FilterLowHeightSpans(vox.voxelWalkableHeight, vox.cellSize, vox.cellHeight);
			AstarProfiler.EndProfile("Filter Low Height Spans");

			vox.BuildCompactField();
			vox.BuildVoxelConnections();
			vox.ErodeWalkableArea(self.CharacterRadiusInVoxels);
			vox.BuildDistanceField();
			vox.BuildRegions();

			var cset = new VoxelContourSet();
			vox.BuildContours(self.contourMaxError, 1, cset, Voxelize.RC_CONTOUR_TESS_WALL_EDGES | Voxelize.RC_CONTOUR_TESS_TILE_EDGES);

			VoxelMesh mesh;
			vox.BuildPolyMesh(cset, 3, out mesh);

			AstarProfiler.StartProfile("Build Nodes");

			// Position the vertices correctly in graph space (all tiles are laid out on the xz plane with the (0,0) tile at the origin)
			for (int i = 0; i < mesh.verts.Length; i++) {
				mesh.verts[i] *= Int3.Precision;
			}
			vox.transformVoxel2Graph.Transform(mesh.verts);

			NavmeshTile tile = self.CreateTile(vox, mesh, x, z, threadIndex);

			AstarProfiler.EndProfile("Build Nodes");

			AstarProfiler.EndProfile("Build Tile");
			return tile;
		}

		/** Create a tile at tile index \a x, \a z from the mesh.
		 * \version Since version 3.7.6 the implementation is thread safe
		 */
		public static NavmeshTile CreateTile (this RecastGraph self, Voxelize vox, VoxelMesh mesh, int x, int z, int threadIndex) {
			if (mesh.tris == null) throw new System.ArgumentNullException("mesh.tris");
			if (mesh.verts == null) throw new System.ArgumentNullException("mesh.verts");
			if (mesh.tris.Length % 3 != 0) throw new System.ArgumentException("Indices array's length must be a multiple of 3 (mesh.tris)");
			if (mesh.verts.Length >= NavmeshBase.VertexIndexMask) {
				if (self.tileXCount*self.tileZCount == 1) {
					throw new System.ArgumentException("Too many vertices per tile (more than " + NavmeshBase.VertexIndexMask + ")." +
						"\n<b>Try enabling tiling in the recast graph settings.</b>\n");
				} else {
					throw new System.ArgumentException("Too many vertices per tile (more than " + NavmeshBase.VertexIndexMask + ")." +
						"\n<b>Try reducing tile size or enabling ASTAR_RECAST_LARGER_TILES under the 'Optimizations' tab in the A* Inspector</b>");
				}
			}

			// Create a new navmesh tile and assign its settings
			var tile = new NavmeshTile {
				x = x,
				z = z,
				w = 1,
				d = 1,
				tris = mesh.tris,
				bbTree = new BBTree(),
				graph = self,
			};

			tile.vertsInGraphSpace = Utility.RemoveDuplicateVertices(mesh.verts, tile.tris);
			tile.verts = (Int3[])tile.vertsInGraphSpace.Clone();
			self.transform.Transform(tile.verts);

			// Here we are faking a new graph
			// The tile is not added to any graphs yet, but to get the position queries from the nodes
			// to work correctly (not throw exceptions because the tile is not calculated) we fake a new graph
			// and direct the position queries directly to the tile
			// The thread index is added to make sure that if multiple threads are calculating tiles at the same time
			// they will not use the same temporary graph index
			uint temporaryGraphIndex = (uint)(AstarPath.active.data.graphs.Length + threadIndex);

			if (temporaryGraphIndex > GraphNode.MaxGraphIndex) {
				// Multithreaded tile calculations use fake graph indices, see above.
				throw new System.Exception("Graph limit reached. Multithreaded recast calculations cannot be done because a few scratch graph indices are required.");
			}

			TriangleMeshNode.SetNavmeshHolder((int)temporaryGraphIndex, tile);
			// We need to lock here because creating nodes is not thread safe
			// and we may be doing this from multiple threads at the same time
			tile.nodes = new TriangleMeshNode[tile.tris.Length/3];
			lock (AstarPath.active) {
				self.CreateNodes(tile.nodes, tile.tris, x + z*self.tileXCount, temporaryGraphIndex);
			}

			tile.bbTree.RebuildFrom(tile.nodes);
			NavmeshBase.CreateNodeConnections(tile.nodes);
			// Remove the fake graph
			TriangleMeshNode.SetNavmeshHolder((int)temporaryGraphIndex, null);

			return tile;
		}
	    
	    /** Changes the bounds of the graph to precisely encapsulate all objects in the scene that can be included in the scanning process based on the settings.
 * Which objects are used depends on the settings. If an object would have affected the graph with the current settings if it would have
 * been inside the bounds of the graph, it will be detected and the bounds will be expanded to contain that object.
 *
 * This method corresponds to the 'Snap bounds to scene' button in the inspector.
 *
 * \see rasterizeMeshes
 * \see rasterizeTerrain
 * \see rasterizeColliders
 * \see mask
 * \see tagMask
 *
 * \see forcedBoundsCenter
 * \see forcedBoundsSize
 */
	    public static void SnapForceBoundsToScene (this RecastGraph self) {
		    var meshes = self.CollectMeshes(new Bounds(Vector3.zero, new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity)));

		    if (meshes.Count == 0) {
			    return;
		    }

		    var bounds = meshes[0].bounds;

		    for (int i = 1; i < meshes.Count; i++) {
			    bounds.Encapsulate(meshes[i].bounds);
			    meshes[i].Pool();
		    }

		    self.forcedBoundsCenter = bounds.center;
		    self.forcedBoundsSize = bounds.size;
	    }
	    
	    
		public static Bounds Transform (this GraphTransform self, Bounds bounds) {
			if (self.onlyTranslational) return new Bounds(bounds.center + self.translation.ToUnityV3(), bounds.size);

			var corners = ArrayPool<Vector3>.Claim(8);
			var extents = bounds.extents;
			corners[0] = self.Transform(bounds.center + new Vector3(extents.x, extents.y, extents.z));
			corners[1] = self.Transform(bounds.center + new Vector3(extents.x, extents.y, -extents.z));
			corners[2] = self.Transform(bounds.center + new Vector3(extents.x, -extents.y, extents.z));
			corners[3] = self.Transform(bounds.center + new Vector3(extents.x, -extents.y, -extents.z));
			corners[4] = self.Transform(bounds.center + new Vector3(-extents.x, extents.y, extents.z));
			corners[5] = self.Transform(bounds.center + new Vector3(-extents.x, extents.y, -extents.z));
			corners[6] = self.Transform(bounds.center + new Vector3(-extents.x, -extents.y, extents.z));
			corners[7] = self.Transform(bounds.center + new Vector3(-extents.x, -extents.y, -extents.z));

			var min = corners[0];
			var max = corners[0];
			for (int i = 1; i < 8; i++) {
				min = Vector3.Min(min, corners[i]);
				max = Vector3.Max(max, corners[i]);
			}
			ArrayPool<Vector3>.Release(ref corners);
			return new Bounds((min+max)*0.5f, max - min);
		}

		public static Bounds InverseTransform (this GraphTransform self, Bounds bounds) {
			if (self.onlyTranslational) return new Bounds(bounds.center - self.translation.ToUnityV3(), bounds.size);

			var corners = ArrayPool<Vector3>.Claim(8);
			var extents = bounds.extents;
			corners[0] = self.InverseTransform(bounds.center + new Vector3(extents.x, extents.y, extents.z));
			corners[1] = self.InverseTransform(bounds.center + new Vector3(extents.x, extents.y, -extents.z));
			corners[2] = self.InverseTransform(bounds.center + new Vector3(extents.x, -extents.y, extents.z));
			corners[3] = self.InverseTransform(bounds.center + new Vector3(extents.x, -extents.y, -extents.z));
			corners[4] = self.InverseTransform(bounds.center + new Vector3(-extents.x, extents.y, extents.z));
			corners[5] = self.InverseTransform(bounds.center + new Vector3(-extents.x, extents.y, -extents.z));
			corners[6] = self.InverseTransform(bounds.center + new Vector3(-extents.x, -extents.y, extents.z));
			corners[7] = self.InverseTransform(bounds.center + new Vector3(-extents.x, -extents.y, -extents.z));

			var min = corners[0];
			var max = corners[0];
			for (int i = 1; i < 8; i++) {
				min = Vector3.Min(min, corners[i]);
				max = Vector3.Max(max, corners[i]);
			}
			ArrayPool<Vector3>.Release(ref corners);
			return new Bounds((min+max)*0.5f, max - min);
		}
	    
	    /** Returns a rect containing the indices of all tiles touching the specified bounds */
	    public static IntRect GetTouchingTiles (this NavmeshBase self, Bounds bounds) {
		    bounds = self.transform.InverseTransform(bounds);

		    // Calculate world bounds of all affected tiles
		    var r = new IntRect(Mathf.FloorToInt(bounds.min.x / self.TileWorldSizeX), Mathf.FloorToInt(bounds.min.z / self.TileWorldSizeZ), Mathf.FloorToInt(bounds.max.x / self.TileWorldSizeX), Mathf.FloorToInt(bounds.max.z / self.TileWorldSizeZ));
		    // Clamp to bounds
		    r = IntRect.Intersection(r, new IntRect(0, 0, self.tileXCount-1, self.tileZCount-1));
		    return r;
	    }

	    /** Returns a rect containing the indices of all tiles touching the specified bounds.
	     * \param rect Graph space rectangle (in graph space all tiles are on the XZ plane regardless of graph rotation and other transformations, the first tile has a corner at the origin)
	     */
	    public static IntRect GetTouchingTilesInGraphSpace (this NavmeshBase self, Rect rect) {
		    // Calculate world bounds of all affected tiles
		    var r = new IntRect(Mathf.FloorToInt(rect.xMin / self.TileWorldSizeX), Mathf.FloorToInt(rect.yMin / self.TileWorldSizeZ), Mathf.FloorToInt(rect.xMax / self.TileWorldSizeX), Mathf.FloorToInt(rect.yMax / self.TileWorldSizeZ));

		    // Clamp to bounds
		    r = IntRect.Intersection(r, new IntRect(0, 0, self.tileXCount-1, self.tileZCount-1));
		    return r;
	    }
	    
	    /** True if the matrix will reverse orientations of faces.
		 *
		 * Scaling by a negative value along an odd number of axes will reverse
		 * the orientation of e.g faces on a mesh. This must be counter adjusted
		 * by for example the recast rasterization system to be able to handle
		 * meshes with negative scales properly.
		 *
		 * We can find out if they are flipped by finding out how the signed
		 * volume of a unit cube is transformed when applying the matrix
		 *
		 * If the (signed) volume turns out to be negative
		 * that also means that the orientation of it has been reversed.
		 *
		 * \see https://en.wikipedia.org/wiki/Normal_(geometry)
		 * \see https://en.wikipedia.org/wiki/Parallelepiped
		 */
		public static bool ReversesFaceOrientations (Matrix4x4 matrix) {
			var dX = matrix.MultiplyVector(new Vector3(1, 0, 0));
			var dY = matrix.MultiplyVector(new Vector3(0, 1, 0));
			var dZ = matrix.MultiplyVector(new Vector3(0, 0, 1));

			// Calculate the signed volume of the parallelepiped
			var volume = Vector3.Dot(Vector3.Cross(dX, dY), dZ);

			return volume < 0;
		}

		/** True if the matrix will reverse orientations of faces in the XZ plane.
		 * Almost the same as ReversesFaceOrientations, but this method assumes
		 * that scaling a face with a negative scale along the Y axis does not
		 * reverse the orientation of the face.
		 *
		 * This is used for navmesh cuts.
		 *
		 * Scaling by a negative value along one axis or rotating
		 * it so that it is upside down will reverse
		 * the orientation of the cut, so we need to be reverse
		 * it again as a countermeasure.
		 * However if it is flipped along two axes it does not need to
		 * be reversed.
		 * We can handle all these cases by finding out how a unit square formed
		 * by our forward axis and our rightward axis is transformed in XZ space
		 * when applying the local to world matrix.
		 * If the (signed) area of the unit square turns out to be negative
		 * that also means that the orientation of it has been reversed.
		 * The signed area is calculated using a cross product of the vectors.
		 */
		public static bool ReversesFaceOrientationsXZ (Matrix4x4 matrix) {
			var dX = matrix.MultiplyVector(new Vector3(1, 0, 0));
			var dZ = matrix.MultiplyVector(new Vector3(0, 0, 1));

			// Take the cross product of the vectors projected onto the XZ plane
			var cross = (dX.x*dZ.z - dZ.x*dX.z);

			return cross < 0;
		}
	    
	    static int Bit (int a, int b) {
		    return (a >> b) & 1;
	    }
	    
	    public static Color IntToColor (int i, float a) {
		    int r = Bit(i, 2) + Bit(i, 3) * 2 + 1;
		    int g = Bit(i, 1) + Bit(i, 4) * 2 + 1;
		    int b = Bit(i, 0) + Bit(i, 5) * 2 + 1;

		    return new Color(r*0.25F, g*0.25F, b*0.25F, a);
	    }

	    /**
	     * Converts an HSV color to an RGB color.
	     * According to the algorithm described at http://en.wikipedia.org/wiki/HSL_and_HSV
	     *
	     * @author Wikipedia
	     * @return the RGB representation of the color.
	     */
	    public static Color HSVToRGB (float h, float s, float v) {
		    float r = 0, g = 0, b = 0;

		    float Chroma = s * v;
		    float Hdash = h / 60.0f;
		    float X = Chroma * (1.0f - System.Math.Abs((Hdash % 2.0f) - 1.0f));

		    if (Hdash < 1.0f) {
			    r = Chroma;
			    g = X;
		    } else if (Hdash < 2.0f) {
			    r = X;
			    g = Chroma;
		    } else if (Hdash < 3.0f) {
			    g = Chroma;
			    b = X;
		    } else if (Hdash < 4.0f) {
			    g = X;
			    b = Chroma;
		    } else if (Hdash < 5.0f) {
			    r = X;
			    b = Chroma;
		    } else if (Hdash < 6.0f) {
			    r = Chroma;
			    b = X;
		    }

		    float Min = v - Chroma;

		    r += Min;
		    g += Min;
		    b += Min;

		    return new Color(r, g, b);
	    }
    }
}