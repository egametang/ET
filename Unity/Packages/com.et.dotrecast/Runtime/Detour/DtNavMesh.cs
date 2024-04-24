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

namespace DotRecast.Detour
{
    using static RcMath;

    public class DtNavMesh
    {
        public const int DT_SALT_BITS = 16;
        public const int DT_TILE_BITS = 28;
        public const int DT_POLY_BITS = 20;
        public const int DT_DETAIL_EDGE_BOUNDARY = 0x01;

        /// A flag that indicates that an entity links to an external entity.
        /// (E.g. A polygon edge is a portal that links to another polygon.)
        public const int DT_EXT_LINK = 0x8000;

        /// A value that indicates the entity does not link to anything.
        public const int DT_NULL_LINK = unchecked((int)0xffffffff);

        /// A flag that indicates that an off-mesh connection can be traversed in
        /// both directions. (Is bidirectional.)
        public const int DT_OFFMESH_CON_BIDIR = 1;

        /// The maximum number of user defined area ids.
        public const int DT_MAX_AREAS = 64;

        /// Limit raycasting during any angle pahfinding
        /// The limit is given as a multiple of the character radius
        public const float DT_RAY_CAST_LIMIT_PROPORTIONS = 50.0f;

        private readonly DtNavMeshParams m_params;

        /// < Current initialization params. TODO: do not store this info twice.
        private readonly RcVec3f m_orig;

        /// < Origin of the tile (0,0)
        // float m_orig[3]; ///< Origin of the tile (0,0)
        private float m_tileWidth;

        private float m_tileHeight;

        /// < Dimensions of each tile.
        int m_maxTiles;

        /// < Max number of tiles.
        private readonly int m_tileLutMask;

        /// < Tile hash lookup mask.
        private readonly Dictionary<int, List<DtMeshTile>> posLookup = new Dictionary<int, List<DtMeshTile>>();

        private readonly LinkedList<DtMeshTile> availableTiles = new LinkedList<DtMeshTile>();
        private readonly DtMeshTile[] m_tiles;

        /// < List of tiles.
        /** The maximum number of vertices per navigation polygon. */
        private readonly int m_maxVertPerPoly;

        private int m_tileCount;

        public DtNavMesh(DtMeshData data, int maxVertsPerPoly, int flags)
            : this(GetNavMeshParams(data), maxVertsPerPoly)
        {
            AddTile(data, flags, 0);
        }

        public DtNavMesh(DtNavMeshParams option, int maxVertsPerPoly)
        {
            m_params = option;
            m_orig = option.orig;
            m_tileWidth = option.tileWidth;
            m_tileHeight = option.tileHeight;
            // Init tiles
            m_maxTiles = option.maxTiles;
            m_maxVertPerPoly = maxVertsPerPoly;
            m_tileLutMask = Math.Max(1, DtUtils.NextPow2(option.maxTiles)) - 1;
            m_tiles = new DtMeshTile[m_maxTiles];
            for (int i = 0; i < m_maxTiles; i++)
            {
                m_tiles[i] = new DtMeshTile(i);
                m_tiles[i].salt = 1;
                availableTiles.AddLast(m_tiles[i]);
            }
        }

        private static DtNavMeshParams GetNavMeshParams(DtMeshData data)
        {
            DtNavMeshParams option = new DtNavMeshParams();
            option.orig = data.header.bmin;
            option.tileWidth = data.header.bmax.x - data.header.bmin.x;
            option.tileHeight = data.header.bmax.z - data.header.bmin.z;
            option.maxTiles = 1;
            option.maxPolys = data.header.polyCount;
            return option;
        }


        /**
     * The maximum number of tiles supported by the navigation mesh.
     *
     * @return The maximum number of tiles supported by the navigation mesh.
     */
        public int GetMaxTiles()
        {
            return m_maxTiles;
        }

        /**
     * Returns tile in the tile array.
     */
        public DtMeshTile GetTile(int i)
        {
            return m_tiles[i];
        }

        /**
     * Gets the polygon reference for the tile's base polygon.
     *
     * @param tile
     *            The tile.
     * @return The polygon reference for the base polygon in the specified tile.
     */
        public long GetPolyRefBase(DtMeshTile tile)
        {
            if (tile == null)
            {
                return 0;
            }

            int it = tile.index;
            return EncodePolyId(tile.salt, it, 0);
        }

        /// @{
        /// @name Encoding and Decoding
        /// These functions are generally meant for internal use only.
        /// Derives a standard polygon reference.
        ///  @note This function is generally meant for internal use only.
        ///  @param[in]	salt	The tile's salt value.
        ///  @param[in]	it		The index of the tile.
        ///  @param[in]	ip		The index of the polygon within the tile.
        public static long EncodePolyId(int salt, int it, int ip)
        {
            return (((long)salt) << (DT_POLY_BITS + DT_TILE_BITS)) | ((long)it << DT_POLY_BITS) | (long)ip;
        }

        /// Decodes a standard polygon reference.
        /// @note This function is generally meant for internal use only.
        /// @param[in] ref The polygon reference to decode.
        /// @param[out] salt The tile's salt value.
        /// @param[out] it The index of the tile.
        /// @param[out] ip The index of the polygon within the tile.
        /// @see #encodePolyId
        static void DecodePolyId(long refs, out int salt, out int it, out int ip)
        {
            long saltMask = (1L << DT_SALT_BITS) - 1;
            long tileMask = (1L << DT_TILE_BITS) - 1;
            long polyMask = (1L << DT_POLY_BITS) - 1;
            salt = (int)((refs >> (DT_POLY_BITS + DT_TILE_BITS)) & saltMask);
            it = (int)((refs >> DT_POLY_BITS) & tileMask);
            ip = (int)(refs & polyMask);
        }

        /// Extracts a tile's salt value from the specified polygon reference.
        /// @note This function is generally meant for internal use only.
        /// @param[in] ref The polygon reference.
        /// @see #encodePolyId
        static int DecodePolyIdSalt(long refs)
        {
            long saltMask = (1L << DT_SALT_BITS) - 1;
            return (int)((refs >> (DT_POLY_BITS + DT_TILE_BITS)) & saltMask);
        }

        /// Extracts the tile's index from the specified polygon reference.
        /// @note This function is generally meant for internal use only.
        /// @param[in] ref The polygon reference.
        /// @see #encodePolyId
        public static int DecodePolyIdTile(long refs)
        {
            long tileMask = (1L << DT_TILE_BITS) - 1;
            return (int)((refs >> DT_POLY_BITS) & tileMask);
        }

        /// Extracts the polygon's index (within its tile) from the specified
        /// polygon reference.
        /// @note This function is generally meant for internal use only.
        /// @param[in] ref The polygon reference.
        /// @see #encodePolyId
        static int DecodePolyIdPoly(long refs)
        {
            long polyMask = (1L << DT_POLY_BITS) - 1;
            return (int)(refs & polyMask);
        }

        private int AllocLink(DtMeshTile tile)
        {
            if (tile.linksFreeList == DT_NULL_LINK)
            {
                DtLink link = new DtLink();
                link.next = DT_NULL_LINK;
                tile.links.Add(link);
                return tile.links.Count - 1;
            }

            int linkIdx = tile.linksFreeList;
            tile.linksFreeList = tile.links[linkIdx].next;
            return linkIdx;
        }

        private void FreeLink(DtMeshTile tile, int link)
        {
            tile.links[link].next = tile.linksFreeList;
            tile.linksFreeList = link;
        }

        /**
     * Calculates the tile grid location for the specified world position.
     *
     * @param pos
     *            The world position for the query. [(x, y, z)]
     * @return 2-element int array with (tx,ty) tile location
     */
        public void CalcTileLoc(RcVec3f pos, out int tx, out int ty)
        {
            tx = (int)Math.Floor((pos.x - m_orig.x) / m_tileWidth);
            ty = (int)Math.Floor((pos.z - m_orig.z) / m_tileHeight);
        }

        /// Gets the tile and polygon for the specified polygon reference.
        ///  @param[in]		ref		The reference for the a polygon.
        ///  @param[out]	tile	The tile containing the polygon.
        ///  @param[out]	poly	The polygon.
        /// @return The status flags for the operation.
        public DtStatus GetTileAndPolyByRef(long refs, out DtMeshTile tile, out DtPoly poly)
        {
            tile = null;
            poly = null;

            if (refs == 0)
            {
                return DtStatus.DT_FAILURE;
            }

            DecodePolyId(refs, out var salt, out var it, out var ip);
            if (it >= m_maxTiles)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            if (m_tiles[it].salt != salt || m_tiles[it].data.header == null)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            if (ip >= m_tiles[it].data.header.polyCount)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            tile = m_tiles[it];
            poly = m_tiles[it].data.polys[ip];

            return DtStatus.DT_SUCCSESS;
        }

        /// @par
        ///
        /// @warning Only use this function if it is known that the provided polygon
        /// reference is valid. This function is faster than #getTileAndPolyByRef,
        /// but
        /// it does not validate the reference.
        public void GetTileAndPolyByRefUnsafe(long refs, out DtMeshTile tile, out DtPoly poly)
        {
            DecodePolyId(refs, out var salt, out var it, out var ip);
            tile = m_tiles[it];
            poly = m_tiles[it].data.polys[ip];
        }

        public bool IsValidPolyRef(long refs)
        {
            if (refs == 0)
            {
                return false;
            }

            DecodePolyId(refs, out var salt, out var it, out var ip);
            if (it >= m_maxTiles)
            {
                return false;
            }

            if (m_tiles[it].salt != salt || m_tiles[it].data == null)
            {
                return false;
            }

            if (ip >= m_tiles[it].data.header.polyCount)
            {
                return false;
            }

            return true;
        }

        public ref readonly DtNavMeshParams GetParams()
        {
            return ref m_params;
        }


        // TODO: These methods are duplicates from dtNavMeshQuery, but are needed
        // for off-mesh connection finding.

        List<long> QueryPolygonsInTile(DtMeshTile tile, RcVec3f qmin, RcVec3f qmax)
        {
            List<long> polys = new List<long>();
            if (tile.data.bvTree != null)
            {
                int nodeIndex = 0;
                var tbmin = tile.data.header.bmin;
                var tbmax = tile.data.header.bmax;
                float qfac = tile.data.header.bvQuantFactor;
                // Calculate quantized box
                int[] bmin = new int[3];
                int[] bmax = new int[3];
                // dtClamp query box to world box.
                float minx = Clamp(qmin.x, tbmin.x, tbmax.x) - tbmin.x;
                float miny = Clamp(qmin.y, tbmin.y, tbmax.y) - tbmin.y;
                float minz = Clamp(qmin.z, tbmin.z, tbmax.z) - tbmin.z;
                float maxx = Clamp(qmax.x, tbmin.x, tbmax.x) - tbmin.x;
                float maxy = Clamp(qmax.y, tbmin.y, tbmax.y) - tbmin.y;
                float maxz = Clamp(qmax.z, tbmin.z, tbmax.z) - tbmin.z;
                // Quantize
                bmin[0] = (int)(qfac * minx) & 0x7ffffffe;
                bmin[1] = (int)(qfac * miny) & 0x7ffffffe;
                bmin[2] = (int)(qfac * minz) & 0x7ffffffe;
                bmax[0] = (int)(qfac * maxx + 1) | 1;
                bmax[1] = (int)(qfac * maxy + 1) | 1;
                bmax[2] = (int)(qfac * maxz + 1) | 1;

                // Traverse tree
                long @base = GetPolyRefBase(tile);
                int end = tile.data.header.bvNodeCount;
                while (nodeIndex < end)
                {
                    DtBVNode node = tile.data.bvTree[nodeIndex];
                    bool overlap = DtUtils.OverlapQuantBounds(bmin, bmax, node.bmin, node.bmax);
                    bool isLeafNode = node.i >= 0;

                    if (isLeafNode && overlap)
                    {
                        polys.Add(@base | (long)node.i);
                    }

                    if (overlap || isLeafNode)
                    {
                        nodeIndex++;
                    }
                    else
                    {
                        int escapeIndex = -node.i;
                        nodeIndex += escapeIndex;
                    }
                }

                return polys;
            }
            else
            {
                RcVec3f bmin = new RcVec3f();
                RcVec3f bmax = new RcVec3f();
                long @base = GetPolyRefBase(tile);
                for (int i = 0; i < tile.data.header.polyCount; ++i)
                {
                    DtPoly p = tile.data.polys[i];
                    // Do not return off-mesh connection polygons.
                    if (p.GetPolyType() == DtPoly.DT_POLYTYPE_OFFMESH_CONNECTION)
                    {
                        continue;
                    }

                    // Calc polygon bounds.
                    int v = p.verts[0] * 3;
                    bmin.Set(tile.data.verts, v);
                    bmax.Set(tile.data.verts, v);
                    for (int j = 1; j < p.vertCount; ++j)
                    {
                        v = p.verts[j] * 3;
                        bmin.Min(tile.data.verts, v);
                        bmax.Max(tile.data.verts, v);
                    }

                    if (DtUtils.OverlapBounds(qmin, qmax, bmin, bmax))
                    {
                        polys.Add(@base | (long)i);
                    }
                }

                return polys;
            }
        }

        public long UpdateTile(DtMeshData data, int flags)
        {
            long refs = GetTileRefAt(data.header.x, data.header.y, data.header.layer);
            refs = RemoveTile(refs);
            return AddTile(data, flags, refs);
        }

        /// Adds a tile to the navigation mesh.
        /// @param[in] data Data for the new tile mesh. (See: #dtCreateNavMeshData)
        /// @param[in] dataSize Data size of the new tile mesh.
        /// @param[in] flags Tile flags. (See: #dtTileFlags)
        /// @param[in] lastRef The desired reference for the tile. (When reloading a
        /// tile.) [opt] [Default: 0]
        /// @param[out] result The tile reference. (If the tile was succesfully
        /// added.) [opt]
        /// @return The status flags for the operation.
        /// @par
        ///
        /// The add operation will fail if the data is in the wrong format, the
        /// allocated tile
        /// space is full, or there is a tile already at the specified reference.
        ///
        /// The lastRef parameter is used to restore a tile with the same tile
        /// reference it had previously used. In this case the #long's for the
        /// tile will be restored to the same values they were before the tile was
        /// removed.
        ///
        /// The nav mesh assumes exclusive access to the data passed and will make
        /// changes to the dynamic portion of the data. For that reason the data
        /// should not be reused in other nav meshes until the tile has been successfully
        /// removed from this nav mesh.
        ///
        /// @see dtCreateNavMeshData, #removeTile
        public long AddTile(DtMeshData data, int flags, long lastRef)
        {
            // Make sure the data is in right format.
            DtMeshHeader header = data.header;

            // Make sure the location is free.
            if (GetTileAt(header.x, header.y, header.layer) != null)
            {
                throw new Exception("Tile already exists");
            }

            // Allocate a tile.
            DtMeshTile tile = null;
            if (lastRef == 0)
            {
                // Make sure we could allocate a tile.
                if (0 == availableTiles.Count)
                {
                    throw new Exception("Could not allocate a tile");
                }

                tile = availableTiles.First?.Value;
                availableTiles.RemoveFirst();
                m_tileCount++;
            }
            else
            {
                // Try to relocate the tile to specific index with same salt.
                int tileIndex = DecodePolyIdTile(lastRef);
                if (tileIndex >= m_maxTiles)
                {
                    throw new Exception("Tile index too high");
                }

                // Try to find the specific tile id from the free list.
                DtMeshTile target = m_tiles[tileIndex];
                // Remove from freelist
                if (!availableTiles.Remove(target))
                {
                    // Could not find the correct location.
                    throw new Exception("Could not find tile");
                }

                tile = target;
                // Restore salt.
                tile.salt = DecodePolyIdSalt(lastRef);
            }

            tile.data = data;
            tile.flags = flags;
            tile.links.Clear();
            tile.polyLinks = new int[data.polys.Length];
            Array.Fill(tile.polyLinks, DtNavMesh.DT_NULL_LINK);

            // Insert tile into the position lut.
            GetTileListByPos(header.x, header.y).Add(tile);

            // Patch header pointers.

            // If there are no items in the bvtree, reset the tree pointer.
            if (tile.data.bvTree != null && tile.data.bvTree.Length == 0)
            {
                tile.data.bvTree = null;
            }

            // Init tile.

            ConnectIntLinks(tile);
            // Base off-mesh connections to their starting polygons and connect connections inside the tile.
            BaseOffMeshLinks(tile);
            ConnectExtOffMeshLinks(tile, tile, -1);

            // Connect with layers in current tile.
            List<DtMeshTile> neis = GetTilesAt(header.x, header.y);
            for (int j = 0; j < neis.Count; ++j)
            {
                if (neis[j] == tile)
                {
                    continue;
                }

                ConnectExtLinks(tile, neis[j], -1);
                ConnectExtLinks(neis[j], tile, -1);
                ConnectExtOffMeshLinks(tile, neis[j], -1);
                ConnectExtOffMeshLinks(neis[j], tile, -1);
            }

            // Connect with neighbour tiles.
            for (int i = 0; i < 8; ++i)
            {
                neis = GetNeighbourTilesAt(header.x, header.y, i);
                for (int j = 0; j < neis.Count; ++j)
                {
                    ConnectExtLinks(tile, neis[j], i);
                    ConnectExtLinks(neis[j], tile, DtUtils.OppositeTile(i));
                    ConnectExtOffMeshLinks(tile, neis[j], i);
                    ConnectExtOffMeshLinks(neis[j], tile, DtUtils.OppositeTile(i));
                }
            }

            return GetTileRef(tile);
        }

        /// Removes the specified tile from the navigation mesh.
        /// @param[in] ref The reference of the tile to remove.
        /// @param[out] data Data associated with deleted tile.
        /// @param[out] dataSize Size of the data associated with deleted tile.
        ///
        /// This function returns the data for the tile so that, if desired,
        /// it can be added back to the navigation mesh at a later point.
        ///
        /// @see #addTile
        public long RemoveTile(long refs)
        {
            if (refs == 0)
            {
                return 0;
            }

            int tileIndex = DecodePolyIdTile(refs);
            int tileSalt = DecodePolyIdSalt(refs);
            if (tileIndex >= m_maxTiles)
            {
                throw new Exception("Invalid tile index");
            }

            DtMeshTile tile = m_tiles[tileIndex];
            if (tile.salt != tileSalt)
            {
                throw new Exception("Invalid tile salt");
            }

            // Remove tile from hash lookup.
            GetTileListByPos(tile.data.header.x, tile.data.header.y).Remove(tile);

            // Remove connections to neighbour tiles.
            // Create connections with neighbour tiles.

            // Disconnect from other layers in current tile.
            List<DtMeshTile> nneis = GetTilesAt(tile.data.header.x, tile.data.header.y);
            foreach (DtMeshTile j in nneis)
            {
                if (j == tile)
                {
                    continue;
                }

                UnconnectLinks(j, tile);
            }

            // Disconnect from neighbour tiles.
            for (int i = 0; i < 8; ++i)
            {
                nneis = GetNeighbourTilesAt(tile.data.header.x, tile.data.header.y, i);
                foreach (DtMeshTile j in nneis)
                {
                    UnconnectLinks(j, tile);
                }
            }

            // Reset tile.
            tile.data = null;

            tile.flags = 0;
            tile.links.Clear();
            tile.linksFreeList = DtNavMesh.DT_NULL_LINK;

            // Update salt, salt should never be zero.
            tile.salt = (tile.salt + 1) & ((1 << DT_SALT_BITS) - 1);
            if (tile.salt == 0)
            {
                tile.salt++;
            }

            // Add to free list.
            availableTiles.AddFirst(tile);
            m_tileCount--;
            return GetTileRef(tile);
        }

        /// Builds internal polygons links for a tile.
        void ConnectIntLinks(DtMeshTile tile)
        {
            if (tile == null)
            {
                return;
            }

            long @base = GetPolyRefBase(tile);

            for (int i = 0; i < tile.data.header.polyCount; ++i)
            {
                DtPoly poly = tile.data.polys[i];
                tile.polyLinks[poly.index] = DT_NULL_LINK;

                if (poly.GetPolyType() == DtPoly.DT_POLYTYPE_OFFMESH_CONNECTION)
                {
                    continue;
                }

                // Build edge links backwards so that the links will be
                // in the linked list from lowest index to highest.
                for (int j = poly.vertCount - 1; j >= 0; --j)
                {
                    // Skip hard and non-internal edges.
                    if (poly.neis[j] == 0 || (poly.neis[j] & DT_EXT_LINK) != 0)
                    {
                        continue;
                    }

                    int idx = AllocLink(tile);
                    DtLink link = tile.links[idx];
                    link.refs = @base | (long)(poly.neis[j] - 1);
                    link.edge = j;
                    link.side = 0xff;
                    link.bmin = link.bmax = 0;
                    // Add to linked list.
                    link.next = tile.polyLinks[poly.index];
                    tile.polyLinks[poly.index] = idx;
                }
            }
        }

        void UnconnectLinks(DtMeshTile tile, DtMeshTile target)
        {
            if (tile == null || target == null)
            {
                return;
            }

            int targetNum = DecodePolyIdTile(GetTileRef(target));

            for (int i = 0; i < tile.data.header.polyCount; ++i)
            {
                DtPoly poly = tile.data.polys[i];
                int j = tile.polyLinks[poly.index];
                int pj = DT_NULL_LINK;
                while (j != DT_NULL_LINK)
                {
                    if (DecodePolyIdTile(tile.links[j].refs) == targetNum)
                    {
                        // Remove link.
                        int nj = tile.links[j].next;
                        if (pj == DT_NULL_LINK)
                        {
                            tile.polyLinks[poly.index] = nj;
                        }
                        else
                        {
                            tile.links[pj].next = nj;
                        }

                        FreeLink(tile, j);
                        j = nj;
                    }
                    else
                    {
                        // Advance
                        pj = j;
                        j = tile.links[j].next;
                    }
                }
            }
        }

        void ConnectExtLinks(DtMeshTile tile, DtMeshTile target, int side)
        {
            if (tile == null)
            {
                return;
            }

            var connectPolys = new List<DtConnectPoly>();

            // Connect border links.
            for (int i = 0; i < tile.data.header.polyCount; ++i)
            {
                DtPoly poly = tile.data.polys[i];

                // Create new links.
                // short m = DT_EXT_LINK | (short)side;

                int nv = poly.vertCount;
                for (int j = 0; j < nv; ++j)
                {
                    // Skip non-portal edges.
                    if ((poly.neis[j] & DT_EXT_LINK) == 0)
                    {
                        continue;
                    }

                    int dir = poly.neis[j] & 0xff;
                    if (side != -1 && dir != side)
                    {
                        continue;
                    }

                    // Create new links
                    int va = poly.verts[j] * 3;
                    int vb = poly.verts[(j + 1) % nv] * 3;
                    int nnei = FindConnectingPolys(tile.data.verts, va, vb, target, DtUtils.OppositeTile(dir), ref connectPolys);
                    foreach (var connectPoly in connectPolys)
                    {
                        int idx = AllocLink(tile);
                        DtLink link = tile.links[idx];
                        link.refs = connectPoly.refs;
                        link.edge = j;
                        link.side = dir;

                        link.next = tile.polyLinks[poly.index];
                        tile.polyLinks[poly.index] = idx;

                        // Compress portal limits to a byte value.
                        if (dir == 0 || dir == 4)
                        {
                            float tmin = (connectPoly.tmin - tile.data.verts[va + 2])
                                         / (tile.data.verts[vb + 2] - tile.data.verts[va + 2]);
                            float tmax = (connectPoly.tmax - tile.data.verts[va + 2])
                                         / (tile.data.verts[vb + 2] - tile.data.verts[va + 2]);
                            if (tmin > tmax)
                            {
                                float temp = tmin;
                                tmin = tmax;
                                tmax = temp;
                            }

                            link.bmin = (int)Math.Round(Clamp(tmin, 0.0f, 1.0f) * 255.0f);
                            link.bmax = (int)Math.Round(Clamp(tmax, 0.0f, 1.0f) * 255.0f);
                        }
                        else if (dir == 2 || dir == 6)
                        {
                            float tmin = (connectPoly.tmin - tile.data.verts[va])
                                         / (tile.data.verts[vb] - tile.data.verts[va]);
                            float tmax = (connectPoly.tmax - tile.data.verts[va])
                                         / (tile.data.verts[vb] - tile.data.verts[va]);
                            if (tmin > tmax)
                            {
                                float temp = tmin;
                                tmin = tmax;
                                tmax = temp;
                            }

                            link.bmin = (int)Math.Round(Clamp(tmin, 0.0f, 1.0f) * 255.0f);
                            link.bmax = (int)Math.Round(Clamp(tmax, 0.0f, 1.0f) * 255.0f);
                        }
                    }
                }
            }
        }

        void ConnectExtOffMeshLinks(DtMeshTile tile, DtMeshTile target, int side)
        {
            if (tile == null)
            {
                return;
            }

            // Connect off-mesh links.
            // We are interested on links which land from target tile to this tile.
            int oppositeSide = (side == -1) ? 0xff : DtUtils.OppositeTile(side);

            for (int i = 0; i < target.data.header.offMeshConCount; ++i)
            {
                DtOffMeshConnection targetCon = target.data.offMeshCons[i];
                if (targetCon.side != oppositeSide)
                {
                    continue;
                }

                DtPoly targetPoly = target.data.polys[targetCon.poly];
                // Skip off-mesh connections which start location could not be
                // connected at all.
                if (target.polyLinks[targetPoly.index] == DT_NULL_LINK)
                {
                    continue;
                }

                var ext = new RcVec3f()
                {
                    x = targetCon.rad,
                    y = target.data.header.walkableClimb,
                    z = targetCon.rad
                };

                // Find polygon to connect to.
                RcVec3f p = new RcVec3f();
                p.x = targetCon.pos[3];
                p.y = targetCon.pos[4];
                p.z = targetCon.pos[5];
                var refs = FindNearestPolyInTile(tile, p, ext, out var nearestPt);
                if (refs == 0)
                {
                    continue;
                }

                // findNearestPoly may return too optimistic results, further check
                // to make sure.

                if (Sqr(nearestPt.x - p.x) + Sqr(nearestPt.z - p.z) > Sqr(targetCon.rad))
                {
                    continue;
                }

                // Make sure the location is on current mesh.
                target.data.verts[targetPoly.verts[1] * 3] = nearestPt.x;
                target.data.verts[targetPoly.verts[1] * 3 + 1] = nearestPt.y;
                target.data.verts[targetPoly.verts[1] * 3 + 2] = nearestPt.z;

                // Link off-mesh connection to target poly.
                int idx = AllocLink(target);
                DtLink link = target.links[idx];
                link.refs = refs;
                link.edge = 1;
                link.side = oppositeSide;
                link.bmin = link.bmax = 0;
                // Add to linked list.
                link.next = target.polyLinks[targetPoly.index];
                target.polyLinks[targetPoly.index] = idx;

                // Link target poly to off-mesh connection.
                if ((targetCon.flags & DT_OFFMESH_CON_BIDIR) != 0)
                {
                    int tidx = AllocLink(tile);
                    int landPolyIdx = DecodePolyIdPoly(refs);
                    DtPoly landPoly = tile.data.polys[landPolyIdx];
                    link = tile.links[tidx];
                    link.refs = GetPolyRefBase(target) | (long)targetCon.poly;
                    link.edge = 0xff;
                    link.side = (side == -1 ? 0xff : side);
                    link.bmin = link.bmax = 0;
                    // Add to linked list.
                    link.next = tile.polyLinks[landPoly.index];
                    tile.polyLinks[landPoly.index] = tidx;
                }
            }
        }

        private int FindConnectingPolys(float[] verts, int va, int vb, DtMeshTile tile, int side,
            ref List<DtConnectPoly> cons)
        {
            if (tile == null)
                return 0;

            cons.Clear();

            RcVec2f amin = RcVec2f.Zero;
            RcVec2f amax = RcVec2f.Zero;
            CalcSlabEndPoints(verts, va, vb, ref amin, ref amax, side);
            float apos = GetSlabCoord(verts, va, side);

            // Remove links pointing to 'side' and compact the links array.
            RcVec2f bmin = RcVec2f.Zero;
            RcVec2f bmax = RcVec2f.Zero;
            int m = DT_EXT_LINK | side;
            int n = 0;
            long @base = GetPolyRefBase(tile);

            for (int i = 0; i < tile.data.header.polyCount; ++i)
            {
                DtPoly poly = tile.data.polys[i];
                int nv = poly.vertCount;
                for (int j = 0; j < nv; ++j)
                {
                    // Skip edges which do not point to the right side.
                    if (poly.neis[j] != m)
                    {
                        continue;
                    }

                    int vc = poly.verts[j] * 3;
                    int vd = poly.verts[(j + 1) % nv] * 3;
                    float bpos = GetSlabCoord(tile.data.verts, vc, side);
                    // Segments are not close enough.
                    if (Math.Abs(apos - bpos) > 0.01f)
                    {
                        continue;
                    }

                    // Check if the segments touch.
                    CalcSlabEndPoints(tile.data.verts, vc, vd, ref bmin, ref bmax, side);

                    if (!OverlapSlabs(amin, amax, bmin, bmax, 0.01f, tile.data.header.walkableClimb))
                    {
                        continue;
                    }

                    // Add return value.
                    long refs = @base | (long)i;
                    float tmin = Math.Max(amin.x, bmin.x);
                    float tmax = Math.Min(amax.x, bmax.x);
                    cons.Add(new DtConnectPoly(refs, tmin, tmax));
                    n++;
                    break;
                }
            }

            return n;
        }

        static float GetSlabCoord(float[] verts, int va, int side)
        {
            if (side == 0 || side == 4)
            {
                return verts[va];
            }
            else if (side == 2 || side == 6)
            {
                return verts[va + 2];
            }

            return 0;
        }

        static void CalcSlabEndPoints(float[] verts, int va, int vb, ref RcVec2f bmin, ref RcVec2f bmax, int side)
        {
            if (side == 0 || side == 4)
            {
                if (verts[va + 2] < verts[vb + 2])
                {
                    bmin.x = verts[va + 2];
                    bmin.y = verts[va + 1];
                    bmax.x = verts[vb + 2];
                    bmax.y = verts[vb + 1];
                }
                else
                {
                    bmin.x = verts[vb + 2];
                    bmin.y = verts[vb + 1];
                    bmax.x = verts[va + 2];
                    bmax.y = verts[va + 1];
                }
            }
            else if (side == 2 || side == 6)
            {
                if (verts[va + 0] < verts[vb + 0])
                {
                    bmin.x = verts[va + 0];
                    bmin.y = verts[va + 1];
                    bmax.x = verts[vb + 0];
                    bmax.y = verts[vb + 1];
                }
                else
                {
                    bmin.x = verts[vb + 0];
                    bmin.y = verts[vb + 1];
                    bmax.x = verts[va + 0];
                    bmax.y = verts[va + 1];
                }
            }
        }

        bool OverlapSlabs(RcVec2f amin, RcVec2f amax, RcVec2f bmin, RcVec2f bmax, float px, float py)
        {
            // Check for horizontal overlap.
            // The segment is shrunken a little so that slabs which touch
            // at end points are not connected.
            float minx = Math.Max(amin.x + px, bmin.x + px);
            float maxx = Math.Min(amax.x - px, bmax.x - px);
            if (minx > maxx)
            {
                return false;
            }

            // Check vertical overlap.
            float ad = (amax.y - amin.y) / (amax.x - amin.x);
            float ak = amin.y - ad * amin.x;
            float bd = (bmax.y - bmin.y) / (bmax.x - bmin.x);
            float bk = bmin.y - bd * bmin.x;
            float aminy = ad * minx + ak;
            float amaxy = ad * maxx + ak;
            float bminy = bd * minx + bk;
            float bmaxy = bd * maxx + bk;
            float dmin = bminy - aminy;
            float dmax = bmaxy - amaxy;

            // Crossing segments always overlap.
            if (dmin * dmax < 0)
            {
                return true;
            }

            // Check for overlap at endpoints.
            float thr = (py * 2) * (py * 2);
            if (dmin * dmin <= thr || dmax * dmax <= thr)
            {
                return true;
            }

            return false;
        }

        /**
     * Builds internal polygons links for a tile.
     *
     * @param tile
     */
        void BaseOffMeshLinks(DtMeshTile tile)
        {
            if (tile == null)
            {
                return;
            }

            long @base = GetPolyRefBase(tile);

            // Base off-mesh connection start points.
            for (int i = 0; i < tile.data.header.offMeshConCount; ++i)
            {
                DtOffMeshConnection con = tile.data.offMeshCons[i];
                DtPoly poly = tile.data.polys[con.poly];

                var ext = new RcVec3f()
                {
                    x = con.rad,
                    y = tile.data.header.walkableClimb,
                    z = con.rad,
                };

                // Find polygon to connect to.
                var refs = FindNearestPolyInTile(tile, RcVec3f.Of(con.pos), ext, out var nearestPt);
                if (refs == 0)
                {
                    continue;
                }

                float[] p = con.pos; // First vertex
                // findNearestPoly may return too optimistic results, further check
                // to make sure.
                if (Sqr(nearestPt.x - p[0]) + Sqr(nearestPt.z - p[2]) > Sqr(con.rad))
                {
                    continue;
                }

                // Make sure the location is on current mesh.
                tile.data.verts[poly.verts[0] * 3] = nearestPt.x;
                tile.data.verts[poly.verts[0] * 3 + 1] = nearestPt.y;
                tile.data.verts[poly.verts[0] * 3 + 2] = nearestPt.z;

                // Link off-mesh connection to target poly.
                int idx = AllocLink(tile);
                DtLink link = tile.links[idx];
                link.refs = refs;
                link.edge = 0;
                link.side = 0xff;
                link.bmin = link.bmax = 0;
                // Add to linked list.
                link.next = tile.polyLinks[poly.index];
                tile.polyLinks[poly.index] = idx;

                // Start end-point is always connect back to off-mesh connection.
                int tidx = AllocLink(tile);
                int landPolyIdx = DecodePolyIdPoly(refs);
                DtPoly landPoly = tile.data.polys[landPolyIdx];
                link = tile.links[tidx];
                link.refs = @base | (long)con.poly;
                link.edge = 0xff;
                link.side = 0xff;
                link.bmin = link.bmax = 0;
                // Add to linked list.
                link.next = tile.polyLinks[landPoly.index];
                tile.polyLinks[landPoly.index] = tidx;
            }
        }

        /**
     * Returns closest point on polygon.
     *
     * @param ref
     * @param pos
     * @return
     */
        RcVec3f ClosestPointOnDetailEdges(DtMeshTile tile, DtPoly poly, RcVec3f pos, bool onlyBoundary)
        {
            int ANY_BOUNDARY_EDGE = (DT_DETAIL_EDGE_BOUNDARY << 0) | (DT_DETAIL_EDGE_BOUNDARY << 2)
                                                                   | (DT_DETAIL_EDGE_BOUNDARY << 4);
            int ip = poly.index;
            float dmin = float.MaxValue;
            float tmin = 0;
            RcVec3f pmin = new RcVec3f();
            RcVec3f pmax = new RcVec3f();

            if (tile.data.detailMeshes != null)
            {
                DtPolyDetail pd = tile.data.detailMeshes[ip];
                for (int i = 0; i < pd.triCount; i++)
                {
                    int ti = (pd.triBase + i) * 4;
                    int[] tris = tile.data.detailTris;
                    if (onlyBoundary && (tris[ti + 3] & ANY_BOUNDARY_EDGE) == 0)
                    {
                        continue;
                    }

                    RcVec3f[] v = new RcVec3f[3];
                    for (int j = 0; j < 3; ++j)
                    {
                        if (tris[ti + j] < poly.vertCount)
                        {
                            int index = poly.verts[tris[ti + j]] * 3;
                            v[j] = new RcVec3f
                            {
                                x = tile.data.verts[index],
                                y = tile.data.verts[index + 1],
                                z = tile.data.verts[index + 2]
                            };
                        }
                        else
                        {
                            int index = (pd.vertBase + (tris[ti + j] - poly.vertCount)) * 3;
                            v[j] = new RcVec3f
                            {
                                x = tile.data.detailVerts[index],
                                y = tile.data.detailVerts[index + 1],
                                z = tile.data.detailVerts[index + 2]
                            };
                        }
                    }

                    for (int k = 0, j = 2; k < 3; j = k++)
                    {
                        if ((GetDetailTriEdgeFlags(tris[ti + 3], j) & DT_DETAIL_EDGE_BOUNDARY) == 0
                            && (onlyBoundary || tris[ti + j] < tris[ti + k]))
                        {
                            // Only looking at boundary edges and this is internal, or
                            // this is an inner edge that we will see again or have already seen.
                            continue;
                        }

                        var d = DtUtils.DistancePtSegSqr2D(pos, v[j], v[k], out var t);
                        if (d < dmin)
                        {
                            dmin = d;
                            tmin = t;
                            pmin = v[j];
                            pmax = v[k];
                        }
                    }
                }
            }
            else
            {
                RcVec3f[] v = new RcVec3f[2];
                for (int j = 0; j < poly.vertCount; ++j)
                {
                    int k = (j + 1) % poly.vertCount;
                    v[0].x = tile.data.verts[poly.verts[j] * 3];
                    v[0].y = tile.data.verts[poly.verts[j] * 3 + 1];
                    v[0].z = tile.data.verts[poly.verts[j] * 3 + 2];
                    v[1].x = tile.data.verts[poly.verts[k] * 3];
                    v[1].y = tile.data.verts[poly.verts[k] * 3 + 1];
                    v[1].z = tile.data.verts[poly.verts[k] * 3 + 2];

                    var d = DtUtils.DistancePtSegSqr2D(pos, v[0], v[1], out var t);
                    if (d < dmin)
                    {
                        dmin = d;
                        tmin = t;
                        pmin = v[0];
                        pmax = v[1];
                    }
                }
            }

            return RcVec3f.Lerp(pmin, pmax, tmin);
        }

        public bool GetPolyHeight(DtMeshTile tile, DtPoly poly, RcVec3f pos, out float height)
        {
            height = 0;
            
            // Off-mesh connections do not have detail polys and getting height
            // over them does not make sense.
            if (poly.GetPolyType() == DtPoly.DT_POLYTYPE_OFFMESH_CONNECTION)
            {
                return false;
            }

            int ip = poly.index;

            float[] verts = new float[m_maxVertPerPoly * 3];
            int nv = poly.vertCount;
            for (int i = 0; i < nv; ++i)
            {
                Array.Copy(tile.data.verts, poly.verts[i] * 3, verts, i * 3, 3);
            }

            if (!DtUtils.PointInPolygon(pos, verts, nv))
            {
                return false;
            }

            // Find height at the location.
            if (tile.data.detailMeshes != null)
            {
                DtPolyDetail pd = tile.data.detailMeshes[ip];
                for (int j = 0; j < pd.triCount; ++j)
                {
                    int t = (pd.triBase + j) * 4;
                    RcVec3f[] v = new RcVec3f[3];
                    for (int k = 0; k < 3; ++k)
                    {
                        if (tile.data.detailTris[t + k] < poly.vertCount)
                        {
                            int index = poly.verts[tile.data.detailTris[t + k]] * 3;
                            v[k] = new RcVec3f
                            {
                                x = tile.data.verts[index],
                                y = tile.data.verts[index + 1],
                                z = tile.data.verts[index + 2]
                            };
                        }
                        else
                        {
                            int index = (pd.vertBase + (tile.data.detailTris[t + k] - poly.vertCount)) * 3;
                            v[k] = new RcVec3f
                            {
                                x = tile.data.detailVerts[index],
                                y = tile.data.detailVerts[index + 1],
                                z = tile.data.detailVerts[index + 2]
                            };
                        }
                    }

                    if (DtUtils.ClosestHeightPointTriangle(pos, v[0], v[1], v[2], out var h))
                    {
                        height = h;
                        return true;
                    }
                }
            }
            else
            {
                RcVec3f[] v = new RcVec3f[3];
                v[0].x = tile.data.verts[poly.verts[0] * 3];
                v[0].y = tile.data.verts[poly.verts[0] * 3 + 1];
                v[0].z = tile.data.verts[poly.verts[0] * 3 + 2];
                for (int j = 1; j < poly.vertCount - 1; ++j)
                {
                    for (int k = 0; k < 2; ++k)
                    {
                        v[k + 1].x = tile.data.verts[poly.verts[j + k] * 3];
                        v[k + 1].y = tile.data.verts[poly.verts[j + k] * 3 + 1];
                        v[k + 1].z = tile.data.verts[poly.verts[j + k] * 3 + 2];
                    }

                    if (DtUtils.ClosestHeightPointTriangle(pos, v[0], v[1], v[2], out var h))
                    {
                        height = h;
                        return true;
                    }
                }
            }

            // If all triangle checks failed above (can happen with degenerate triangles
            // or larger floating point values) the point is on an edge, so just select
            // closest. This should almost never happen so the extra iteration here is
            // ok.
            var closest = ClosestPointOnDetailEdges(tile, poly, pos, false);
            height = closest.y;
            return true;
        }

        public void ClosestPointOnPoly(long refs, RcVec3f pos, out RcVec3f closest, out bool posOverPoly)
        {
            GetTileAndPolyByRefUnsafe(refs, out var tile, out var poly);
            closest = pos;

            if (GetPolyHeight(tile, poly, pos, out var h))
            {
                closest.y = h;
                posOverPoly = true;
                return;
            }

            posOverPoly = false;

            // Off-mesh connections don't have detail polygons.
            if (poly.GetPolyType() == DtPoly.DT_POLYTYPE_OFFMESH_CONNECTION)
            {
                int i = poly.verts[0] * 3;
                var v0 = new RcVec3f { x = tile.data.verts[i], y = tile.data.verts[i + 1], z = tile.data.verts[i + 2] };
                i = poly.verts[1] * 3;
                var v1 = new RcVec3f { x = tile.data.verts[i], y = tile.data.verts[i + 1], z = tile.data.verts[i + 2] };
                DtUtils.DistancePtSegSqr2D(pos, v0, v1, out var t);
                closest = RcVec3f.Lerp(v0, v1, t);
                return;
            }

            // Outside poly that is not an offmesh connection.
            closest = ClosestPointOnDetailEdges(tile, poly, pos, true);
        }

        /// Find nearest polygon within a tile.
        private long FindNearestPolyInTile(DtMeshTile tile, RcVec3f center, RcVec3f halfExtents, out RcVec3f nearestPt)
        {
            nearestPt = RcVec3f.Zero;

            bool overPoly = false;
            RcVec3f bmin = center.Subtract(halfExtents);
            RcVec3f bmax = center.Add(halfExtents);

            // Get nearby polygons from proximity grid.
            List<long> polys = QueryPolygonsInTile(tile, bmin, bmax);

            // Find nearest polygon amongst the nearby polygons.
            long nearest = 0;
            float nearestDistanceSqr = float.MaxValue;
            for (int i = 0; i < polys.Count; ++i)
            {
                long refs = polys[i];
                float d;
                ClosestPointOnPoly(refs, center, out var closestPtPoly, out var posOverPoly);

                // If a point is directly over a polygon and closer than
                // climb height, favor that instead of straight line nearest point.
                RcVec3f diff = center.Subtract(closestPtPoly);
                if (posOverPoly)
                {
                    d = Math.Abs(diff.y) - tile.data.header.walkableClimb;
                    d = d > 0 ? d * d : 0;
                }
                else
                {
                    d = RcVec3f.LenSqr(diff);
                }

                if (d < nearestDistanceSqr)
                {
                    nearestPt = closestPtPoly;
                    nearestDistanceSqr = d;
                    nearest = refs;
                    overPoly = posOverPoly;
                }
            }

            return nearest;
        }

        DtMeshTile GetTileAt(int x, int y, int layer)
        {
            foreach (DtMeshTile tile in GetTileListByPos(x, y))
            {
                if (tile.data.header != null && tile.data.header.x == x && tile.data.header.y == y
                    && tile.data.header.layer == layer)
                {
                    return tile;
                }
            }

            return null;
        }

        List<DtMeshTile> GetNeighbourTilesAt(int x, int y, int side)
        {
            int nx = x, ny = y;
            switch (side)
            {
                case 0:
                    nx++;
                    break;
                case 1:
                    nx++;
                    ny++;
                    break;
                case 2:
                    ny++;
                    break;
                case 3:
                    nx--;
                    ny++;
                    break;
                case 4:
                    nx--;
                    break;
                case 5:
                    nx--;
                    ny--;
                    break;
                case 6:
                    ny--;
                    break;
                case 7:
                    nx++;
                    ny--;
                    break;
            }

            return GetTilesAt(nx, ny);
        }

        public List<DtMeshTile> GetTilesAt(int x, int y)
        {
            List<DtMeshTile> tiles = new List<DtMeshTile>();
            foreach (DtMeshTile tile in GetTileListByPos(x, y))
            {
                if (tile.data.header != null && tile.data.header.x == x && tile.data.header.y == y)
                {
                    tiles.Add(tile);
                }
            }

            return tiles;
        }

        public long GetTileRefAt(int x, int y, int layer)
        {
            return GetTileRef(GetTileAt(x, y, layer));
        }

        public DtMeshTile GetTileByRef(long refs)
        {
            if (refs == 0)
            {
                return null;
            }

            int tileIndex = DecodePolyIdTile(refs);
            int tileSalt = DecodePolyIdSalt(refs);
            if (tileIndex >= m_maxTiles)
            {
                return null;
            }

            DtMeshTile tile = m_tiles[tileIndex];
            if (tile.salt != tileSalt)
            {
                return null;
            }

            return tile;
        }

        public long GetTileRef(DtMeshTile tile)
        {
            if (tile == null)
            {
                return 0;
            }

            return EncodePolyId(tile.salt, tile.index, 0);
        }

        public static int ComputeTileHash(int x, int y, int mask)
        {
            uint h1 = 0x8da6b343; // Large multiplicative constants;
            uint h2 = 0xd8163841; // here arbitrarily chosen primes
            uint n = h1 * (uint)x + h2 * (uint)y;
            return (int)(n & mask);
        }

        /// Gets the endpoints for an off-mesh connection, ordered by "direction of travel".
        ///  @param[in]		prevRef		The reference of the polygon before the connection.
        ///  @param[in]		polyRef		The reference of the off-mesh connection polygon.
        ///  @param[out]	startPos	The start position of the off-mesh connection. [(x, y, z)]
        ///  @param[out]	endPos		The end position of the off-mesh connection. [(x, y, z)]
        /// @return The status flags for the operation.
        /// 
        /// @par
        ///
        /// Off-mesh connections are stored in the navigation mesh as special 2-vertex 
        /// polygons with a single edge. At least one of the vertices is expected to be 
        /// inside a normal polygon. So an off-mesh connection is "entered" from a 
        /// normal polygon at one of its endpoints. This is the polygon identified by 
        /// the prevRef parameter.
        public DtStatus GetOffMeshConnectionPolyEndPoints(long prevRef, long polyRef, ref RcVec3f startPos, ref RcVec3f endPos)
        {
            if (polyRef == 0)
            {
                return DtStatus.DT_FAILURE;
            }

            // Get current polygon
            DecodePolyId(polyRef, out var salt, out var it, out var ip);
            if (it >= m_maxTiles)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            if (m_tiles[it].salt != salt || m_tiles[it].data.header == null)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            DtMeshTile tile = m_tiles[it];
            if (ip >= tile.data.header.polyCount)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            DtPoly poly = tile.data.polys[ip];

            // Make sure that the current poly is indeed off-mesh link.
            if (poly.GetPolyType() != DtPoly.DT_POLYTYPE_OFFMESH_CONNECTION)
            {
                return DtStatus.DT_FAILURE;
            }

            // Figure out which way to hand out the vertices.
            int idx0 = 0, idx1 = 1;

            // Find link that points to first vertex.
            for (int i = tile.polyLinks[poly.index]; i != DT_NULL_LINK; i = tile.links[i].next)
            {
                if (tile.links[i].edge == 0)
                {
                    if (tile.links[i].refs != prevRef)
                    {
                        idx0 = 1;
                        idx1 = 0;
                    }

                    break;
                }
            }

            startPos = RcVec3f.Of(tile.data.verts, poly.verts[idx0] * 3);
            endPos = RcVec3f.Of(tile.data.verts, poly.verts[idx1] * 3);

            return DtStatus.DT_SUCCSESS;
        }

        public int GetMaxVertsPerPoly()
        {
            return m_maxVertPerPoly;
        }

        public int GetTileCount()
        {
            return m_tileCount;
        }

        public int GetAvailableTileCount()
        {
            return availableTiles.Count;
        }

        public DtStatus SetPolyFlags(long refs, int flags)
        {
            if (refs == 0)
            {
                return DtStatus.DT_FAILURE;
            }

            DecodePolyId(refs, out var salt, out var it, out var ip);
            if (it >= m_maxTiles)
            {
                return DtStatus.DT_INVALID_PARAM;
            }

            if (m_tiles[it].salt != salt || m_tiles[it].data == null || m_tiles[it].data.header == null)
            {
                return DtStatus.DT_INVALID_PARAM;
            }

            DtMeshTile tile = m_tiles[it];
            if (ip >= tile.data.header.polyCount)
            {
                return DtStatus.DT_INVALID_PARAM;
            }

            DtPoly poly = tile.data.polys[ip];

            // Change flags.
            poly.flags = flags;
            return DtStatus.DT_SUCCSESS;
        }

        /// Gets the user defined flags for the specified polygon.
        ///  @param[in]		ref				The polygon reference.
        ///  @param[out]	resultFlags		The polygon flags.
        /// @return The status flags for the operation.
        public DtStatus GetPolyFlags(long refs, out int resultFlags)
        {
            resultFlags = 0;

            if (refs == 0)
            {
                return DtStatus.DT_FAILURE;
            }

            DecodePolyId(refs, out var salt, out var it, out var ip);
            if (it >= m_maxTiles)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            if (m_tiles[it].salt != salt || m_tiles[it].data == null || m_tiles[it].data.header == null)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            DtMeshTile tile = m_tiles[it];
            if (ip >= tile.data.header.polyCount)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            DtPoly poly = tile.data.polys[ip];

            resultFlags = poly.flags;

            return DtStatus.DT_SUCCSESS;
        }

        public DtStatus SetPolyArea(long refs, char area)
        {
            if (refs == 0)
            {
                return DtStatus.DT_FAILURE;
            }

            DecodePolyId(refs, out var salt, out var it, out var ip);
            if (it >= m_maxTiles)
            {
                return DtStatus.DT_FAILURE;
            }

            if (m_tiles[it].salt != salt || m_tiles[it].data == null || m_tiles[it].data.header == null)
            {
                return DtStatus.DT_INVALID_PARAM;
            }

            DtMeshTile tile = m_tiles[it];
            if (ip >= tile.data.header.polyCount)
            {
                return DtStatus.DT_INVALID_PARAM;
            }

            DtPoly poly = tile.data.polys[ip];

            poly.SetArea(area);

            return DtStatus.DT_SUCCSESS;
        }

        public DtStatus GetPolyArea(long refs, out int resultArea)
        {
            resultArea = 0;

            if (refs == 0)
            {
                return DtStatus.DT_FAILURE;
            }

            DecodePolyId(refs, out var salt, out var it, out var ip);
            if (it >= m_maxTiles)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            if (m_tiles[it].salt != salt || m_tiles[it].data == null || m_tiles[it].data.header == null)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            DtMeshTile tile = m_tiles[it];
            if (ip >= tile.data.header.polyCount)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            DtPoly poly = tile.data.polys[ip];
            resultArea = poly.GetArea();

            return DtStatus.DT_SUCCSESS;
        }

        public RcVec3f GetPolyCenter(long refs)
        {
            RcVec3f center = RcVec3f.Zero;

            var status = GetTileAndPolyByRef(refs, out var tile, out var poly);
            if (status.Succeeded())
            {
                for (int i = 0; i < poly.vertCount; ++i)
                {
                    int v = poly.verts[i] * 3;
                    center.x += tile.data.verts[v];
                    center.y += tile.data.verts[v + 1];
                    center.z += tile.data.verts[v + 2];
                }

                float s = 1.0f / poly.vertCount;
                center.x *= s;
                center.y *= s;
                center.z *= s;
            }

            return center;
        }

        /**
     * Get flags for edge in detail triangle.
     *
     * @param triFlags
     *            The flags for the triangle (last component of detail vertices above).
     * @param edgeIndex
     *            The index of the first vertex of the edge. For instance, if 0,
     * @return flags for edge AB.
     */
        public static int GetDetailTriEdgeFlags(int triFlags, int edgeIndex)
        {
            return (triFlags >> (edgeIndex * 2)) & 0x3;
        }

        private List<DtMeshTile> GetTileListByPos(int x, int z)
        {
            var tileHash = ComputeTileHash(x, z, m_tileLutMask);
            if (!posLookup.TryGetValue(tileHash, out var tiles))
            {
                tiles = new List<DtMeshTile>();
                posLookup.Add(tileHash, tiles);
            }

            return tiles;
        }

        public void ComputeBounds(out RcVec3f bmin, out RcVec3f bmax)
        {
            bmin = RcVec3f.Of(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            bmax = RcVec3f.Of(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            for (int t = 0; t < GetMaxTiles(); ++t)
            {
                DtMeshTile tile = GetTile(t);
                if (tile != null && tile.data != null)
                {
                    for (int i = 0; i < tile.data.verts.Length; i += 3)
                    {
                        bmin.x = Math.Min(bmin.x, tile.data.verts[i]);
                        bmin.y = Math.Min(bmin.y, tile.data.verts[i + 1]);
                        bmin.z = Math.Min(bmin.z, tile.data.verts[i + 2]);
                        bmax.x = Math.Max(bmax.x, tile.data.verts[i]);
                        bmax.y = Math.Max(bmax.y, tile.data.verts[i + 1]);
                        bmax.z = Math.Max(bmax.z, tile.data.verts[i + 2]);
                    }
                }
            }
        }
    }
}