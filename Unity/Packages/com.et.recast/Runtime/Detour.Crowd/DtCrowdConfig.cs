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

namespace DotRecast.Detour.Crowd
{
    public class DtCrowdConfig
    {
        public readonly float maxAgentRadius;

        /**
     * Max number of path requests in the queue
     */
        public int pathQueueSize = 32;

        /**
     * Max number of sliced path finding iterations executed per update (used to handle longer paths and replans)
     */
        public int maxFindPathIterations = 100;

        /**
     * Max number of sliced path finding iterations executed per agent to find the initial path to target
     */
        public int maxTargetFindPathIterations = 20;

        /**
     * Min time between topology optimizations (in seconds)
     */
        public float topologyOptimizationTimeThreshold = 0.5f;

        /**
     * The number of polygons from the beginning of the corridor to check to ensure path validity
     */
        public int checkLookAhead = 10;

        /**
     * Min time between target re-planning (in seconds)
     */
        public float targetReplanDelay = 1.0f;

        /**
     * Max number of sliced path finding iterations executed per topology optimization per agent
     */
        public int maxTopologyOptimizationIterations = 32;

        public float collisionResolveFactor = 0.7f;

        /**
     * Max number of neighbour agents to consider in obstacle avoidance processing
     */
        public int maxObstacleAvoidanceCircles = 6;

        /**
     * Max number of neighbour segments to consider in obstacle avoidance processing
     */
        public int maxObstacleAvoidanceSegments = 8;

        public DtCrowdConfig(float maxAgentRadius)
        {
            this.maxAgentRadius = maxAgentRadius;
        }
    }
}