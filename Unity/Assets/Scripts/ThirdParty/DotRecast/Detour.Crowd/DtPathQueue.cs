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

using System.Collections.Generic;
using DotRecast.Core;


namespace DotRecast.Detour.Crowd
{
    public class DtPathQueue
    {
        private readonly DtCrowdConfig config;
        private readonly LinkedList<DtPathQuery> queue = new LinkedList<DtPathQuery>();

        public DtPathQueue(DtCrowdConfig config)
        {
            this.config = config;
        }

        public void Update(DtNavMesh navMesh)
        {
            // Update path request until there is nothing to update or up to maxIters pathfinder iterations has been
            // consumed.
            int iterCount = config.maxFindPathIterations;
            while (iterCount > 0)
            {
                DtPathQuery q = queue.First?.Value;
                if (q == null)
                {
                    break;
                }

                queue.RemoveFirst();

                // Handle query start.
                if (q.result.status.IsEmpty())
                {
                    q.navQuery = new DtNavMeshQuery(navMesh);
                    q.result.status = q.navQuery.InitSlicedFindPath(q.startRef, q.endRef, q.startPos, q.endPos, q.filter, 0);
                }

                // Handle query in progress.
                if (q.result.status.InProgress())
                {
                    q.result.status = q.navQuery.UpdateSlicedFindPath(iterCount, out var iters);
                    iterCount -= iters;
                }

                if (q.result.status.Succeeded())
                {
                    q.result.status = q.navQuery.FinalizeSlicedFindPath(ref q.result.path);
                }

                if (!(q.result.status.Failed() || q.result.status.Succeeded()))
                {
                    queue.AddFirst(q);
                }
            }
        }

        public DtPathQueryResult Request(long startRef, long endRef, RcVec3f startPos, RcVec3f endPos, IDtQueryFilter filter)
        {
            if (queue.Count >= config.pathQueueSize)
            {
                return null;
            }

            DtPathQuery q = new DtPathQuery();
            q.startPos = startPos;
            q.startRef = startRef;
            q.endPos = endPos;
            q.endRef = endRef;
            q.filter = filter;
            queue.AddLast(q);
            return q.result;
        }
    }
}