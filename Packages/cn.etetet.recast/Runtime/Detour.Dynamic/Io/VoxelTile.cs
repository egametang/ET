/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org
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

using DotRecast.Core;
using DotRecast.Recast;

namespace DotRecast.Detour.Dynamic.Io
{
    public class VoxelTile
    {
        private const int SERIALIZED_SPAN_COUNT_BYTES = 2;
        private const int SERIALIZED_SPAN_BYTES = 12;
        public readonly int tileX;
        public readonly int tileZ;
        public readonly int borderSize;
        public int width;
        public int depth;
        public readonly RcVec3f boundsMin;
        public RcVec3f boundsMax;
        public float cellSize;
        public float cellHeight;
        public readonly byte[] spanData;

        public VoxelTile(int tileX, int tileZ, int width, int depth, RcVec3f boundsMin, RcVec3f boundsMax, float cellSize,
            float cellHeight, int borderSize, RcByteBuffer buffer)
        {
            this.tileX = tileX;
            this.tileZ = tileZ;
            this.width = width;
            this.depth = depth;
            this.boundsMin = boundsMin;
            this.boundsMax = boundsMax;
            this.cellSize = cellSize;
            this.cellHeight = cellHeight;
            this.borderSize = borderSize;
            spanData = ToByteArray(buffer, width, depth, VoxelFile.PREFERRED_BYTE_ORDER);
        }

        public VoxelTile(int tileX, int tileZ, RcHeightfield heightfield)
        {
            this.tileX = tileX;
            this.tileZ = tileZ;
            width = heightfield.width;
            depth = heightfield.height;
            boundsMin = heightfield.bmin;
            boundsMax = heightfield.bmax;
            cellSize = heightfield.cs;
            cellHeight = heightfield.ch;
            borderSize = heightfield.borderSize;
            spanData = SerializeSpans(heightfield, VoxelFile.PREFERRED_BYTE_ORDER);
        }

        public RcHeightfield Heightfield()
        {
            return VoxelFile.PREFERRED_BYTE_ORDER == RcByteOrder.BIG_ENDIAN ? HeightfieldBE() : HeightfieldLE();
        }

        private RcHeightfield HeightfieldBE()
        {
            RcHeightfield hf = new RcHeightfield(width, depth, boundsMin, boundsMax, cellSize, cellHeight, borderSize);
            int position = 0;
            for (int z = 0, pz = 0; z < depth; z++, pz += width)
            {
                for (int x = 0; x < width; x++)
                {
                    RcSpan prev = null;
                    int spanCount = ByteUtils.GetShortBE(spanData, position);
                    position += 2;
                    for (int s = 0; s < spanCount; s++)
                    {
                        RcSpan span = new RcSpan();
                        span.smin = ByteUtils.GetIntBE(spanData, position);
                        position += 4;
                        span.smax = ByteUtils.GetIntBE(spanData, position);
                        position += 4;
                        span.area = ByteUtils.GetIntBE(spanData, position);
                        position += 4;
                        if (prev == null)
                        {
                            hf.spans[pz + x] = span;
                        }
                        else
                        {
                            prev.next = span;
                        }

                        prev = span;
                    }
                }
            }

            return hf;
        }

        private RcHeightfield HeightfieldLE()
        {
            RcHeightfield hf = new RcHeightfield(width, depth, boundsMin, boundsMax, cellSize, cellHeight, borderSize);
            int position = 0;
            for (int z = 0, pz = 0; z < depth; z++, pz += width)
            {
                for (int x = 0; x < width; x++)
                {
                    RcSpan prev = null;
                    int spanCount = ByteUtils.GetShortLE(spanData, position);
                    position += 2;
                    for (int s = 0; s < spanCount; s++)
                    {
                        RcSpan span = new RcSpan();
                        span.smin = ByteUtils.GetIntLE(spanData, position);
                        position += 4;
                        span.smax = ByteUtils.GetIntLE(spanData, position);
                        position += 4;
                        span.area = ByteUtils.GetIntLE(spanData, position);
                        position += 4;
                        if (prev == null)
                        {
                            hf.spans[pz + x] = span;
                        }
                        else
                        {
                            prev.next = span;
                        }

                        prev = span;
                    }
                }
            }

            return hf;
        }

        private byte[] SerializeSpans(RcHeightfield heightfield, RcByteOrder order)
        {
            int[] counts = new int[heightfield.width * heightfield.height];
            int totalCount = 0;
            for (int z = 0, pz = 0; z < heightfield.height; z++, pz += heightfield.width)
            {
                for (int x = 0; x < heightfield.width; x++)
                {
                    RcSpan span = heightfield.spans[pz + x];
                    while (span != null)
                    {
                        counts[pz + x]++;
                        totalCount++;
                        span = span.next;
                    }
                }
            }

            byte[] data = new byte[totalCount * SERIALIZED_SPAN_BYTES + counts.Length * SERIALIZED_SPAN_COUNT_BYTES];
            int position = 0;
            for (int z = 0, pz = 0; z < heightfield.height; z++, pz += heightfield.width)
            {
                for (int x = 0; x < heightfield.width; x++)
                {
                    position = ByteUtils.PutShort(counts[pz + x], data, position, order);
                    RcSpan span = heightfield.spans[pz + x];
                    while (span != null)
                    {
                        position = ByteUtils.PutInt(span.smin, data, position, order);
                        position = ByteUtils.PutInt(span.smax, data, position, order);
                        position = ByteUtils.PutInt(span.area, data, position, order);
                        span = span.next;
                    }
                }
            }

            return data;
        }

        private byte[] ToByteArray(RcByteBuffer buf, int width, int height, RcByteOrder order)
        {
            byte[] data;
            if (buf.Order() == order)
            {
                data = buf.ReadBytes(buf.Limit()).ToArray();
            }
            else
            {
                data = new byte[buf.Limit()];
                int l = width * height;
                int position = 0;
                for (int i = 0; i < l; i++)
                {
                    int count = buf.GetShort();
                    ByteUtils.PutShort(count, data, position, order);
                    position += 2;
                    for (int j = 0; j < count; j++)
                    {
                        ByteUtils.PutInt(buf.GetInt(), data, position, order);
                        position += 4;
                        ByteUtils.PutInt(buf.GetInt(), data, position, order);
                        position += 4;
                        ByteUtils.PutInt(buf.GetInt(), data, position, order);
                        position += 4;
                    }
                }
            }

            return data;
        }
    }
}