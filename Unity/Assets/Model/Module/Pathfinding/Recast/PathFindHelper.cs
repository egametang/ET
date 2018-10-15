using PF;

namespace PF
{
    /** What data to draw the graph debugging with */
    public enum GraphDebugMode {
        Areas,
        G,
        H,
        F,
        Penalty,
        Connections,
        Tags
    }
    
    /** How path results are logged by the system */
    public enum PathLog {
        /** Does not log anything. This is recommended for release since logging path results has a performance overhead. */
        None,
        /** Logs basic info about the paths */
        Normal,
        /** Includes additional info */
        Heavy,
        /** Same as heavy, but displays the info in-game using GUI */
        InGame,
        /** Same as normal, but logs only paths which returned an error */
        OnlyErrors
    }
    
    /** How to estimate the cost from to the destination.
	 *
	 * The heuristic is the estimated cost from the current node to the target.
	 * The different heuristics have roughly the same performance except not using any heuristic at all (#None)
	 * which is usually significantly slower.
	 *
	 * In the image below you can see a comparison of the different heuristic options for an 8-connected grid and
	 * for a 4-connected grid.
	 * Note that all paths within the green area will all have the same length. The only difference between the heuristics
	 * is which of those paths of the same length that will be chosen.
	 * Note that while the Diagonal Manhattan and Manhattan options seem to behave very differently on an 8-connected grid
	 * they only do it in this case because of very small rounding errors. Usually they behave almost identically on 8-connected grids.
	 *
	 * \shadowimage{heuristic.png}
	 *
	 * Generally for a 4-connected grid graph the Manhattan option should be used as it is the true distance on a 4-connected grid.
	 * For an 8-connected grid graph the Diagonal Manhattan option is the mathematically most correct option, however the Euclidean option
	 * is often preferred, especially if you are simplifying the path afterwards using modifiers.
	 *
	 * For any graph that is not grid based the Euclidean option is the best one to use.
	 *
	 * \see <a href="https://en.wikipedia.org/wiki/A*_search_algorithm">Wikipedia: A* search_algorithm</a>
	 */
	public enum Heuristic {
		/** Manhattan distance. \see https://en.wikipedia.org/wiki/Taxicab_geometry */
		Manhattan,
		/** Manhattan distance, but allowing diagonal movement as well.
		 * \note This option is currently hard coded for the XZ plane. It will be equivalent to Manhattan distance if you try to use it in the XY plane (i.e for a 2D game).
		 */
		DiagonalManhattan,
		/** Ordinary distance. \see https://en.wikipedia.org/wiki/Euclidean_distance */
		Euclidean,
		/** Use no heuristic at all.
		 * This reduces the pathfinding algorithm to Dijkstra's algorithm.
		 * This is usually significantly slower compared to using a heuristic, which is why the A* algorithm is usually preferred over Dijkstra's algorithm.
		 * You may have to use this if you have a very non-standard graph. For example a world with a <a href="https://en.wikipedia.org/wiki/Wraparound_(video_games)">wraparound playfield</a> (think Civilization or Asteroids) and you have custom links
		 * with a zero cost from one end of the map to the other end. Usually the A* algorithm wouldn't find the wraparound links because it wouldn't think to look in that direction.
		 * \see https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
		 */
		None
	}
    
    
    
    public static class PathFindHelper
    {
#if !SERVER
        public static AstarPath GetConfig()
        {
            return AstarPath.active;
        }
#else
        public static AStarConfig GetConfig()
        {
            return AStarConfig.Instance;
        }
#endif
        
        /**
       * Returns the nearest node to a position using the specified NNConstraint.
       * Searches through all graphs for their nearest nodes to the specified position and picks the closest one.\n
       * Using the NNConstraint.None constraint.
       *
       * \snippet MiscSnippets.cs AstarPath.GetNearest1
       *
       * \see Pathfinding.NNConstraint
       */
        public static NNInfo GetNearest(Vector3 position)
        {
            return GetNearest(position, NNConstraint.None);
        }

        /**
         * Returns the nearest node to a position using the specified NNConstraint.
         * Searches through all graphs for their nearest nodes to the specified position and picks the closest one.
         * The NNConstraint can be used to specify constraints on which nodes can be chosen such as only picking walkable nodes.
         *
         * \snippet MiscSnippets.cs AstarPath.GetNearest2
         *
         * \snippet MiscSnippets.cs AstarPath.GetNearest3
         *
         * \see Pathfinding.NNConstraint
         */
        public static NNInfo GetNearest(Vector3 position, NNConstraint constraint)
        {
            return GetNearest(position, constraint, null);
        }

        /**
         * Returns the nearest node to a position using the specified NNConstraint.
         * Searches through all graphs for their nearest nodes to the specified position and picks the closest one.
         * The NNConstraint can be used to specify constraints on which nodes can be chosen such as only picking walkable nodes.
         * \see Pathfinding.NNConstraint
         */
        public static NNInfo GetNearest(Vector3 position, NNConstraint constraint, GraphNode hint)
        {
            // Cache property lookup
            var localGraphs = GetConfig().graphs;

            float minDist = float.PositiveInfinity;
            NNInfoInternal nearestNode = new NNInfoInternal();
            int nearestGraph = -1;

            if (localGraphs != null)
            {
                for (int i = 0; i < localGraphs.Length; i++)
                {
                    NavGraph graph = localGraphs[i];

                    // Check if this graph should be searched
                    if (graph == null || !constraint.SuitableGraph(i, graph))
                    {
                        continue;
                    }

                    NNInfoInternal nnInfo;
                    if (GetConfig().fullGetNearestSearch)
                    {
                        // Slower nearest node search
                        // this will try to find a node which is suitable according to the constraint
                        nnInfo = graph.GetNearestForce(position, constraint);
                    }
                    else
                    {
                        // Fast nearest node search
                        // just find a node close to the position without using the constraint that much
                        // (unless that comes essentially 'for free')
                        nnInfo = graph.GetNearest(position, constraint);
                    }

                    GraphNode node = nnInfo.node;

                    // No node found in this graph
                    if (node == null)
                    {
                        continue;
                    }

                    // Distance to the closest point on the node from the requested position
                    float dist = ((Vector3) nnInfo.clampedPosition - position).magnitude;

                    if (GetConfig().prioritizeGraphs && dist < GetConfig().prioritizeGraphsLimit)
                    {
                        // The node is close enough, choose this graph and discard all others
                        minDist = dist;
                        nearestNode = nnInfo;
                        nearestGraph = i;
                        break;
                    }
                    else
                    {
                        // Choose the best node found so far
                        if (dist < minDist)
                        {
                            minDist = dist;
                            nearestNode = nnInfo;
                            nearestGraph = i;
                        }
                    }
                }
            }

            // No matches found
            if (nearestGraph == -1)
            {
                return new NNInfo();
            }

            // Check if a constrained node has already been set
            if (nearestNode.constrainedNode != null)
            {
                nearestNode.node = nearestNode.constrainedNode;
                nearestNode.clampedPosition = nearestNode.constClampedPosition;
            }

            if (!GetConfig().fullGetNearestSearch && nearestNode.node != null && !constraint.Suitable(nearestNode.node))
            {
                // Otherwise, perform a check to force the graphs to check for a suitable node
                NNInfoInternal nnInfo = localGraphs[nearestGraph].GetNearestForce(position, constraint);

                if (nnInfo.node != null)
                {
                    nearestNode = nnInfo;
                }
            }

            if (!constraint.Suitable(nearestNode.node) || (constraint.constrainDistance &&
                (nearestNode.clampedPosition - position).sqrMagnitude > GetConfig().maxNearestNodeDistanceSqr))
            {
                return new NNInfo();
            }

            // Convert to NNInfo which doesn't have all the internal fields
            return new NNInfo(nearestNode);
        }

        public static PathHandler debugPathData
        {
            get
            {
                return GetConfig().debugPathData;
            }
            set
            {
                GetConfig().debugPathData = value;
            }
        }
        
        public static ushort debugPathID
        {
            get
            {
                return GetConfig().debugPathID;
            }
            set
            {
                GetConfig().debugPathID = value;
            }
        }

        public static PathProcessor pathProcessor
        {
            get
            {
                return GetConfig().pathProcessor;
            }
            set
            {
                GetConfig().pathProcessor = value;
            }
        }
        
        public static bool IsUsingMultithreading {
            get {
                //return GetConfig().pathProcessor.IsUsingMultithreading;
                return false;
            }
        }
        
        public static NavGraph[] graphs 
        {
            get {
                return GetConfig().graphs;
            }
        }
        
        public static Heuristic heuristic
        {
            get
            {
                return GetConfig().heuristic;
            }
            set
            {
                GetConfig().heuristic = value;
            }
        }
        
        public static float heuristicScale
        {
            get
            {
                return GetConfig().heuristicScale;
            }
            set
            {
                GetConfig().heuristicScale = value;
            }
        }
        
        public static EuclideanEmbedding euclideanEmbedding
        {
            get
            {
                return GetConfig().euclideanEmbedding;
            }
            set
            {
                GetConfig().euclideanEmbedding = value;
            }
        }
        
        public static PathLog logPathResults
        {
            get
            {
                return GetConfig().logPathResults;
            }
            set
            {
                GetConfig().logPathResults = value;
            }
        }
        
        /** Returns a new global node index.
 * \warning This method should not be called directly. It is used by the GraphNode constructor.
 */
        public static int GetNewNodeIndex () {
            return pathProcessor.GetNewNodeIndex();
        }
        
        /** Initializes temporary path data for a node.
 * \warning This method should not be called directly. It is used by the GraphNode constructor.
 */
        public static void InitializeNode (GraphNode node) {
            pathProcessor.InitializeNode(node);
        }
        
        /** @name Callbacks */
	    /* Callbacks to pathfinding events.
	     * These allow you to hook in to the pathfinding process.\n
	     * Callbacks can be used like this:
	     * \snippet MiscSnippets.cs AstarPath.Callbacks
	     */
	    /** @{ */
    
	    /** Called on Awake before anything else is done.
	     * This is called at the start of the Awake call, right after #active has been set, but this is the only thing that has been done.\n
	     * Use this when you want to set up default settings for an AstarPath component created during runtime since some settings can only be changed in Awake
	     * (such as multithreading related stuff)
	     * \snippet MiscSnippets.cs AstarPath.OnAwakeSettings
	     */
	    public static System.Action OnAwakeSettings;
    
	    /** Called for each path before searching. Be careful when using multithreading since this will be called from a different thread. */
	    public static OnPathDelegate OnPathPreSearch;
    
	    /** Called for each path after searching. Be careful when using multithreading since this will be called from a different thread. */
	    public static OnPathDelegate OnPathPostSearch;

    
	    /**
	     * Called when \a pathID overflows 65536 and resets back to zero.
	     * \note This callback will be cleared every time it is called, so if you want to register to it repeatedly, register to it directly on receiving the callback as well.
	     */
	    public static System.Action On65KOverflow;
        
        
        /** The next unused Path ID.
           * Incremented for every call to GetNextPathID
        */
        private static ushort nextFreePathID = 1;
        /** Returns the next free path ID */
        public static ushort GetNextPathID () {
            if (nextFreePathID == 0) {
                nextFreePathID++;

                if (On65KOverflow != null) {
                    System.Action tmp = On65KOverflow;
                    On65KOverflow = null;
                    tmp();
                }
            }
            return nextFreePathID++;
        }

        public static void Close()
        {
            OnAwakeSettings         = null;
            OnPathPreSearch         = null;
            OnPathPostSearch        = null;
            On65KOverflow           = null;
        }
    }
}