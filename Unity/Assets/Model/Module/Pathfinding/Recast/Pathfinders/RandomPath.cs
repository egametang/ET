using PF;

namespace PF {
	/** Finds a path in a random direction from the start node.
	 * \ingroup paths
	 * Terminates and returns when G \>= \a length (passed to the constructor) + RandomPath.spread or when there are no more nodes left to search.\n
	 *
	 * \code
	 *
	 * // Call a RandomPath call like this, assumes that a Seeker is attached to the GameObject
	 *
	 * // The path will be returned when the path is over a specified length (or more accurately when the traversal cost is greater than a specified value).
	 * // A score of 1000 is approximately equal to the cost of moving one world unit.
	 * int theGScoreToStopAt = 50000;
	 *
	 * // Create a path object
	 * RandomPath path = RandomPath.Construct(transform.position, theGScoreToStopAt);
	 * // Determines the variation in path length that is allowed
	 * path.spread = 5000;
	 *
	 * // Get the Seeker component which must be attached to this GameObject
	 * Seeker seeker = GetComponent<Seeker>();
	 *
	 * // Start the path and return the result to MyCompleteFunction (which is a function you have to define, the name can of course be changed)
	 * seeker.StartPath(path, MyCompleteFunction);
	 *
	 * \endcode
	 * \astarpro
	 */
	public class RandomPath : ABPath {
		/** G score to stop searching at.
		 * The G score is rougly the distance to get from the start node to a node multiplied by 1000 (per default, see Pathfinding.Int3.Precision), plus any penalties */
		public int searchLength;

		/** All G scores between #searchLength and #searchLength+#spread are valid end points, a random one of them is chosen as the final point.
		 * On grid graphs a low spread usually works (but keep it higher than nodeSize*1000 since that it the default cost of moving between two nodes), on NavMesh graphs
		 * I would recommend a higher spread so it can evaluate more nodes
		 */
		public int spread = 5000;

		/** If an #aim is set, the higher this value is, the more it will try to reach #aim */
		public float aimStrength;

		/** Currently chosen end node */
		PathNode chosenNodeR;

		/** The node with the highest G score which is still lower than #searchLength.
		 * Used as a backup if a node with a G score higher than #searchLength could not be found */
		PathNode maxGScoreNodeR;

		/** The G score of #maxGScoreNodeR */
		int maxGScore;

		/** An aim can be used to guide the pathfinder to not take totally random paths.
		 * For example you might want your AI to continue in generally the same direction as before, then you can specify
		 * aim to be transform.postion + transform.forward*10 which will make it more often take paths nearer that point
		 * \see #aimStrength */
		public Vector3 aim;

		int nodesEvaluatedRep;

		/** Random number generator */
		readonly System.Random rnd = new System.Random();

		internal override bool FloodingPath {
			get {
				return true;
			}
		}

		protected override bool hasEndPoint {
			get {
				return false;
			}
		}

		protected override void Reset () {
			base.Reset();

			searchLength = 5000;
			spread = 5000;

			aimStrength = 0.0f;
			chosenNodeR = null;
			maxGScoreNodeR = null;
			maxGScore = 0;
			aim = Vector3.zero;

			nodesEvaluatedRep = 0;
		}

		public RandomPath () {}

		[System.Obsolete("This constructor is obsolete. Please use the pooling API and the Construct methods")]
		public RandomPath (Vector3 start, int length, OnPathDelegate callback = null) {
			throw new System.Exception("This constructor is obsolete. Please use the pooling API and the Setup methods");
		}

		public static RandomPath Construct (Vector3 start, int length, OnPathDelegate callback = null) {
			var p = PathPool.GetPath<RandomPath>();

			p.Setup(start, length, callback);
			return p;
		}

		protected RandomPath Setup (Vector3 start, int length, OnPathDelegate callback) {
			this.callback = callback;

			searchLength = length;

			originalStartPoint = start;
			originalEndPoint = Vector3.zero;

			startPoint = start;
			endPoint = Vector3.zero;

			startIntPoint = (Int3)start;

			return this;
		}

		/** Calls callback to return the calculated path.
		 * \see #callback */
		protected override void ReturnPath () {
			if (path != null && path.Count > 0) {
				endNode = path[path.Count-1];
				endPoint = (Vector3)endNode.position;
				originalEndPoint = endPoint;

				hTarget = endNode.position;
			}
			if (callback != null) {
				callback(this);
			}
		}

		protected override void Prepare () {
			nnConstraint.tags = enabledTags;
			var startNNInfo  = PathFindHelper.GetNearest(startPoint, nnConstraint);

			startPoint = startNNInfo.position;
			endPoint = startPoint;

			startIntPoint = (Int3)startPoint;
			hTarget = (Int3)aim;//startIntPoint;

			startNode = startNNInfo.node;
			endNode = startNode;

#if ASTARDEBUG
			Debug.DrawLine((Vector3)startNode.position, startPoint, Color.blue);
			Debug.DrawLine((Vector3)endNode.position, endPoint, Color.blue);
#endif

			if (startNode == null || endNode == null) {
				FailWithError("Couldn't find close nodes to the start point");
				return;
			}

			if (!CanTraverse(startNode)) {
				FailWithError("The node closest to the start point could not be traversed");
				return;
			}

			heuristicScale = aimStrength;
		}

		protected override void Initialize () {
			//Adjust the costs for the end node
			/*if (hasEndPoint && recalcStartEndCosts) {
			 *  endNodeCosts = endNode.InitialOpen (open,hTarget,(Int3)endPoint,this,false);
			 *  callback += ResetCosts; /* \todo Might interfere with other paths since other paths might be calculated before #callback is called *
			 * }*/

			//Node.activePath = this;
			PathNode startRNode = pathHandler.GetPathNode(startNode);

			startRNode.node = startNode;

			if (searchLength+spread <= 0) {
				CompleteState = PathCompleteState.Complete;
				Trace(startRNode);
				return;
			}

			startRNode.pathID = pathID;
			startRNode.parent = null;
			startRNode.cost = 0;
			startRNode.G = GetTraversalCost(startNode);
			startRNode.H = CalculateHScore(startNode);

			/*if (recalcStartEndCosts) {
			 *  startNode.InitialOpen (open,hTarget,startIntPoint,this,true);
			 * } else {*/
			startNode.Open(this, startRNode, pathHandler);
			//}

			searchedNodes++;

			//any nodes left to search?
			if (pathHandler.heap.isEmpty) {
				FailWithError("No open points, the start node didn't open any nodes");
				return;
			}

			currentR = pathHandler.heap.Remove();
		}

		protected override void CalculateStep (long targetTick) {
			int counter = 0;

			// Continue to search as long as we haven't encountered an error and we haven't found the target
			while (CompleteState == PathCompleteState.NotCalculated) {
				searchedNodes++;

				// Close the current node, if the current node is the target node then the path is finished
				if (currentR.G >= searchLength) {
					if (currentR.G <= searchLength+spread) {
						nodesEvaluatedRep++;

						if (rnd.NextDouble() <= 1.0f/nodesEvaluatedRep) {
							chosenNodeR = currentR;
						}
					} else {
						// If no node was in the valid range of G scores, then fall back to picking one right outside valid range
						if (chosenNodeR == null) chosenNodeR = currentR;

						CompleteState = PathCompleteState.Complete;
						break;
					}
				} else if (currentR.G > maxGScore) {
					maxGScore = (int)currentR.G;
					maxGScoreNodeR = currentR;
				}

				// Loop through all walkable neighbours of the node and add them to the open list.
				currentR.node.Open(this, currentR, pathHandler);

				// Any nodes left to search?
				if (pathHandler.heap.isEmpty) {
					if (chosenNodeR != null) {
						CompleteState = PathCompleteState.Complete;
					} else if (maxGScoreNodeR != null) {
						chosenNodeR = maxGScoreNodeR;
						CompleteState = PathCompleteState.Complete;
					} else {
						FailWithError("Not a single node found to search");
					}
					break;
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

					if (searchedNodes > 1000000) {
						throw new System.Exception("Probable infinite loop. Over 1,000,000 nodes searched");
					}
				}

				counter++;
			}

			if (CompleteState == PathCompleteState.Complete) {
				Trace(chosenNodeR);
			}
		}
	}
}
