using System;
using System.Collections.Generic;
using DotRecast.Core;

namespace DotRecast.Detour.Extras.Jumplink
{
    class JumpSegmentBuilder
    {
        public JumpSegment[] Build(JumpLinkBuilderConfig acfg, EdgeSampler es)
        {
            int n = es.end[0].gsamples.Length;
            int[][] sampleGrid = RcArrayUtils.Of<int>(n, es.end.Count);
            for (int j = 0; j < es.end.Count; j++)
            {
                for (int i = 0; i < n; i++)
                {
                    sampleGrid[i][j] = -1;
                }
            }

            // Fill connected regions
            int region = 0;
            for (int j = 0; j < es.end.Count; j++)
            {
                for (int i = 0; i < n; i++)
                {
                    if (sampleGrid[i][j] == -1)
                    {
                        GroundSample p = es.end[j].gsamples[i];
                        if (!p.validTrajectory)
                        {
                            sampleGrid[i][j] = -2;
                        }
                        else
                        {
                            var queue = new Queue<int[]>();
                            queue.Enqueue(new int[] { i, j });
                            Fill(es, sampleGrid, queue, acfg.agentClimb, region);
                            region++;
                        }
                    }
                }
            }

            JumpSegment[] jumpSegments = new JumpSegment[region];
            for (int i = 0; i < jumpSegments.Length; i++)
            {
                jumpSegments[i] = new JumpSegment();
            }

            // Find longest segments per region
            for (int j = 0; j < es.end.Count; j++)
            {
                int l = 0;
                int r = -2;
                for (int i = 0; i < n + 1; i++)
                {
                    if (i == n || sampleGrid[i][j] != r)
                    {
                        if (r >= 0)
                        {
                            if (jumpSegments[r].samples < l)
                            {
                                jumpSegments[r].samples = l;
                                jumpSegments[r].startSample = i - l;
                                jumpSegments[r].groundSegment = j;
                            }
                        }

                        if (i < n)
                        {
                            r = sampleGrid[i][j];
                        }

                        l = 1;
                    }
                    else
                    {
                        l++;
                    }
                }
            }

            return jumpSegments;
        }

        private void Fill(EdgeSampler es, int[][] sampleGrid, Queue<int[]> queue, float agentClimb, int region)
        {
            while (queue.TryDequeue(out var ij))
            {
                int i = ij[0];
                int j = ij[1];
                if (sampleGrid[i][j] == -1)
                {
                    GroundSample p = es.end[j].gsamples[i];
                    sampleGrid[i][j] = region;
                    float h = p.p.y;
                    if (i < sampleGrid.Length - 1)
                    {
                        AddNeighbour(es, queue, agentClimb, h, i + 1, j);
                    }

                    if (i > 0)
                    {
                        AddNeighbour(es, queue, agentClimb, h, i - 1, j);
                    }

                    if (j < sampleGrid[0].Length - 1)
                    {
                        AddNeighbour(es, queue, agentClimb, h, i, j + 1);
                    }

                    if (j > 0)
                    {
                        AddNeighbour(es, queue, agentClimb, h, i, j - 1);
                    }
                }
            }
        }

        private void AddNeighbour(EdgeSampler es, Queue<int[]> queue, float agentClimb, float h, int i, int j)
        {
            GroundSample q = es.end[j].gsamples[i];
            if (q.validTrajectory && Math.Abs(q.p.y - h) < agentClimb)
            {
                queue.Enqueue(new int[] { i, j });
            }
        }
    }
}
