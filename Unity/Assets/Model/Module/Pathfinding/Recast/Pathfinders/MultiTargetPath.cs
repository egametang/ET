using PF;
using System.Collections.Generic;

namespace PF {
	/** A path which searches from one point to a number of different targets in one search or from a number of different start points to a single target.
	 *
	 * This is faster than searching with an ABPath for each target if pathsForAll is true.
	 * This path type can be used for example when you want an agent to find the closest target of a few different options.
	 *
	 * When pathsForAll is true, it will calculate a path to each target point, but it can share a lot of calculations for the different paths so
	 * it is faster than requesting them separately.
	 *
	 * When pathsForAll is false, it will perform a search using the heuristic set to None and stop as soon as it finds the first target.
	 * This may be faster or slower than requesting each path separately.
	 * It will run a Dijkstra search where it searches all nodes around the start point until the closest target is found.
	 * Note that this is usually faster if some target points are very close to the start point and some are very far away, but
	 * it can be slower if all target points are relatively far away because then it will have to search a much larger
	 * region since it will not use any heuristics.
	 *
	 * \ingroup paths
	 * \astarpro
	 * \see Seeker.StartMultiTargetPath
	 * \see \ref MultiTargetPathExample.cs "Example of how to use multi-target-paths"
	 *
	 * \version Since 3.7.1 the vectorPath and path fields are always set to the shortest path even when pathsForAll is true.
	 */
	public class MultiTargetPath : ABPath {
		/** Callbacks to call for each individual path */
		public OnPathDelegate[] callbacks;

		/** Nearest nodes to the #targetPoints */
		public GraphNode[] targetNodes;

		/** Number of target nodes left to find */
		protected int targetNodeCount;

		/** Indicates if the target has been found. Also true if the target cannot be reached (is in another area) */
		public bool[] targetsFound;

		/** Target points specified when creating the path. These are snapped to the nearest nodes */
		public Vector3[] targetPoints;

		/** Target points specified when creating the path. These are not snapped to the nearest nodes */
		public Vector3[] originalTargetPoints;

		/** Stores all vector paths to the targets. Elements are null if no path was found */
		public List<Vector3>[] vectorPaths;

		/** Stores all paths to the targets. Elements are null if no path was found */
		public List<GraphNode>[] nodePaths;

		/** If true, a path to all targets will be returned, otherwise just the one to the closest one. */
		public bool pathsForAll = true;

		/** The closest target index (if any target was found) */
		public int chosenTarget = -1;

		/** Current target for Sequential #heuristicMode.
		 * Refers to an item in the targetPoints array
		 */
		int sequentialTarget;

		/** How to calculate the heuristic.
		 * The \link #hTarget heuristic target point \endlink can be calculated in different ways,
		 * by taking the Average position of all targets, or taking the mid point of them (i.e center of the AABB encapsulating all targets).
		 *
		 * The one which works best seems to be Sequential, it sets #hTarget to the target furthest away, and when that target is found, it moves on to the next one.\n
		 * Some modes have the option to be 'moving' (e.g 'MovingAverage'), that means that it is updated every time a target is found.\n
		 * The H score is calculated according to AstarPath.heuristic
		 *
		 * \note If pathsForAll is false then this option is ignored and it is always treated as being set to None
		 */
		public HeuristicMode heuristicMode = HeuristicMode.Sequential;

		public enum HeuristicMode {
			None,
			Average,
			MovingAverage,
			Midpoint,
			MovingMidpoint,
			Sequential
		}

		/** False if the path goes from one point to multiple targets. True if it goes from multiple start points to one target point */
		public bool inverted { get; protected set; }

		/** Default constructor.
		 * Do not use this. Instead use the static Construct method which can handle path pooling.
		 */
		public MultiTargetPath () {}

		public static MultiTargetPath Construct (Vector3[] startPoints, Vector3 target, OnPathDelegate[] callbackDelegates, OnPathDelegate callback = null) {
			MultiTargetPath p = Construct(target, startPoints, callbackDelegates, callback);

			p.inverted = true;
			return p;
		}

		public static MultiTargetPath Construct (Vector3 start, Vector3[] targets, OnPathDelegate[] callbackDelegates, OnPathDelegate callback = null) {
			var p = PathPool.GetPath<MultiTargetPath>();

			p.Setup(start, targets, callbackDelegates, callback);
			return p;
		}

		protected void Setup (Vector3 start, Vector3[] targets, OnPathDelegate[] callbackDelegates, OnPathDelegate callback) {
			inverted = false;
			this.callback = callback;
			callbacks = callbackDelegates;
			if (callbacks != null && callbacks.Length != targets.Length) throw new System.ArgumentException("The targets array must have the same length as the callbackDelegates array");
			targetPoints = targets;

			originalStartPoint = start;

			startPoint = start;
			startIntPoint = (Int3)start;

			if (targets.Length == 0) {
				FailWithError("No targets were assigned to the MultiTargetPath");
				return;
			}

			endPoint = targets[0];

			originalTargetPoints = new Vector3[targetPoints.Length];
			for (int i = 0; i < targetPoints.Length; i++) {
				originalTargetPoints[i] = targetPoints[i];
			}
		}

		protected override void Reset () {
			base.Reset();
			pathsForAll = true;
			chosenTarget = -1;
			sequentialTarget = 0;
			inverted = true;
			heuristicMode = HeuristicMode.Sequential;
		}

		protected override void OnEnterPool () {
			if (vectorPaths != null)
				for (int i = 0; i < vectorPaths.Length; i++)
					if (vectorPaths[i] != null) ListPool<Vector3>.Release(vectorPaths[i]);

			vectorPaths = null;
			vectorPath = null;

			if (nodePaths != null)
				for (int i = 0; i < nodePaths.Length; i++)
					if (nodePaths[i] != null) ListPool<GraphNode>.Release(nodePaths[i]);

			nodePaths = null;
			path = null;
			callbacks = null;
			targetNodes = null;
			targetsFound = null;
			targetPoints = null;
			originalTargetPoints = null;

			base.OnEnterPool();
		}

		/** Set chosenTarget to the index of the shortest path */
		void ChooseShortestPath () {
			//
			// When pathsForAll is false there will only be one non-null path
			chosenTarget = -1;
			if (nodePaths != null) {
				uint bestG = int.MaxValue;
				for (int i = 0; i < nodePaths.Length; i++) {
					var currentPath = nodePaths[i];
					if (currentPath != null) {
						// Get the G score of the first or the last node in the path
						// depending on if the paths are reversed or not
						var g = pathHandler.GetPathNode(currentPath[inverted ? 0 : currentPath.Count-1]).G;
						if (chosenTarget == -1 || g < bestG) {
							chosenTarget = i;
							bestG = g;
						}
					}
				}
			}
		}

		void SetPathParametersForReturn (int target) {
			path = nodePaths[target];
			vectorPath = vectorPaths[target];

			if (inverted) {
				startNode = targetNodes[target];
				startPoint = targetPoints[target];
				originalStartPoint = originalTargetPoints[target];
			} else {
				endNode = targetNodes[target];
				endPoint = targetPoints[target];
				originalEndPoint = originalTargetPoints[target];
			}
		}

		protected override void ReturnPath () {
			if (error) {
				// Call all callbacks
				if (callbacks != null) {
					for (int i = 0; i < callbacks.Length; i++)
						if (callbacks[i] != null) callbacks[i] (this);
				}

				if (callback != null) callback(this);

				return;
			}

			bool anySucceded = false;

			// Set the end point to the start point
			// since the path is reversed
			// (the start point will be set individually for each path)
			if (inverted) {
				endPoint = startPoint;
				endNode = startNode;
				originalEndPoint = originalStartPoint;
			}

			for (int i = 0; i < nodePaths.Length; i++) {
				if (nodePaths[i] != null) {
					// Note that we use the lowercase 'completeState' here.
					// The property (CompleteState) will ensure that the complete state is never
					// changed away from the error state but in this case we don't want that behaviour.
					completeState = PathCompleteState.Complete;
					anySucceded = true;
				} else {
					completeState = PathCompleteState.Error;
				}

				if (callbacks != null && callbacks[i] != null) {
					SetPathParametersForReturn(i);
					callbacks[i] (this);

					// In case a modifier changed the vectorPath, update the array of all vectorPaths
					vectorPaths[i] = vectorPath;
				}
			}

			if (anySucceded) {
				completeState = PathCompleteState.Complete;
				SetPathParametersForReturn(chosenTarget);
			} else {
				completeState = PathCompleteState.Error;
			}

			if (callback != null) {
				callback(this);
			}
		}

		protected void FoundTarget (PathNode nodeR, int i) {
			nodeR.flag1 = false; // Reset bit 8

			Trace(nodeR);
			vectorPaths[i] = vectorPath;
			nodePaths[i] = path;
			vectorPath = ListPool<Vector3>.Claim();
			path = ListPool<GraphNode>.Claim();

			targetsFound[i] = true;

			targetNodeCount--;

			// Since we have found one target
			// and the heuristic is always set to None when
			// pathsForAll is false, we will have found the shortest path
			if (!pathsForAll) {
				CompleteState = PathCompleteState.Complete;
				targetNodeCount = 0;
				return;
			}


			// If there are no more targets to find, return here and avoid calculating a new hTarget
			if (targetNodeCount <= 0) {
				CompleteState = PathCompleteState.Complete;
				return;
			}

			RecalculateHTarget(false);
		}

		protected void RebuildOpenList () {
			BinaryHeap heap = pathHandler.heap;

			for (int j = 0; j < heap.numberOfItems; j++) {
				PathNode nodeR = heap.GetNode(j);
				nodeR.H = CalculateHScore(nodeR.node);
				heap.SetF(j, nodeR.F);
			}

			pathHandler.heap.Rebuild();
		}

		protected override void Prepare () {
			nnConstraint.tags = enabledTags;
			var startNNInfo  = PathFindHelper.GetNearest(startPoint, nnConstraint);
			startNode = startNNInfo.node;

			if (startNode == null) {
				FailWithError("Could not find start node for multi target path");
				return;
			}

			if (!CanTraverse(startNode)) {
				FailWithError("The node closest to the start point could not be traversed");
				return;
			}

			// Tell the NNConstraint which node was found as the start node if it is a PathNNConstraint and not a normal NNConstraint
			var pathNNConstraint = nnConstraint as PathNNConstraint;
			if (pathNNConstraint != null) {
				pathNNConstraint.SetStart(startNNInfo.node);
			}

			vectorPaths = new List<Vector3>[targetPoints.Length];
			nodePaths = new List<GraphNode>[targetPoints.Length];
			targetNodes = new GraphNode[targetPoints.Length];
			targetsFound = new bool[targetPoints.Length];
			targetNodeCount = targetPoints.Length;

			bool anyWalkable = false;
			bool anySameArea = false;
			bool anyNotNull = false;

			for (int i = 0; i < targetPoints.Length; i++) {
				var endNNInfo = PathFindHelper.GetNearest(targetPoints[i], nnConstraint);

				targetNodes[i] = endNNInfo.node;

				targetPoints[i] = endNNInfo.position;
				if (targetNodes[i] != null) {
					anyNotNull = true;
					endNode = targetNodes[i];
				}

				bool notReachable = false;

				if (endNNInfo.node != null && CanTraverse(endNNInfo.node)) {
					anyWalkable = true;
				} else {
					notReachable = true;
				}

				if (endNNInfo.node != null && endNNInfo.node.Area == startNode.Area) {
					anySameArea = true;
				} else {
					notReachable = true;
				}

				if (notReachable) {
					// Signal that the pathfinder should not look for this node because we have already found it
					targetsFound[i] = true;
					targetNodeCount--;
				}
			}

			startPoint = startNNInfo.position;

			startIntPoint = (Int3)startPoint;

			if (!anyNotNull) {
				FailWithError("Couldn't find nodes close to the all of the end points");
				return;
			}

			if (!anyWalkable) {
				FailWithError("No target nodes could be traversed");
				return;
			}

			if (!anySameArea) {
				FailWithError("There are no valid paths to the targets");
				return;
			}

			RecalculateHTarget(true);
		}

		void RecalculateHTarget (bool firstTime) {
			// When pathsForAll is false
			// then no heuristic should be used
			if (!pathsForAll) {
				heuristic = Heuristic.None;
				heuristicScale = 0.0F;
				return;
			}

			// Calculate a new hTarget and rebuild the open list if necessary
			// Rebuilding the open list is necessary when the H score for nodes changes
			switch (heuristicMode) {
			case HeuristicMode.None:
				heuristic = Heuristic.None;
				heuristicScale = 0F;
				break;
			case HeuristicMode.Average:
				if (!firstTime) return;

				// No break
				// The first time the implementation
				// for Average and MovingAverage is identical
				// so we just use fallthrough
				goto case HeuristicMode.MovingAverage;
			case HeuristicMode.MovingAverage:

				// Pick the average position of all nodes that have not been found yet
				var avg = Vector3.zero;
				int count = 0;
				for (int j = 0; j < targetPoints.Length; j++) {
					if (!targetsFound[j]) {
						avg += (Vector3)targetNodes[j].position;
						count++;
					}
				}

				// Should use asserts, but they were first added in Unity 5.1
				// so I cannot use them because I want to keep compatibility with 4.6
				// (as of 2015)
				if (count == 0) throw new System.Exception("Should not happen");

				avg /= count;
				hTarget = (Int3)avg;
				break;
			case HeuristicMode.Midpoint:
				if (!firstTime) return;

				// No break
				// The first time the implementation
				// for Midpoint and MovingMidpoint is identical
				// so we just use fallthrough
				goto case HeuristicMode.MovingMidpoint;
			case HeuristicMode.MovingMidpoint:

				Vector3 min = Vector3.zero;
				Vector3 max = Vector3.zero;
				bool set = false;

				// Pick the median of all points that have
				// not been found yet
				for (int j = 0; j < targetPoints.Length; j++) {
					if (!targetsFound[j]) {
						if (!set) {
							min = (Vector3)targetNodes[j].position;
							max = (Vector3)targetNodes[j].position;
							set = true;
						} else {
							min = Vector3.Min((Vector3)targetNodes[j].position, min);
							max = Vector3.Max((Vector3)targetNodes[j].position, max);
						}
					}
				}

				var midpoint = (Int3)((min+max)*0.5F);
				hTarget = midpoint;
				break;
			case HeuristicMode.Sequential:

				// The first time the hTarget should always be recalculated
				// But other times we can skip it if we have not yet found the current target
				// since then the hTarget would just be set to the same value again
				if (!firstTime && !targetsFound[sequentialTarget]) {
					return;
				}

				float dist = 0;

				// Pick the target which is furthest away and has not been found yet
				for (int j = 0; j < targetPoints.Length; j++) {
					if (!targetsFound[j]) {
						float d = (targetNodes[j].position-startNode.position).sqrMagnitude;
						if (d > dist) {
							dist = d;
							hTarget = (Int3)targetPoints[j];
							sequentialTarget = j;
						}
					}
				}
				break;
			}

			// Rebuild the open list since all the H scores have changed
			// However the first time we can skip this since
			// no nodes are added to the heap yet
			if (!firstTime) {
				RebuildOpenList();
			}
		}

		protected override void Initialize () {
			// Reset the start node to prevent
			// old info from previous paths to be used
			PathNode startRNode = pathHandler.GetPathNode(startNode);

			startRNode.node = startNode;
			startRNode.pathID = pathID;
			startRNode.parent = null;
			startRNode.cost = 0;
			startRNode.G = GetTraversalCost(startNode);
			startRNode.H = CalculateHScore(startNode);

			for (int j = 0; j < targetNodes.Length; j++) {
				if (startNode == targetNodes[j]) {
					// The start node is equal to the target node
					// so we can immediately mark the path as calculated
					FoundTarget(startRNode, j);
				} else if (targetNodes[j] != null) {
					// Mark the node with a flag so that we can quickly check if we have found a target node
					pathHandler.GetPathNode(targetNodes[j]).flag1 = true;
				}
			}

			// If all paths have either been invalidated or found already because they were at the same node as the start node
			if (targetNodeCount <= 0) {
				CompleteState = PathCompleteState.Complete;
				return;
			}

			//if (recalcStartEndCosts) {
			//	startNode.InitialOpen (open,hTarget,startIntPoint,this,true);
			//} else {
			startNode.Open(this, startRNode, pathHandler);
			//}
			searchedNodes++;

			//any nodes left to search?
			if (pathHandler.heap.isEmpty) {
				FailWithError("No open points, the start node didn't open any nodes");
				return;
			}

			// Take the first node off the heap
			currentR = pathHandler.heap.Remove();
		}

		protected override void Cleanup () {
			// Make sure that the shortest path is set
			// after the path has been calculated
			ChooseShortestPath();
			ResetFlags();
		}

		/** Reset flag1 on all nodes after the pathfinding has completed (no matter if an error occurs or if the path is canceled) */
		void ResetFlags () {
			// Reset all flags
			if (targetNodes != null) {
				for (int i = 0; i < targetNodes.Length; i++) {
					if (targetNodes[i] != null) pathHandler.GetPathNode(targetNodes[i]).flag1 = false;
				}
			}
		}

		protected override void CalculateStep (long targetTick) {
			int counter = 0;

			// Continue to search as long as we haven't encountered an error and we haven't found the target
			while (CompleteState == PathCompleteState.NotCalculated) {
				// @Performance Just for debug info
				searchedNodes++;

				// The node might be the target node for one of the paths
				if (currentR.flag1) {
					// Close the current node, if the current node is the target node then the path is finnished
					for (int i = 0; i < targetNodes.Length; i++) {
						if (!targetsFound[i] && currentR.node == targetNodes[i]) {
							FoundTarget(currentR, i);
							if (CompleteState != PathCompleteState.NotCalculated) {
								break;
							}
						}
					}

					if (targetNodeCount <= 0) {
						CompleteState = PathCompleteState.Complete;
						break;
					}
				}

				// Loop through all walkable neighbours of the node and add them to the open list.
				currentR.node.Open(this, currentR, pathHandler);

				// Any nodes left to search?
				if (pathHandler.heap.isEmpty) {
					CompleteState = PathCompleteState.Complete;
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
				}

				counter++;
			}
		}

		protected override void Trace (PathNode node) {
			base.Trace(node);

			if (inverted) {
				// Reverse the paths
				int half = path.Count/2;

				for (int i = 0; i < half; i++) {
					GraphNode tmp = path[i];
					path[i] = path[path.Count-i-1];
					path[path.Count-i-1] = tmp;
				}

				for (int i = 0; i < half; i++) {
					Vector3 tmp = vectorPath[i];
					vectorPath[i] = vectorPath[vectorPath.Count-i-1];
					vectorPath[vectorPath.Count-i-1] = tmp;
				}
			}
		}

		internal override string DebugString (PathLog logMode) {
			if (logMode == PathLog.None || (!error && logMode == PathLog.OnlyErrors)) {
				return "";
			}

			System.Text.StringBuilder text = pathHandler.DebugStringBuilder;
			text.Length = 0;

			DebugStringPrefix(logMode, text);

			if (!error) {
				text.Append("\nShortest path was ");
				text.Append(chosenTarget == -1 ? "undefined" : nodePaths[chosenTarget].Count.ToString());
				text.Append(" nodes long");

				if (logMode == PathLog.Heavy) {
					text.Append("\nPaths (").Append(targetsFound.Length).Append("):");
					for (int i = 0; i < targetsFound.Length; i++) {
						text.Append("\n\n	Path ").Append(i).Append(" Found: ").Append(targetsFound[i]);

						if (nodePaths[i] != null) {
							text.Append("\n		Length: ");
							text.Append(nodePaths[i].Count);

							GraphNode node = nodePaths[i][nodePaths[i].Count-1];

							if (node != null) {
								PathNode nodeR = pathHandler.GetPathNode(endNode);
								if (nodeR != null) {
									text.Append("\n		End Node");
									text.Append("\n			G: ");
									text.Append(nodeR.G);
									text.Append("\n			H: ");
									text.Append(nodeR.H);
									text.Append("\n			F: ");
									text.Append(nodeR.F);
									text.Append("\n			Point: ");
									text.Append(((Vector3)endPoint).ToString());
									text.Append("\n			Graph: ");
									text.Append(endNode.GraphIndex);
								} else {
									text.Append("\n		End Node: Null");
								}
							}
						}
					}

					text.Append("\nStart Node");
					text.Append("\n	Point: ");
					text.Append(((Vector3)endPoint).ToString());
					text.Append("\n	Graph: ");
					text.Append(startNode.GraphIndex);
					text.Append("\nBinary Heap size at completion: ");
					text.AppendLine(pathHandler.heap == null ? "Null" : (pathHandler.heap.numberOfItems-2).ToString());  // -2 because numberOfItems includes the next item to be added and item zero is not used
				}
			}

			DebugStringSuffix(logMode, text);

			return text.ToString();
		}
	}
}
