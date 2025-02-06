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

namespace DotRecast.Recast
{
    public static class RecastCommon
    {
        private static readonly int[] DirOffsetX = { -1, 0, 1, 0, };
        private static readonly int[] DirOffsetY = { 0, 1, 0, -1 };
        private static readonly int[] DirForOffset = { 3, 0, -1, 2, 1 };

        /// Sets the neighbor connection data for the specified direction.
        /// @param[in]		span			The span to update.
        /// @param[in]		direction		The direction to set. [Limits: 0 <= value < 4]
        /// @param[in]		neighborIndex	The index of the neighbor span.
        public static void SetCon(RcCompactSpan span, int direction, int neighborIndex)
        {
            int shift = direction * 6;
            int con = span.con;
            span.con = (con & ~(0x3f << shift)) | ((neighborIndex & 0x3f) << shift);
        }

        /// Gets neighbor connection data for the specified direction.
        /// @param[in]		span		The span to check.
        /// @param[in]		direction	The direction to check. [Limits: 0 <= value < 4]
        /// @return The neighbor connection data for the specified direction, or #RC_NOT_CONNECTED if there is no connection.
        public static int GetCon(RcCompactSpan s, int dir)
        {
            int shift = dir * 6;
            return (s.con >> shift) & 0x3f;
        }

        /// Gets the standard width (x-axis) offset for the specified direction.
        /// @param[in]		direction		The direction. [Limits: 0 <= value < 4]
        /// @return The width offset to apply to the current cell position to move in the direction.
        public static int GetDirOffsetX(int dir)
        {
            return DirOffsetX[dir & 0x03];
        }

        // TODO (graham): Rename this to rcGetDirOffsetZ
        /// Gets the standard height (z-axis) offset for the specified direction.
        /// @param[in]		direction		The direction. [Limits: 0 <= value < 4]
        /// @return The height offset to apply to the current cell position to move in the direction.
        public static int GetDirOffsetY(int dir)
        {
            return DirOffsetY[dir & 0x03];
        }

        /// Gets the direction for the specified offset. One of x and y should be 0.
        /// @param[in]		offsetX		The x offset. [Limits: -1 <= value <= 1]
        /// @param[in]		offsetZ		The z offset. [Limits: -1 <= value <= 1]
        /// @return The direction that represents the offset.
        public static int GetDirForOffset(int x, int y)
        {
            return DirForOffset[((y + 1) << 1) + x];
        }
    }
}