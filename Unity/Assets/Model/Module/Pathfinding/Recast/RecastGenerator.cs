using System.Collections.Generic;

namespace PF {

	/** Automatically generates navmesh graphs based on world geometry.
	 * The recast graph is based on Recast (http://code.google.com/p/recastnavigation/).\n
	 * I have translated a good portion of it to C# to run it natively in Unity.
	 *
	 * \section howitworks How a recast graph works
	 * When generating a recast graph what happens is that the world is voxelized.
	 * You can think of this as constructing an approximation of the world out of lots of boxes.
	 * If you have played Minecraft it looks very similar (but with smaller boxes).
	 * \shadowimage{recast/voxelized_truck.jpg}
	 *
	 * The Recast process is described as follows:
	 * - The voxel mold is build from the input triangle mesh by rasterizing the triangles into a multi-layer heightfield.
	 * Some simple filters are then applied to the mold to prune out locations where the character would not be able to move.
	 * - The walkable areas described by the mold are divided into simple overlayed 2D regions.
	 * The resulting regions have only one non-overlapping contour, which simplifies the final step of the process tremendously.
	 * - The navigation polygons are peeled off from the regions by first tracing the boundaries and then simplifying them.
	 * The resulting polygons are finally converted to convex polygons which makes them perfect for pathfinding and spatial reasoning about the level.
	 *
	 * It works exactly like that in the C# version as well, except that everything is triangulated to triangles instead of n-gons.
	 * The recast generation process usually works directly on the visiable geometry in the world, this is usually a good thing, because world geometry is usually more detailed than the colliders.
	 * You can however specify that colliders should be rasterized, if you have very detailed world geometry, this can speed up the scan.
	 *
	 * Check out the second part of the Get Started Tutorial which discusses recast graphs.
	 *
	 * \section export Exporting for manual editing
	 * In the editor there is a button for exporting the generated graph to a .obj file.
	 * Usually the generation process is good enough for the game directly, but in some cases you might want to edit some minor details.
	 * So you can export the graph to a .obj file, open it in your favourite 3D application, edit it, and export it to a mesh which Unity can import.
	 * You can then use that mesh in a navmesh graph.
	 *
	 * Since many 3D modelling programs use different axis systems (unity uses X=right, Y=up, Z=forward), it can be a bit tricky to get the rotation and scaling right.
	 * For blender for example, what you have to do is to first import the mesh using the .obj importer. Don't change anything related to axes in the settings.
	 * Then select the mesh, open the transform tab (usually the thin toolbar to the right of the 3D view) and set Scale -> Z to -1.
	 * If you transform it using the S (scale) hotkey, it seems to set both Z and Y to -1 for some reason.
	 * Then make the edits you need and export it as an .obj file to somewhere in the Unity project.
	 * But this time, edit the setting named "Forward" to "Z forward" (not -Z as it is per default).
	 *
	 * \shadowimage{recastgraph_graph.png}
	 * \shadowimage{recastgraph_inspector.png}
	 *
	 * \ingroup graphs
	 *
	 * \astarpro
	 */
	[JsonOptIn]
	public class RecastGraph : NavmeshBase {
		[JsonMember]
		/** Radius of the agent which will traverse the navmesh.
		 * The navmesh will be eroded with this radius.
		 * \shadowimage{recast/character_radius.gif}
		 */
		public float characterRadius = 1.5F;

		/** Max distance from simplified edge to real edge.
		 * This value is measured in voxels. So with the default value of 2 it means that the final navmesh contour may be at most
		 * 2 voxels (i.e 2 times #cellSize) away from the border that was calculated when voxelizing the world.
		 * A higher value will yield a more simplified and cleaner navmesh while a lower value may capture more details.
		 * However a too low value will cause the individual voxels to be visible (see image below).
		 *
		 * \shadowimage{recast/max_edge_error.gif}
		 *
		 * \see #cellSize
		 */
		[JsonMember]
		public float contourMaxError = 2F;

		/** Voxel sample size (x,z).
		 * When generating a recast graph what happens is that the world is voxelized.
		 * You can think of this as constructing an approximation of the world out of lots of boxes.
		 * If you have played Minecraft it looks very similar (but with smaller boxes).
		 * \shadowimage{recast/voxelized_truck.jpg}
		 * The cell size is the width and depth of those boxes. The height of the boxes is usually much smaller
		 * and automatically calculated however. See #CellHeight.
		 *
		 * Lower values will yield higher quality navmeshes, however the graph will be slower to scan.
		 *
		 * \shadowimage{recast/cell_size.gif}
		 */
		[JsonMember]
		public float cellSize = 0.5F;

		/** Character height.
		 * \shadowimage{recast/walkable_height.gif}
		 */
		[JsonMember]
		public float walkableHeight = 2F;

		/** Height the character can climb.
		 * \shadowimage{recast/walkable_climb.gif}
		 */
		[JsonMember]
		public float walkableClimb = 0.5F;

		/** Max slope in degrees the character can traverse.
		 * \shadowimage{recast/max_slope.gif}
		 */
		[JsonMember]
		public float maxSlope = 30;

		/** Longer edges will be subdivided.
		 * Reducing this value can sometimes improve path quality since similarly sized triangles
		 * yield better paths than really large and really triangles small next to each other.
		 * However it will also add a lot more nodes which will make pathfinding slower.
		 * For more information about this take a look at \ref navmeshnotes.
		 *
		 * \shadowimage{recast/max_edge_length.gif}
		 */
		[JsonMember]
		public float maxEdgeLength = 20;

		/** Minumum region size.
		 * Small regions will be removed from the navmesh.
		 * Measured in square world units (square meters in most games).
		 *
		 * \shadowimage{recast/min_region_size.gif}
		 *
		 * If a region is adjacent to a tile border, it will not be removed
		 * even though it is small since the adjacent tile might join it
		 * to form a larger region.
		 *
		 * \shadowimage{recast_minRegionSize_1.png}
		 * \shadowimage{recast_minRegionSize_2.png}
		 */
		[JsonMember]
		public float minRegionSize = 3;

		/** Size in voxels of a single tile.
		 * This is the width of the tile.
		 *
		 * \shadowimage{recast/tile.png}
		 *
		 * A large tile size can be faster to initially scan (but beware of out of memory issues if you try with a too large tile size in a large world)
		 * smaller tile sizes are (much) faster to update.
		 *
		 * Different tile sizes can affect the quality of paths. It is often good to split up huge open areas into several tiles for
		 * better quality paths, but too small tiles can also lead to effects looking like invisible obstacles.
		 * For more information about this take a look at \ref navmeshnotes.
		 * Usually it is best to experiment and see what works best for your game.
		 *
		 * When scanning a recast graphs individual tiles can be calculated in parallel which can make it much faster to scan large worlds.
		 * When you want to recalculate a part of a recast graph, this can only be done on a tile-by-tile basis which means that if you often try to update a region
		 * of the recast graph much smaller than the tile size, then you will be doing a lot of unnecessary calculations. However if you on the other hand
		 * update regions of the recast graph that are much larger than the tile size then it may be slower than necessary as there is some overhead in having lots of tiles
		 * instead of a few larger ones (not that much though).
		 *
		 * Recommended values are between 64 and 256, but these are very soft limits. It is possible to use both larger and smaller values.
		 */
		[JsonMember]
		public int editorTileSize = 128;

		/** Size of a tile along the X axis in voxels.
		 * \copydetails editorTileSize
		 *
		 * \warning Do not modify, it is set from #editorTileSize at Scan
		 *
		 * \see #tileSizeZ
		 */
		[JsonMember]
		public int tileSizeX = 128;

		/** Size of a tile along the Z axis in voxels.
		 * \copydetails editorTileSize
		 *
		 * \warning Do not modify, it is set from #editorTileSize at Scan
		 *
		 * \see #tileSizeX
		 */
		[JsonMember]
		public int tileSizeZ = 128;


		/** If true, divide the graph into tiles, otherwise use a single tile covering the whole graph.
		 * \since Since 4.1 the default value is \a true.
		 */
		[JsonMember]
		public bool useTiles = true;

		/** If true, scanning the graph will yield a completely empty graph.
		 * Useful if you want to replace the graph with a custom navmesh for example
		 */
		public bool scanEmptyGraph;

		public enum RelevantGraphSurfaceMode {
			/** No RelevantGraphSurface components are required anywhere */
			DoNotRequire,
			/** Any surfaces that are completely inside tiles need to have a \link Pathfinding.RelevantGraphSurface RelevantGraphSurface\endlink component
			 * positioned on that surface, otherwise it will be stripped away.
			 */
			OnlyForCompletelyInsideTile,
			/** All surfaces need to have one \link Pathfinding.RelevantGraphSurface RelevantGraphSurface\endlink component
			 * positioned somewhere on the surface and in each tile that it touches, otherwise it will be stripped away.
			 * Only tiles that have a RelevantGraphSurface component for that surface will keep it.
			 */
			RequireForAll
		}

		/** Require every region to have a RelevantGraphSurface component inside it.
		 * A RelevantGraphSurface component placed in the scene specifies that
		 * the navmesh region it is inside should be included in the navmesh.
		 *
		 * If this is set to OnlyForCompletelyInsideTile
		 * a navmesh region is included in the navmesh if it
		 * has a RelevantGraphSurface inside it, or if it
		 * is adjacent to a tile border. This can leave some small regions
		 * which you didn't want to have included because they are adjacent
		 * to tile borders, but it removes the need to place a component
		 * in every single tile, which can be tedious (see below).
		 *
		 * If this is set to RequireForAll
		 * a navmesh region is included only if it has a RelevantGraphSurface
		 * inside it. Note that even though the navmesh
		 * looks continous between tiles, the tiles are computed individually
		 * and therefore you need a RelevantGraphSurface component for each
		 * region and for each tile.
		 *
		 *
		 *
		 * \shadowimage{relevantgraphsurface/dontreq.png}
		 * In the above image, the mode OnlyForCompletelyInsideTile was used. Tile borders
		 * are highlighted in black. Note that since all regions are adjacent to a tile border,
		 * this mode didn't remove anything in this case and would give the same result as DoNotRequire.
		 * The RelevantGraphSurface component is shown using the green gizmo in the top-right of the blue plane.
		 *
		 * \shadowimage{relevantgraphsurface/no_tiles.png}
		 * In the above image, the mode RequireForAll was used. No tiles were used.
		 * Note that the small region at the top of the orange cube is now gone, since it was not the in the same
		 * region as the relevant graph surface component.
		 * The result would have been identical with OnlyForCompletelyInsideTile since there are no tiles (or a single tile, depending on how you look at it).
		 *
		 * \shadowimage{relevantgraphsurface/req_all.png}
		 * The mode RequireForAll was used here. Since there is only a single RelevantGraphSurface component, only the region
		 * it was in, in the tile it is placed in, will be enabled. If there would have been several RelevantGraphSurface in other tiles,
		 * those regions could have been enabled as well.
		 *
		 * \shadowimage{relevantgraphsurface/tiled_uneven.png}
		 * Here another tile size was used along with the OnlyForCompletelyInsideTile.
		 * Note that the region on top of the orange cube is gone now since the region borders do not intersect that region (and there is no
		 * RelevantGraphSurface component inside it).
		 *
		 * \note When not using tiles. OnlyForCompletelyInsideTile is equivalent to RequireForAll.
		 */
		[JsonMember]
		public RelevantGraphSurfaceMode relevantGraphSurfaceMode = RelevantGraphSurfaceMode.DoNotRequire;

		[JsonMember]
		/** Use colliders to calculate the navmesh */
		public bool rasterizeColliders;

		[JsonMember]
		/** Use scene meshes to calculate the navmesh */
		public bool rasterizeMeshes = true;

		/** Include the Terrain in the scene. */
		[JsonMember]
		public bool rasterizeTerrain = true;

		/** Rasterize tree colliders on terrains.
		 *
		 * If the tree prefab has a collider, that collider will be rasterized.
		 * Otherwise a simple box collider will be used and the script will
		 * try to adjust it to the tree's scale, it might not do a very good job though so
		 * an attached collider is preferable.
		 *
		 * \note It seems that Unity will only generate tree colliders at runtime when the game is started.
		 * For this reason, this graph will not pick up tree colliders when scanned outside of play mode
		 * but it will pick them up if the graph is scanned when the game has started. If it still does not pick them up
		 * make sure that the trees actually have colliders attached to them and that the tree prefabs are
		 * in the correct layer (the layer should be included in the layer mask).
		 *
		 * \see rasterizeTerrain
		 * \see colliderRasterizeDetail
		 */
		[JsonMember]
		public bool rasterizeTrees = true;

		/** Controls detail on rasterization of sphere and capsule colliders.
		 * This controls the number of rows and columns on the generated meshes.
		 * A higher value does not necessarily increase quality of the mesh, but a lower
		 * value will often speed it up.
		 *
		 * You should try to keep this value as low as possible without affecting the mesh quality since
		 * that will yield the fastest scan times.
		 *
		 * \see rasterizeColliders
		 */
		[JsonMember]
		public float colliderRasterizeDetail = 10;

		/** Layer mask which filters which objects to include.
		 * \see tagMask
		 */
		[JsonMember]
		public int mask = -1;

		/** Objects tagged with any of these tags will be rasterized.
		 * Note that this extends the layer mask, so if you only want to use tags, set #mask to 'Nothing'.
		 *
		 * \see mask
		 */
		[JsonMember]
		public List<string> tagMask = new List<string>();

		/** Controls how large the sample size for the terrain is.
		 * A higher value is faster to scan but less accurate
		 */
		[JsonMember]
		public int terrainSampleSize = 3;

		/** Rotation of the graph in degrees */
		[JsonMember]
		public Vector3 rotation;

		/** Center of the bounding box.
		 * Scanning will only be done inside the bounding box */
		[JsonMember]
		public Vector3 forcedBoundsCenter;

		public const int BorderVertexMask = 1;
		public const int BorderVertexOffset = 31;

		protected override bool RecalculateNormals { get { return true; } }

		public override float TileWorldSizeX {
			get {
				return tileSizeX*cellSize;
			}
		}

		public override float TileWorldSizeZ {
			get {
				return tileSizeZ*cellSize;
			}
		}

		protected override float MaxTileConnectionEdgeDistance {
			get {
				return walkableClimb;
			}
		}
		
		public override GraphTransform CalculateTransform () {
			return new GraphTransform(Matrix4x4.TRS(forcedBoundsCenter, Quaternion.Euler(rotation), Vector3.one) * Matrix4x4.TRS(-forcedBoundsSize*0.5f, Quaternion.identity, Vector3.one));
		}
		
		public void InitializeTileInfo () {
			// Voxel grid size
			int totalVoxelWidth = (int)(forcedBoundsSize.x/cellSize + 0.5f);
			int totalVoxelDepth = (int)(forcedBoundsSize.z/cellSize + 0.5f);

			if (!useTiles) {
				tileSizeX = totalVoxelWidth;
				tileSizeZ = totalVoxelDepth;
			} else {
				tileSizeX = editorTileSize;
				tileSizeZ = editorTileSize;
			}

			// Number of tiles
			tileXCount = (totalVoxelWidth + tileSizeX-1) / tileSizeX;
			tileZCount = (totalVoxelDepth + tileSizeZ-1) / tileSizeZ;

			if (tileXCount * tileZCount > TileIndexMask+1) {
				throw new System.Exception("Too many tiles ("+(tileXCount * tileZCount)+") maximum is "+(TileIndexMask+1)+
					"\nTry disabling ASTAR_RECAST_LARGER_TILES under the 'Optimizations' tab in the A* inspector.");
			}

			tiles = new NavmeshTile[tileXCount*tileZCount];
		}





		public float CellHeight {
			get {
				// Voxel y coordinates will be stored as ushorts which have 65536 values
				// Leave a margin to make sure things do not overflow
				return Mathf.Max(forcedBoundsSize.y / 64000, 0.001f);
			}
		}

		/** Convert character radius to a number of voxels */
		public int CharacterRadiusInVoxels {
			get {
				// Round it up most of the time, but round it down
				// if it is very close to the result when rounded down
				return Mathf.CeilToInt((characterRadius / cellSize) - 0.1f);
			}
		}

		/** Number of extra voxels on each side of a tile to ensure accurate navmeshes near the tile border.
		 * The width of a tile is expanded by 2 times this value (1x to the left and 1x to the right)
		 */
		public int TileBorderSizeInVoxels {
			get {
				return CharacterRadiusInVoxels + 3;
			}
		}

		public float TileBorderSizeInWorldUnits {
			get {
				return TileBorderSizeInVoxels*cellSize;
			}
		}

		protected override void DeserializeSettingsCompatibility (GraphSerializationContext ctx) {
			base.DeserializeSettingsCompatibility(ctx);

			characterRadius = ctx.reader.ReadSingle();
			contourMaxError = ctx.reader.ReadSingle();
			cellSize = ctx.reader.ReadSingle();
			ctx.reader.ReadSingle(); // Backwards compatibility, cellHeight was previously read here
			walkableHeight = ctx.reader.ReadSingle();
			maxSlope = ctx.reader.ReadSingle();
			maxEdgeLength = ctx.reader.ReadSingle();
			editorTileSize = ctx.reader.ReadInt32();
			tileSizeX = ctx.reader.ReadInt32();
			nearestSearchOnlyXZ = ctx.reader.ReadBoolean();
			useTiles = ctx.reader.ReadBoolean();
			relevantGraphSurfaceMode = (RelevantGraphSurfaceMode)ctx.reader.ReadInt32();
			rasterizeColliders = ctx.reader.ReadBoolean();
			rasterizeMeshes = ctx.reader.ReadBoolean();
			rasterizeTerrain = ctx.reader.ReadBoolean();
			rasterizeTrees = ctx.reader.ReadBoolean();
			colliderRasterizeDetail = ctx.reader.ReadSingle();
			forcedBoundsCenter = ctx.DeserializeVector3();
			forcedBoundsSize = ctx.DeserializeVector3();
			mask = ctx.reader.ReadInt32();

			int count = ctx.reader.ReadInt32();
			tagMask = new List<string>(count);
			for (int i = 0; i < count; i++) {
				tagMask.Add(ctx.reader.ReadString());
			}

			showMeshOutline = ctx.reader.ReadBoolean();
			showNodeConnections = ctx.reader.ReadBoolean();
			terrainSampleSize = ctx.reader.ReadInt32();

			// These were originally forgotten but added in an upgrade
			// To keep backwards compatibility, they are only deserialized
			// If they exist in the streamed data
			walkableClimb = ctx.DeserializeFloat(walkableClimb);
			minRegionSize = ctx.DeserializeFloat(minRegionSize);

			// Make the world square if this value is not in the stream
			tileSizeZ = ctx.DeserializeInt(tileSizeX);

			showMeshSurface = ctx.reader.ReadBoolean();
		}
	}
}
