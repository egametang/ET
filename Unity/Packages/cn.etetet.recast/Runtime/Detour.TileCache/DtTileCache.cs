/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023 Choi Ikpil ikpil@naver.com

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.
Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:
1. The origin of this software must not be misrepresented; you must not
 claim that you wrote the original software. If you use this software
 in a product, an acknowledgment in the product documentation would be
 appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
 misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Detour.TileCache.Io;

namespace DotRecast.Detour.TileCache
{
    public class DtTileCache
    {
        private int m_tileLutSize; // < Tile hash lookup size (must be pot).
        private int m_tileLutMask; // < Tile hash lookup mask.
        private readonly DtCompressedTile[] m_posLookup; // < Tile hash lookup.

        private DtCompressedTile m_nextFreeTile; // < Freelist of tiles.
        private readonly DtCompressedTile[] m_tiles; // < List of tiles. // TODO: (PP) replace with list

        private readonly int m_saltBits; // < Number of salt bits in the tile ID.
        private readonly int m_tileBits; // < Number of tile bits in the tile ID.
        private readonly DtNavMesh m_navmesh;

        private readonly DtTileCacheParams m_params;
        private readonly DtTileCacheStorageParams m_storageParams;

        private readonly IRcCompressor m_tcomp;
        private readonly IDtTileCacheMeshProcess m_tmproc;

        private readonly List<DtTileCacheObstacle> m_obstacles = new List<DtTileCacheObstacle>();
        private DtTileCacheObstacle m_nextFreeObstacle;

        private readonly List<ObstacleRequest> m_reqs = new List<ObstacleRequest>();
        private readonly List<long> m_update = new List<long>();

        private readonly DtTileCacheBuilder builder = new DtTileCacheBuilder();
        private readonly DtTileCacheLayerHeaderReader tileReader = new DtTileCacheLayerHeaderReader();

        public DtTileCache(DtTileCacheParams option, DtTileCacheStorageParams storageParams, DtNavMesh navmesh, IRcCompressor tcomp, IDtTileCacheMeshProcess tmprocs)
        {
            m_params = option;
            m_storageParams = storageParams;
            m_navmesh = navmesh;
            m_tcomp = tcomp;
            m_tmproc = tmprocs;

            m_tileLutSize = DtUtils.NextPow2(m_params.maxTiles / 4);
            if (m_tileLutSize == 0)
            {
                m_tileLutSize = 1;
            }

            m_tileLutMask = m_tileLutSize - 1;
            m_tiles = new DtCompressedTile[m_params.maxTiles];
            m_posLookup = new DtCompressedTile[m_tileLutSize];
            for (int i = m_params.maxTiles - 1; i >= 0; --i)
            {
                m_tiles[i] = new DtCompressedTile(i);
                m_tiles[i].next = m_nextFreeTile;
                m_nextFreeTile = m_tiles[i];
            }

            m_tileBits = DtUtils.Ilog2(DtUtils.NextPow2(m_params.maxTiles));
            m_saltBits = Math.Min(31, 32 - m_tileBits);
            if (m_saltBits < 10)
            {
                throw new Exception("Too few salt bits: " + m_saltBits);
            }
        }

        private bool Contains(List<long> a, long v)
        {
            return a.Contains(v);
        }

        /// Encodes a tile id.
        private long EncodeTileId(int salt, int it)
        {
            return ((long)salt << m_tileBits) | (long)it;
        }

        /// Decodes a tile salt.
        private int DecodeTileIdSalt(long refs)
        {
            long saltMask = (1L << m_saltBits) - 1;
            return (int)((refs >> m_tileBits) & saltMask);
        }

        /// Decodes a tile id.
        private int DecodeTileIdTile(long refs)
        {
            long tileMask = (1L << m_tileBits) - 1;
            return (int)(refs & tileMask);
        }

        /// Encodes an obstacle id.
        private long EncodeObstacleId(int salt, int it)
        {
            return ((long)salt << 16) | (long)it;
        }

        /// Decodes an obstacle salt.
        private int DecodeObstacleIdSalt(long refs)
        {
            long saltMask = ((long)1 << 16) - 1;
            return (int)((refs >> 16) & saltMask);
        }

        /// Decodes an obstacle id.
        private int DecodeObstacleIdObstacle(long refs)
        {
            long tileMask = ((long)1 << 16) - 1;
            return (int)(refs & tileMask);
        }


        public DtCompressedTile GetTileByRef(long refs)
        {
            if (refs == 0)
            {
                return null;
            }

            int tileIndex = DecodeTileIdTile(refs);
            int tileSalt = DecodeTileIdSalt(refs);
            if (tileIndex >= m_params.maxTiles)
            {
                return null;
            }

            DtCompressedTile tile = m_tiles[tileIndex];
            if (tile.salt != tileSalt)
            {
                return null;
            }

            return tile;
        }

        public List<long> GetTilesAt(int tx, int ty)
        {
            List<long> tiles = new List<long>();

            // Find tile based on hash.
            int h = DtNavMesh.ComputeTileHash(tx, ty, m_tileLutMask);
            DtCompressedTile tile = m_posLookup[h];
            while (tile != null)
            {
                if (tile.header != null && tile.header.tx == tx && tile.header.ty == ty)
                {
                    tiles.Add(GetTileRef(tile));
                }

                tile = tile.next;
            }

            return tiles;
        }

        DtCompressedTile GetTileAt(int tx, int ty, int tlayer)
        {
            // Find tile based on hash.
            int h = DtNavMesh.ComputeTileHash(tx, ty, m_tileLutMask);
            DtCompressedTile tile = m_posLookup[h];
            while (tile != null)
            {
                if (tile.header != null && tile.header.tx == tx && tile.header.ty == ty && tile.header.tlayer == tlayer)
                {
                    return tile;
                }

                tile = tile.next;
            }

            return null;
        }

        public long GetTileRef(DtCompressedTile tile)
        {
            if (tile == null)
            {
                return 0;
            }

            int it = tile.index;
            return EncodeTileId(tile.salt, it);
        }

        public long GetObstacleRef(DtTileCacheObstacle ob)
        {
            if (ob == null)
            {
                return 0;
            }

            int idx = ob.index;
            return EncodeObstacleId(ob.salt, idx);
        }

        public DtTileCacheObstacle GetObstacleByRef(long refs)
        {
            if (refs == 0)
            {
                return null;
            }

            int idx = DecodeObstacleIdObstacle(refs);
            if (idx >= m_obstacles.Count)
            {
                return null;
            }

            DtTileCacheObstacle ob = m_obstacles[idx];
            int salt = DecodeObstacleIdSalt(refs);
            if (ob.salt != salt)
            {
                return null;
            }

            return ob;
        }

        public long AddTile(byte[] data, int flags)
        {
            // Make sure the data is in right format.
            RcByteBuffer buf = new RcByteBuffer(data);
            buf.Order(m_storageParams.Order);
            DtTileCacheLayerHeader header = tileReader.Read(buf, m_storageParams.Compatibility);
            // Make sure the location is free.
            if (GetTileAt(header.tx, header.ty, header.tlayer) != null)
            {
                return 0;
            }

            // Allocate a tile.
            DtCompressedTile tile = null;
            if (m_nextFreeTile != null)
            {
                tile = m_nextFreeTile;
                m_nextFreeTile = tile.next;
                tile.next = null;
            }

            // Make sure we could allocate a tile.
            if (tile == null)
            {
                throw new Exception("Out of storage");
            }

            // Insert tile into the position lut.
            int h = DtNavMesh.ComputeTileHash(header.tx, header.ty, m_tileLutMask);
            tile.next = m_posLookup[h];
            m_posLookup[h] = tile;

            // Init tile.
            tile.header = header;
            tile.data = data;
            tile.compressed = Align4(buf.Position());
            tile.flags = flags;

            return GetTileRef(tile);
        }

        private int Align4(int i)
        {
            return (i + 3) & (~3);
        }

        public void RemoveTile(long refs)
        {
            if (refs == 0)
            {
                throw new Exception("Invalid tile ref");
            }

            int tileIndex = DecodeTileIdTile(refs);
            int tileSalt = DecodeTileIdSalt(refs);
            if (tileIndex >= m_params.maxTiles)
            {
                throw new Exception("Invalid tile index");
            }

            DtCompressedTile tile = m_tiles[tileIndex];
            if (tile.salt != tileSalt)
            {
                throw new Exception("Invalid tile salt");
            }

            // Remove tile from hash lookup.
            int h = DtNavMesh.ComputeTileHash(tile.header.tx, tile.header.ty, m_tileLutMask);
            DtCompressedTile prev = null;
            DtCompressedTile cur = m_posLookup[h];
            while (cur != null)
            {
                if (cur == tile)
                {
                    if (prev != null)
                    {
                        prev.next = cur.next;
                    }
                    else
                    {
                        m_posLookup[h] = cur.next;
                    }

                    break;
                }

                prev = cur;
                cur = cur.next;
            }

            tile.header = null;
            tile.data = null;
            tile.compressed = 0;
            tile.flags = 0;

            // Update salt, salt should never be zero.
            tile.salt = (tile.salt + 1) & ((1 << m_saltBits) - 1);
            if (tile.salt == 0)
            {
                tile.salt++;
            }

            // Add to free list.
            tile.next = m_nextFreeTile;
            m_nextFreeTile = tile;
        }

        // Cylinder obstacle
        public long AddObstacle(RcVec3f pos, float radius, float height)
        {
            DtTileCacheObstacle ob = AllocObstacle();
            ob.type = TileCacheObstacleType.CYLINDER;

            ob.pos = pos;
            ob.radius = radius;
            ob.height = height;

            return AddObstacleRequest(ob).refs;
        }

        // Aabb obstacle
        public long AddBoxObstacle(RcVec3f bmin, RcVec3f bmax)
        {
            DtTileCacheObstacle ob = AllocObstacle();
            ob.type = TileCacheObstacleType.BOX;

            ob.bmin = bmin;
            ob.bmax = bmax;

            return AddObstacleRequest(ob).refs;
        }

        // Box obstacle: can be rotated in Y
        public long AddBoxObstacle(RcVec3f center, RcVec3f extents, float yRadians)
        {
            DtTileCacheObstacle ob = AllocObstacle();
            ob.type = TileCacheObstacleType.ORIENTED_BOX;
            ob.center = center;
            ob.extents = extents;
            float coshalf = (float)Math.Cos(0.5f * yRadians);
            float sinhalf = (float)Math.Sin(-0.5f * yRadians);
            ob.rotAux[0] = coshalf * sinhalf;
            ob.rotAux[1] = coshalf * coshalf - 0.5f;
            return AddObstacleRequest(ob).refs;
        }

        private ObstacleRequest AddObstacleRequest(DtTileCacheObstacle ob)
        {
            ObstacleRequest req = new ObstacleRequest(ObstacleRequestAction.REQUEST_ADD, GetObstacleRef(ob));
            m_reqs.Add(req);
            return req;
        }

        public void RemoveObstacle(long refs)
        {
            if (refs == 0)
            {
                return;
            }

            ObstacleRequest req = new ObstacleRequest(ObstacleRequestAction.REQUEST_REMOVE, refs);
            m_reqs.Add(req);
        }

        private DtTileCacheObstacle AllocObstacle()
        {
            DtTileCacheObstacle o = m_nextFreeObstacle;
            if (o == null)
            {
                o = new DtTileCacheObstacle(m_obstacles.Count);
                m_obstacles.Add(o);
            }
            else
            {
                m_nextFreeObstacle = o.next;
            }

            o.state = DtObstacleState.DT_OBSTACLE_PROCESSING;
            o.touched.Clear();
            o.pending.Clear();
            o.next = null;
            return o;
        }

        public int GetObstacleCount()
        {
            return m_obstacles.Count;
        }

        public DtTileCacheObstacle GetObstacle(int i)
        {
            if (0 > i || i >= m_obstacles.Count)
            {
                return null;
            }

            return m_obstacles[i];
        }

        private List<long> QueryTiles(RcVec3f bmin, RcVec3f bmax)
        {
            List<long> results = new List<long>();
            float tw = m_params.width * m_params.cs;
            float th = m_params.height * m_params.cs;
            int tx0 = (int)Math.Floor((bmin.x - m_params.orig.x) / tw);
            int tx1 = (int)Math.Floor((bmax.x - m_params.orig.x) / tw);
            int ty0 = (int)Math.Floor((bmin.z - m_params.orig.z) / th);
            int ty1 = (int)Math.Floor((bmax.z - m_params.orig.z) / th);
            for (int ty = ty0; ty <= ty1; ++ty)
            {
                for (int tx = tx0; tx <= tx1; ++tx)
                {
                    List<long> tiles = GetTilesAt(tx, ty);
                    foreach (long i in tiles)
                    {
                        DtCompressedTile tile = m_tiles[DecodeTileIdTile(i)];
                        RcVec3f tbmin = new RcVec3f();
                        RcVec3f tbmax = new RcVec3f();
                        CalcTightTileBounds(tile.header, ref tbmin, ref tbmax);
                        if (DtUtils.OverlapBounds(bmin, bmax, tbmin, tbmax))
                        {
                            results.Add(i);
                        }
                    }
                }
            }

            return results;
        }

        /**
         * Updates the tile cache by rebuilding tiles touched by unfinished obstacle requests.
         *
         * @return Returns true if the tile cache is fully up to date with obstacle requests and tile rebuilds. If the tile
         *         cache is up to date another (immediate) call to update will have no effect; otherwise another call will
         *         continue processing obstacle requests and tile rebuilds.
         */
        public bool Update()
        {
            if (0 == m_update.Count)
            {
                // Process requests.
                foreach (ObstacleRequest req in m_reqs)
                {
                    int idx = DecodeObstacleIdObstacle(req.refs);
                    if (idx >= m_obstacles.Count)
                    {
                        continue;
                    }

                    DtTileCacheObstacle ob = m_obstacles[idx];
                    int salt = DecodeObstacleIdSalt(req.refs);
                    if (ob.salt != salt)
                    {
                        continue;
                    }

                    if (req.action == ObstacleRequestAction.REQUEST_ADD)
                    {
                        // Find touched tiles.
                        RcVec3f bmin = new RcVec3f();
                        RcVec3f bmax = new RcVec3f();
                        GetObstacleBounds(ob, ref bmin, ref bmax);
                        ob.touched = QueryTiles(bmin, bmax);
                        // Add tiles to update list.
                        ob.pending.Clear();
                        foreach (long j in ob.touched)
                        {
                            if (!Contains(m_update, j))
                            {
                                m_update.Add(j);
                            }

                            ob.pending.Add(j);
                        }
                    }
                    else if (req.action == ObstacleRequestAction.REQUEST_REMOVE)
                    {
                        // Prepare to remove obstacle.
                        ob.state = DtObstacleState.DT_OBSTACLE_REMOVING;
                        // Add tiles to update list.
                        ob.pending.Clear();
                        foreach (long j in ob.touched)
                        {
                            if (!Contains(m_update, j))
                            {
                                m_update.Add(j);
                            }

                            ob.pending.Add(j);
                        }
                    }
                }

                m_reqs.Clear();
            }

            // Process updates
            if (0 < m_update.Count)
            {
                long refs = m_update[0];
                m_update.RemoveAt(0);
                // Build mesh
                BuildNavMeshTile(refs);

                // Update obstacle states.
                for (int i = 0; i < m_obstacles.Count; ++i)
                {
                    DtTileCacheObstacle ob = m_obstacles[i];
                    if (ob.state == DtObstacleState.DT_OBSTACLE_PROCESSING
                        || ob.state == DtObstacleState.DT_OBSTACLE_REMOVING)
                    {
                        // Remove handled tile from pending list.
                        ob.pending.Remove(refs);

                        // If all pending tiles processed, change state.
                        if (0 == ob.pending.Count)
                        {
                            if (ob.state == DtObstacleState.DT_OBSTACLE_PROCESSING)
                            {
                                ob.state = DtObstacleState.DT_OBSTACLE_PROCESSED;
                            }
                            else if (ob.state == DtObstacleState.DT_OBSTACLE_REMOVING)
                            {
                                ob.state = DtObstacleState.DT_OBSTACLE_EMPTY;
                                // Update salt, salt should never be zero.
                                ob.salt = (ob.salt + 1) & ((1 << 16) - 1);
                                if (ob.salt == 0)
                                {
                                    ob.salt++;
                                }

                                // Return obstacle to free list.
                                ob.next = m_nextFreeObstacle;
                                m_nextFreeObstacle = ob;
                            }
                        }
                    }
                }
            }

            return 0 == m_update.Count && 0 == m_reqs.Count;
        }

        public void BuildNavMeshTile(long refs)
        {
            int idx = DecodeTileIdTile(refs);
            if (idx > m_params.maxTiles)
            {
                throw new Exception("Invalid tile index");
            }

            DtCompressedTile tile = m_tiles[idx];
            int salt = DecodeTileIdSalt(refs);
            if (tile.salt != salt)
            {
                throw new Exception("Invalid tile salt");
            }

            int walkableClimbVx = (int)(m_params.walkableClimb / m_params.ch);

            // Decompress tile layer data.
            DtTileCacheLayer layer = DecompressTile(tile);

            // Rasterize obstacles.
            for (int i = 0; i < m_obstacles.Count; ++i)
            {
                DtTileCacheObstacle ob = m_obstacles[i];
                if (ob.state == DtObstacleState.DT_OBSTACLE_EMPTY || ob.state == DtObstacleState.DT_OBSTACLE_REMOVING)
                {
                    continue;
                }

                if (Contains(ob.touched, refs))
                {
                    if (ob.type == TileCacheObstacleType.CYLINDER)
                    {
                        builder.MarkCylinderArea(layer, tile.header.bmin, m_params.cs, m_params.ch, ob.pos, ob.radius, ob.height, 0);
                    }
                    else if (ob.type == TileCacheObstacleType.BOX)
                    {
                        builder.MarkBoxArea(layer, tile.header.bmin, m_params.cs, m_params.ch, ob.bmin, ob.bmax, 0);
                    }
                    else if (ob.type == TileCacheObstacleType.ORIENTED_BOX)
                    {
                        builder.MarkBoxArea(layer, tile.header.bmin, m_params.cs, m_params.ch, ob.center, ob.extents, ob.rotAux, 0);
                    }
                }
            }

            // Build navmesh
            builder.BuildTileCacheRegions(layer, walkableClimbVx);
            DtTileCacheContourSet lcset = builder.BuildTileCacheContours(layer, walkableClimbVx,
                m_params.maxSimplificationError);
            DtTileCachePolyMesh polyMesh = builder.BuildTileCachePolyMesh(lcset, m_navmesh.GetMaxVertsPerPoly());
            // Early out if the mesh tile is empty.
            if (polyMesh.npolys == 0)
            {
                m_navmesh.RemoveTile(m_navmesh.GetTileRefAt(tile.header.tx, tile.header.ty, tile.header.tlayer));
                return;
            }

            DtNavMeshCreateParams option = new DtNavMeshCreateParams();
            option.verts = polyMesh.verts;
            option.vertCount = polyMesh.nverts;
            option.polys = polyMesh.polys;
            option.polyAreas = polyMesh.areas;
            option.polyFlags = polyMesh.flags;
            option.polyCount = polyMesh.npolys;
            option.nvp = m_navmesh.GetMaxVertsPerPoly();
            option.walkableHeight = m_params.walkableHeight;
            option.walkableRadius = m_params.walkableRadius;
            option.walkableClimb = m_params.walkableClimb;
            option.tileX = tile.header.tx;
            option.tileZ = tile.header.ty;
            option.tileLayer = tile.header.tlayer;
            option.cs = m_params.cs;
            option.ch = m_params.ch;
            option.buildBvTree = false;
            option.bmin = tile.header.bmin;
            option.bmax = tile.header.bmax;
            if (m_tmproc != null)
            {
                m_tmproc.Process(option);
            }

            DtMeshData meshData = DtNavMeshBuilder.CreateNavMeshData(option);
            // Remove existing tile.
            m_navmesh.RemoveTile(m_navmesh.GetTileRefAt(tile.header.tx, tile.header.ty, tile.header.tlayer));
            // Add new tile, or leave the location empty. if (navData) { // Let the
            if (meshData != null)
            {
                m_navmesh.AddTile(meshData, 0, 0);
            }
        }

        public DtTileCacheLayer DecompressTile(DtCompressedTile tile)
        {
            DtTileCacheLayer layer = builder.DecompressTileCacheLayer(m_tcomp, tile.data, m_storageParams.Order, m_storageParams.Compatibility);
            return layer;
        }

        void CalcTightTileBounds(DtTileCacheLayerHeader header, ref RcVec3f bmin, ref RcVec3f bmax)
        {
            float cs = m_params.cs;
            bmin.x = header.bmin.x + header.minx * cs;
            bmin.y = header.bmin.y;
            bmin.z = header.bmin.z + header.miny * cs;
            bmax.x = header.bmin.x + (header.maxx + 1) * cs;
            bmax.y = header.bmax.y;
            bmax.z = header.bmin.z + (header.maxy + 1) * cs;
        }

        public void GetObstacleBounds(DtTileCacheObstacle ob, ref RcVec3f bmin, ref RcVec3f bmax)
        {
            if (ob.type == TileCacheObstacleType.CYLINDER)
            {
                bmin.x = ob.pos.x - ob.radius;
                bmin.y = ob.pos.y;
                bmin.z = ob.pos.z - ob.radius;
                bmax.x = ob.pos.x + ob.radius;
                bmax.y = ob.pos.y + ob.height;
                bmax.z = ob.pos.z + ob.radius;
            }
            else if (ob.type == TileCacheObstacleType.BOX)
            {
                bmin = ob.bmin;
                bmax = ob.bmax;
            }
            else if (ob.type == TileCacheObstacleType.ORIENTED_BOX)
            {
                float maxr = 1.41f * Math.Max(ob.extents.x, ob.extents.z);
                bmin.x = ob.center.x - maxr;
                bmax.x = ob.center.x + maxr;
                bmin.y = ob.center.y - ob.extents.y;
                bmax.y = ob.center.y + ob.extents.y;
                bmin.z = ob.center.z - maxr;
                bmax.z = ob.center.z + maxr;
            }
        }

        public ref readonly DtTileCacheParams GetParams()
        {
            return ref m_params;
        }

        public IRcCompressor GetCompressor()
        {
            return m_tcomp;
        }

        public int GetTileCount()
        {
            return m_params.maxTiles;
        }

        public DtCompressedTile GetTile(int i)
        {
            return m_tiles[i];
        }

        public DtNavMesh GetNavMesh()
        {
            return m_navmesh;
        }
    }
}