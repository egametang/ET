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

using System.Collections.Generic;

namespace DotRecast.Detour.Dynamic
{
    public class RemoveColliderQueueItem : IUpdateQueueItem
    {
        private readonly long colliderId;
        private readonly ICollection<DynamicTile> _affectedTiles;

        public RemoveColliderQueueItem(long colliderId, ICollection<DynamicTile> affectedTiles)
        {
            this.colliderId = colliderId;
            this._affectedTiles = affectedTiles;
        }

        public ICollection<DynamicTile> AffectedTiles()
        {
            return _affectedTiles;
        }

        public void Process(DynamicTile tile)
        {
            tile.RemoveCollider(colliderId);
        }
    }
}