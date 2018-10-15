using PF;

namespace PF {
	/** Basic path, finds the shortest path from A to B.
	 * \ingroup paths
	 * This is the most basic path object it will try to find the shortest path between two points.\n
	 * Many other path types inherit from this type.
	 * \see Seeker.StartPath
	 * \see \ref calling-pathfinding
	 * \see \ref getstarted
	 */
	public class ABPath : Path {
		/** Start node of the path */
		public GraphNode startNode;

		/** End node of the path */
		public GraphNode endNode;

		/** Start Point exactly as in the path request */
		public Vector3 originalStartPoint;

		/** End Point exactly as in the path request */
		public Vector3 originalEndPoint;

		/** Start point of the path.
		 * This is the closest point on the #startNode to #originalStartPoint
		 */
		public Vector3 startPoint;

		/** End point of the path.
		 * This is the closest point on the #endNode to #originalEndPoint
		 */
		public Vector3 endPoint;

		/** Determines if a search for an end node should be done.
		 * Set by different path types.
		 * \since Added in 3.0.8.3
		 */
		protected virtual bool hasEndPoint {
			get {
				return true;
			}
		}

		public Int3 startIntPoint; /**< Start point in integer coordinates */

		/** Calculate partial path if the target node cannot be reached.
		 * If the target node cannot be reached, the node which was closest (given by heuristic) will be chosen as target node
		 * and a partial path will be returned.
		 * This only works if a heuristic is used (which is the default).
		 * If a partial path is found, CompleteState is set to Partial.
		 * \note It is not required by other path types to respect this setting
		 *
		 * \warning This feature is currently a work in progress and may not work in the current version
		 */
		public bool calculatePartial;

		/** Current best target for the partial path.
		 * This is the node with the lowest H score.
		 * \warning This feature is currently a work in progress and may not work in the current version
		 */
		protected PathNode partialBestTarget;

		/** Saved original costs for the end node. \see ResetCosts */
		protected int[] endNodeCosts;

		/** @{ @name Constructors */

		/** Default constructor.
		 * Do not use this. Instead use the static Construct method which can handle path pooling.
		 */
		public ABPath () {}

		/** Construct a path with a start and end point.
		 * The delegate will be called when the path has been calculated.
		 * Do not confuse it with the Seeker callback as they are sent at different times.
		 * If you are using a Seeker to start the path you can set \a callback to null.
		 *
		 * \returns The constructed path object
		 */
		public static ABPath Construct (Vector3 start, Vector3 end, OnPathDelegate callback = null) {
			var p = PathPool.GetPath<ABPath>();

			p.Setup(start, end, callback);
			return p;
		}

		protected void Setup (Vector3 start, Vector3 end, OnPathDelegate callbackDelegate) {
			callback = callbackDelegate;
			UpdateStartEnd(start, end);
		}

		/** @} */

		/** Sets the start and end points.
		 * Sets #originalStartPoint, #originalEndPoint, #startPoint, #endPoint, #startIntPoint and #hTarget (to \a end ) */
		protected void UpdateStartEnd (Vector3 start, Vector3 end) {
			originalStartPoint = start;
			originalEndPoint = end;

			startPoint = start;
			endPoint = end;

			startIntPoint = (Int3)start;
			hTarget = (Int3)end;
		}

		internal override uint GetConnectionSpecialCost (GraphNode a, GraphNode b, uint currentCost) {
			if (startNode != null && endNode != null) {
				if (a == startNode) {
					return (uint)((startIntPoint - (b == endNode ? hTarget : b.position)).costMagnitude * (currentCost*1.0/(a.position-b.position).costMagnitude));
				}
				if (b == startNode) {
					return (uint)((startIntPoint - (a == endNode ? hTarget : a.position)).costMagnitude * (currentCost*1.0/(a.position-b.position).costMagnitude));
				}
				if (a == endNode) {
					return (uint)((hTarget - b.position).costMagnitude * (currentCost*1.0/(a.position-b.position).costMagnitude));
				}
				if (b == endNode) {
					return (uint)((hTarget - a.position).costMagnitude * (currentCost*1.0/(a.position-b.position).costMagnitude));
				}
			} else {
				// endNode is null, startNode should never be null for an ABPath
				if (a == startNode) {
					return (uint)((startIntPoint - b.position).costMagnitude * (currentCost*1.0/(a.position-b.position).costMagnitude));
				}
				if (b == startNode) {
					return (uint)((startIntPoint - a.position).costMagnitude * (currentCost*1.0/(a.position-b.position).costMagnitude));
				}
			}

			return currentCost;
		}

		/** Reset all values to their default values.
		 * All inheriting path types must implement this function, resetting ALL their variables to enable recycling of paths.
		 * Call this base function in inheriting types with base.Reset ();
		 */
		protected override void Reset () {
			base.Reset();

			startNode = null;
			endNode = null;
			originalStartPoint = Vector3.zero;
			originalEndPoint = Vector3.zero;
			startPoint = Vector3.zero;
			endPoint = Vector3.zero;
			calculatePartial = false;
			partialBestTarget = null;
			startIntPoint = new Int3();
			hTarget = new Int3();
			endNodeCosts = null;
		}

		/** Prepares the path. Searches for start and end nodes and does some simple checking if a path is at all possible */
		protected override void Prepare () {

			//Initialize the NNConstraint
			nnConstraint.tags = enabledTags;
			var startNNInfo  = PathFindHelper.GetNearest(startPoint, nnConstraint);

			//Tell the NNConstraint which node was found as the start node if it is a PathNNConstraint and not a normal NNConstraint
			var pathNNConstraint = nnConstraint as PathNNConstraint;
			if (pathNNConstraint != null) {
				pathNNConstraint.SetStart(startNNInfo.node);
			}

			startPoint = startNNInfo.position;

			startIntPoint = (Int3)startPoint;
			startNode = startNNInfo.node;

			if (startNode == null) {
				FailWithError("Couldn't find a node close to the start point");
				return;
			}

			if (!CanTraverse(startNode)) {
				FailWithError("The node closest to the start point could not be traversed");
				return;
			}

			// If it is declared that this path type has an end point
			// Some path types might want to use most of the ABPath code, but will not have an explicit end point at this stage
			if (hasEndPoint) {
				var endNNInfo = PathFindHelper.GetNearest(endPoint, nnConstraint);
				endPoint = endNNInfo.position;
				endNode = endNNInfo.node;

				if (endNode == null) {
					FailWithError("Couldn't find a node close to the end point");
					return;
				}

				// This should not trigger unless the user has modified the NNConstraint
				if (!CanTraverse(endNode)) {
					FailWithError("The node closest to the end point could not be traversed");
					return;
				}

				// This should not trigger unless the user has modified the NNConstraint
				if (startNode.Area != endNode.Area) {
					FailWithError("There is no valid path to the target");
					return;
				}
				{
					// Note, other methods assume hTarget is (Int3)endPoint
					hTarget = (Int3)endPoint;
					hTargetNode = endNode;

					// Mark end node with flag1 to mark it as a target point
					pathHandler.GetPathNode(endNode).flag1 = true;
				}
			}
		}

		/** Checks if the start node is the target and complete the path if that is the case.
		 * This is necessary so that subclasses (e.g XPath) can override this behaviour.
		 *
		 * If the start node is a valid target point, this method should set CompleteState to Complete
		 * and trace the path.
		 */
		protected virtual void CompletePathIfStartIsValidTarget () {
			// flag1 specifies if a node is a target node for the path
			if (hasEndPoint && pathHandler.GetPathNode(startNode).flag1) {
				CompleteWith(startNode);
				Trace(pathHandler.GetPathNode(startNode));
			}
		}

		protected override void Initialize () {
			// Mark nodes to enable special connection costs for start and end nodes
			// See GetConnectionSpecialCost
			if (startNode != null) pathHandler.GetPathNode(startNode).flag2 = true;
			if (endNode != null) pathHandler.GetPathNode(endNode).flag2 = true;

			// Zero out the properties on the start node
			PathNode startRNode = pathHandler.GetPathNode(startNode);
			startRNode.node = startNode;
			startRNode.pathID = pathHandler.PathID;
			startRNode.parent = null;
			startRNode.cost = 0;
			startRNode.G = GetTraversalCost(startNode);
			startRNode.H = CalculateHScore(startNode);

			// Check if the start node is the target and complete the path if that is the case
			CompletePathIfStartIsValidTarget();
			if (CompleteState == PathCompleteState.Complete) return;

			// Open the start node and puts its neighbours in the open list
			startNode.Open(this, startRNode, pathHandler);

			searchedNodes++;

			partialBestTarget = startRNode;

			// Any nodes left to search?
			if (pathHandler.heap.isEmpty) {
				if (calculatePartial) {
					CompleteState = PathCompleteState.Partial;
					Trace(partialBestTarget);
				} else {
					FailWithError("No open points, the start node didn't open any nodes");
				}
				return;
			}

			// Pop the first node off the open list
			currentR = pathHandler.heap.Remove();
		}

		protected override void Cleanup () {
			// TODO: Set flag1 = false as well?
			if (startNode != null) {
				var pathStartNode = pathHandler.GetPathNode(startNode);
				pathStartNode.flag1 = false;
				pathStartNode.flag2 = false;
			}

			if (endNode != null) {
				var pathEndNode = pathHandler.GetPathNode(endNode);
				pathEndNode.flag1 = false;
				pathEndNode.flag2 = false;
			}
		}

		/** Completes the path using the specified target node.
		 * This method assumes that the node is a target node of the path
		 * not just any random node.
		 */
		void CompleteWith (GraphNode node) {
			// Mark the path as completed
			CompleteState = PathCompleteState.Complete;
		}

		/** Calculates the path until completed or until the time has passed \a targetTick.
		 * Usually a check is only done every 500 nodes if the time has passed \a targetTick.
		 * Time/Ticks are got from System.DateTime.UtcNow.Ticks.
		 *
		 * Basic outline of what the function does for the standard path (Pathfinding.ABPath).
		 * \code
		 * while the end has not been found and no error has occurred
		 * check if we have reached the end
		 * if so, exit and return the path
		 *
		 * open the current node, i.e loop through its neighbours, mark them as visited and put them on a heap
		 *
		 * check if there are still nodes left to process (or have we searched the whole graph)
		 * if there are none, flag error and exit
		 *
		 * pop the next node of the heap and set it as current
		 *
		 * check if the function has exceeded the time limit
		 * if so, return and wait for the function to get called again
		 * \endcode
		 */
		protected override void CalculateStep (long targetTick) {
			int counter = 0;

			// Continue to search as long as we haven't encountered an error and we haven't found the target
			while (CompleteState == PathCompleteState.NotCalculated) {
				searchedNodes++;

				// Close the current node, if the current node is the target node then the path is finished
				if (currentR.flag1) {
					// We found a target point
					// Mark that node as the end point
					CompleteWith(currentR.node);
					break;
				}

				if (currentR.H < partialBestTarget.H) {
					partialBestTarget = currentR;
				}

				// Loop through all walkable neighbours of the node and add them to the open list.
				currentR.node.Open(this, currentR, pathHandler);

				// Any nodes left to search?
				if (pathHandler.heap.isEmpty) {
					if (calculatePartial && partialBestTarget != null) {
						CompleteState = PathCompleteState.Partial;
						Trace(partialBestTarget);
					} else {
						FailWithError("Searched whole area but could not find target");
					}
					return;
				}

				// Select the node with the lowest F score and remove it from the open list
				currentR = pathHandler.heap.Remove();

				// Check for time every 500 nodes, roughly every 0.5 ms usually
				if (counter > 500) {
					// Have we exceded the maxFrameTime, if so we should wait one frame before continuing the search since we don't want the game to lag
					if (System.DateTime.UtcNow.Ticks >= targetTick) {
						// Return instead of yield'ing, a separate function handles the yield (CalculatePaths)
						return;
					}
					counter = 0;

					// Mostly for development
					if (searchedNodes > 1000000) {
						throw new System.Exception("Probable infinite loop. Over 1,000,000 nodes searched");
					}
				}

				counter++;
			}

			if (CompleteState == PathCompleteState.Complete) {
				Trace(currentR);
			} else if (calculatePartial && partialBestTarget != null) {
				CompleteState = PathCompleteState.Partial;
				Trace(partialBestTarget);
			}
		}

		/** Returns a debug string for this path.
		 */
		internal override string DebugString (PathLog logMode) {
			if (logMode == PathLog.None || (!error && logMode == PathLog.OnlyErrors)) {
				return "";
			}

			var text = new System.Text.StringBuilder();

			DebugStringPrefix(logMode, text);

			if (!error && logMode == PathLog.Heavy) {
				if (hasEndPoint && endNode != null) {
					PathNode nodeR = pathHandler.GetPathNode(endNode);
					text.Append("\nEnd Node\n	G: ");
					text.Append(nodeR.G);
					text.Append("\n	H: ");
					text.Append(nodeR.H);
					text.Append("\n	F: ");
					text.Append(nodeR.F);
					text.Append("\n	Point: ");
					text.Append(((Vector3)endPoint).ToString());
					text.Append("\n	Graph: ");
					text.Append(endNode.GraphIndex);
				}

				text.Append("\nStart Node");
				text.Append("\n	Point: ");
				text.Append(((Vector3)startPoint).ToString());
				text.Append("\n	Graph: ");
				if (startNode != null) text.Append(startNode.GraphIndex);
				else text.Append("< null startNode >");
			}

			DebugStringSuffix(logMode, text);

			return text.ToString();
		}

		/** \cond INTERNAL */
		/** Returns in which direction to move from a point on the path.
		 * A simple and quite slow (well, compared to more optimized algorithms) algorithm first finds the closest path segment (from #vectorPath) and then returns
		 * the direction to the next point from there. The direction is not normalized.
		 * \returns Direction to move from a \a point, returns Vector3.zero if #vectorPath is null or has a length of 0
		 * \deprecated
		 */
		[System.Obsolete()]
		public Vector3 GetMovementVector (Vector3 point) {
			if (vectorPath == null || vectorPath.Count == 0) {
				return Vector3.zero;
			}

			if (vectorPath.Count == 1) {
				return vectorPath[0]-point;
			}

			float minDist = float.PositiveInfinity;//Mathf.Infinity;
			int minSegment = 0;

			for (int i = 0; i < vectorPath.Count-1; i++) {
				Vector3 closest = VectorMath.ClosestPointOnSegment(vectorPath[i], vectorPath[i+1], point);
				float dist = (closest-point).sqrMagnitude;
				if (dist < minDist) {
					minDist = dist;
					minSegment = i;
				}
			}

			return vectorPath[minSegment+1]-point;
		}

		/** \endcond */
	}
}
