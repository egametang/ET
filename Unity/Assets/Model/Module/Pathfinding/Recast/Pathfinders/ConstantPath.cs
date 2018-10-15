//#define ASTARDEBUG //Draws a ray for each node visited

using PF;
using System;
using System.Collections.Generic;

namespace PF {
	/** Finds all nodes within a specified distance from the start.
	 * This class will search outwards from the start point and find all nodes which it costs less than ConstantPath.maxGScore to reach, this is usually the same as the distance to them multiplied with 1000
	 *
	 * The path can be called like:
	 * \code
	 * // Here you create a new path and set how far it should search. Null is for the callback, but the seeker will handle that
	 * ConstantPath cpath = ConstantPath.Construct(transform.position, 2000, null);
	 * // Set the seeker to search for the path (where mySeeker is a variable referencing a Seeker component)
	 * mySeeker.StartPath(cpath, myCallbackFunction);
	 * \endcode
	 *
	 * Then when getting the callback, all nodes will be stored in the variable ConstantPath.allNodes (remember that you need to cast it from Path to ConstantPath first to get the variable).
	 *
	 * This list will be sorted by the cost to reach that node (more specifically the G score if you are familiar with the terminology for search algorithms).
	 * \shadowimage{constantPath.png}
	 *
	 * \ingroup paths
	 * \astarpro
	 *
	 **/
	public class ConstantPath : Path {
		public GraphNode startNode;
		public Vector3 startPoint;
		public Vector3 originalStartPoint;

		/** Contains all nodes the path found.
		 * This list will be sorted by G score (cost/distance to reach the node).
		 */
		public List<GraphNode> allNodes;

		/** Controls when the path should terminate.
		 * This is set up automatically in the constructor to an instance of the Pathfinding.EndingConditionDistance class with a \a maxGScore is specified in the constructor.
		 * If you want to use another ending condition.
		 * \see Pathfinding.PathEndingCondition for examples
		 */
		public PathEndingCondition endingCondition;

		internal override bool FloodingPath {
			get {
				return true;
			}
		}

		/** Constructs a ConstantPath starting from the specified point.
		 * \param start             From where the path will be started from (the closest node to that point will be used)
		 * \param maxGScore			Searching will be stopped when a node has a G score greater than this
		 * \param callback			Will be called when the path has completed, leave this to null if you use a Seeker to handle calls
		 *
		 * Searching will be stopped when a node has a G score (cost to reach it) greater or equal to \a maxGScore
		 * in order words it will search all nodes with a cost to get there less than \a maxGScore.
		 */
		public static ConstantPath Construct (Vector3 start, int maxGScore, OnPathDelegate callback = null) {
			var p = PathPool.GetPath<ConstantPath>();

			p.Setup(start, maxGScore, callback);
			return p;
		}

		/** Sets up a ConstantPath starting from the specified point */
		protected void Setup (Vector3 start, int maxGScore, OnPathDelegate callback) {
			this.callback = callback;
			startPoint = start;
			originalStartPoint = startPoint;

			endingCondition = new EndingConditionDistance(this, maxGScore);
		}

		protected override void OnEnterPool () {
			base.OnEnterPool();
			if (allNodes != null) ListPool<GraphNode>.Release(ref allNodes);
		}

		/** Reset the path to default values.
		 * Clears the #allNodes list.
		 * \note This does not reset the #endingCondition.
		 *
		 * Also sets #heuristic to Heuristic.None as it is the default value for this path type
		 */
		protected override void Reset () {
			base.Reset();
			allNodes = ListPool<GraphNode>.Claim();
			endingCondition = null;
			originalStartPoint = Vector3.zero;
			startPoint = Vector3.zero;
			startNode = null;
			heuristic = Heuristic.None;
		}

		protected override void Prepare () {
			nnConstraint.tags = enabledTags;
			var startNNInfo  = PathFindHelper.GetNearest(startPoint, nnConstraint);

			startNode = startNNInfo.node;
			if (startNode == null) {
				FailWithError("Could not find close node to the start point");
				return;
			}
		}

		/** Initializes the path.
		 * Sets up the open list and adds the first node to it */
		protected override void Initialize () {
			PathNode startRNode = pathHandler.GetPathNode(startNode);

			startRNode.node = startNode;
			startRNode.pathID = pathHandler.PathID;
			startRNode.parent = null;
			startRNode.cost = 0;
			startRNode.G = GetTraversalCost(startNode);
			startRNode.H = CalculateHScore(startNode);

			startNode.Open(this, startRNode, pathHandler);

			searchedNodes++;

			startRNode.flag1 = true;
			allNodes.Add(startNode);

			//any nodes left to search?
			if (pathHandler.heap.isEmpty) {
				CompleteState = PathCompleteState.Complete;
				return;
			}

			currentR = pathHandler.heap.Remove();
		}

		protected override void Cleanup () {
			int c = allNodes.Count;

			for (int i = 0; i < c; i++) pathHandler.GetPathNode(allNodes[i]).flag1 = false;
		}

		protected override void CalculateStep (long targetTick) {
			int counter = 0;

			//Continue to search as long as we haven't encountered an error and we haven't found the target
			while (CompleteState == PathCompleteState.NotCalculated) {
				searchedNodes++;

//--- Here's the important stuff
				//Close the current node, if the current node satisfies the ending condition, the path is finished
				if (endingCondition.TargetFound(currentR)) {
					CompleteState = PathCompleteState.Complete;
					break;
				}

				if (!currentR.flag1) {
					//Add Node to allNodes
					allNodes.Add(currentR.node);
					currentR.flag1 = true;
				}

#if ASTARDEBUG
				Debug.DrawRay((Vector3)currentR.node.position, Vector3.up*5, Color.cyan);
#endif

//--- Here the important stuff ends

				//Debug.DrawRay ((Vector3)currentR.node.Position, Vector3.up*2,Color.red);

				//Loop through all walkable neighbours of the node and add them to the open list.
				currentR.node.Open(this, currentR, pathHandler);

				//any nodes left to search?
				if (pathHandler.heap.isEmpty) {
					CompleteState = PathCompleteState.Complete;
					break;
				}


				//Select the node with the lowest F score and remove it from the open list
				currentR = pathHandler.heap.Remove();

				//Check for time every 500 nodes, roughly every 0.5 ms usually
				if (counter > 500) {
					//Have we exceded the maxFrameTime, if so we should wait one frame before continuing the search since we don't want the game to lag
					if (DateTime.UtcNow.Ticks >= targetTick) {
						//Return instead of yield'ing, a separate function handles the yield (CalculatePaths)
						return;
					}
					counter = 0;

					if (searchedNodes > 1000000) {
						throw new Exception("Probable infinite loop. Over 1,000,000 nodes searched");
					}
				}

				counter++;
			}
		}
	}

	/** Target is found when the path is longer than a specified value.
	 * Actually this is defined as when the current node's G score is >= a specified amount (EndingConditionDistance.maxGScore).\n
	 * The G score is the cost from the start node to the current node, so an area with a higher penalty (weight) will add more to the G score.
	 * However the G score is usually just the shortest distance from the start to the current node.
	 *
	 * \see Pathfinding.ConstantPath which uses this ending condition
	 */
	public class EndingConditionDistance : PathEndingCondition {
		/** Max G score a node may have */
		public int maxGScore = 100;

		//public EndingConditionDistance () {}
		public EndingConditionDistance (Path p, int maxGScore) : base(p) {
			this.maxGScore = maxGScore;
		}

		public override bool TargetFound (PathNode node) {
			return node.G >= maxGScore;
		}
	}
}
