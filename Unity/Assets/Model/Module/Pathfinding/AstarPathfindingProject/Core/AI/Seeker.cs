using UnityEngine;
using System.Collections.Generic;
using PF;
using Vector3 = UnityEngine.Vector3;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace Pathfinding {
	/** Handles path calls for a single unit.
	 * \ingroup relevant
	 * This is a component which is meant to be attached to a single unit (AI, Robot, Player, whatever) to handle its pathfinding calls.
	 * It also handles post-processing of paths using modifiers.
	 *
	 * \shadowimage{seeker_inspector.png}
	 *
	 * \see \ref calling-pathfinding
	 * \see \ref modifiers
	 */
	[AddComponentMenu("Pathfinding/Seeker")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_seeker.php")]
	public class Seeker : VersionedMonoBehaviour {
		/** Enables drawing of the last calculated path using Gizmos.
		 * The path will show up in green.
		 *
		 * \see OnDrawGizmos
		 */
		public bool drawGizmos = true;

		/** Enables drawing of the non-postprocessed path using Gizmos.
		 * The path will show up in orange.
		 *
		 * Requires that #drawGizmos is true.
		 *
		 * This will show the path before any post processing such as smoothing is applied.
		 *
		 * \see drawGizmos
		 * \see OnDrawGizmos
		 */
		public bool detailedGizmos;

		/** Path modifier which tweaks the start and end points of a path */
		[HideInInspector]
		public StartEndModifier startEndModifier = new StartEndModifier();

		/** The tags which the Seeker can traverse.
		 *
		 * \note This field is a bitmask.
		 * \see \ref bitmasks
		 */
		[HideInInspector]
		public int traversableTags = -1;

		/** Penalties for each tag.
		 * Tag 0 which is the default tag, will have added a penalty of tagPenalties[0].
		 * These should only be positive values since the A* algorithm cannot handle negative penalties.
		 *
		 * \note This array should always have a length of 32 otherwise the system will ignore it.
		 *
		 * \see Pathfinding.Path.tagPenalties
		 */
		[HideInInspector]
		public int[] tagPenalties = new int[32];

		/** Graphs that this Seeker can use.
		 * This field determines which graphs will be considered when searching for the start and end nodes of a path.
		 * It is useful in numerous situations, for example if you want to make one graph for small units and one graph for large units.
		 *
		 * This is a bitmask so if you for example want to make the agent only use graph index 3 then you can set this to:
		 * \code seeker.graphMask = 1 << 3; \endcode
		 *
		 * \see \ref bitmasks
		 *
		 * Note that this field only stores which graph indices that are allowed. This means that if the graphs change their ordering
		 * then this mask may no longer be correct.
		 *
		 * If you know the name of the graph you can set the mask like this:
		 * \snippet MiscSnippets.cs Masks.FromGraphName
		 *
		 * Some overloads of the #StartPath methods take a graphMask parameter. If those overloads are used then they
		 * will override the graph mask for that path request.
		 *
		 * \shadowimage{multiple_agents/seeker.png}
		 *
		 * \see \ref multiple-agent-types
		 */
		[HideInInspector]
		public int graphMask = -1;

		/** Callback for when a path is completed.
		 * Movement scripts should register to this delegate.\n
		 * A temporary callback can also be set when calling StartPath, but that delegate will only be called for that path
		 */
		public OnPathDelegate pathCallback;

		/** Called before pathfinding is started */
		public OnPathDelegate preProcessPath;

		/** Called after a path has been calculated, right before modifiers are executed.
		 */
		public OnPathDelegate postProcessPath;

		/** Used for drawing gizmos */
		[System.NonSerialized]
		List<PF.Vector3> lastCompletedVectorPath;

		/** Used for drawing gizmos */
		[System.NonSerialized]
		List<GraphNode> lastCompletedNodePath;

		/** The current path */
		[System.NonSerialized]
		protected Path path;

		/** Previous path. Used to draw gizmos */
		[System.NonSerialized]
		private Path prevPath;

		/** Cached delegate to avoid allocating one every time a path is started */
		private readonly OnPathDelegate onPathDelegate;
		/** Cached delegate to avoid allocating one every time a path is started */
		private readonly OnPathDelegate onPartialPathDelegate;

		/** Temporary callback only called for the current path. This value is set by the StartPath functions */
		private OnPathDelegate tmpPathCallback;

		/** The path ID of the last path queried */
		protected uint lastPathID;

		/** Internal list of all modifiers */
		readonly List<IPathModifier> modifiers = new List<IPathModifier>();

		public enum ModifierPass {
			PreProcess,
			// An obsolete item occupied index 1 previously
			PostProcess = 2,
		}

		public Seeker () {
			onPathDelegate = OnPathComplete;
			onPartialPathDelegate = OnPartialPathComplete;
		}

		/** Initializes a few variables */
		protected override void Awake () {
			base.Awake();
			startEndModifier.Awake(this);
		}

		/** Path that is currently being calculated or was last calculated.
		 * You should rarely have to use this. Instead get the path when the path callback is called.
		 *
		 * \see pathCallback
		 */
		public Path GetCurrentPath () {
			return path;
		}

		/** Stop calculating the current path request.
		 * If this Seeker is currently calculating a path it will be canceled.
		 * The callback (usually to a method named OnPathComplete) will soon be called
		 * with a path that has the 'error' field set to true.
		 *
		 * This does not stop the character from moving, it just aborts
		 * the path calculation.
		 *
		 * \param pool If true then the path will be pooled when the pathfinding system is done with it.
		 */
		public void CancelCurrentPathRequest (bool pool = true) {
			if (!IsDone()) {
				path.FailWithError("Canceled by script (Seeker.CancelCurrentPathRequest)");
				if (pool) {
					// Make sure the path has had its reference count incremented and decremented once.
					// If this is not done the system will think no pooling is used at all and will not pool the path.
					// The particular object that is used as the parameter (in this case 'path') doesn't matter at all
					// it just has to be *some* object.
					path.Claim(path);
					path.Release(path);
				}
			}
		}

		/** Cleans up some variables.
		 * Releases any eventually claimed paths.
		 * Calls OnDestroy on the #startEndModifier.
		 *
		 * \see ReleaseClaimedPath
		 * \see startEndModifier
		 */
		public void OnDestroy () {
			ReleaseClaimedPath();
			startEndModifier.OnDestroy(this);
		}

		/** Releases the path used for gizmos (if any).
		 * The seeker keeps the latest path claimed so it can draw gizmos.
		 * In some cases this might not be desireable and you want it released.
		 * In that case, you can call this method to release it (not that path gizmos will then not be drawn).
		 *
		 * If you didn't understand anything from the description above, you probably don't need to use this method.
		 *
		 * \see \ref pooling
		 */
		public void ReleaseClaimedPath () {
			if (prevPath != null) {
				prevPath.Release(this, true);
				prevPath = null;
			}
		}

		/** Called by modifiers to register themselves */
		public void RegisterModifier (IPathModifier modifier) {
			modifiers.Add(modifier);

			// Sort the modifiers based on their specified order
			modifiers.Sort((a, b) => a.Order.CompareTo(b.Order));
		}

		/** Called by modifiers when they are disabled or destroyed */
		public void DeregisterModifier (IPathModifier modifier) {
			modifiers.Remove(modifier);
		}

		/** Post Processes the path.
		 * This will run any modifiers attached to this GameObject on the path.
		 * This is identical to calling RunModifiers(ModifierPass.PostProcess, path)
		 * \see RunModifiers
		 * \since Added in 3.2
		 */
		public void PostProcess (Path path) {
			RunModifiers(ModifierPass.PostProcess, path);
		}

		/** Runs modifiers on a path */
		public void RunModifiers (ModifierPass pass, Path path) {
			if (pass == ModifierPass.PreProcess) {
				if (preProcessPath != null) preProcessPath(path);
				for (int i = 0; i < modifiers.Count; i++) modifiers[i].PreProcess(path);
			} else if (pass == ModifierPass.PostProcess) {
				Profiler.BeginSample("Running Path Modifiers");
				// Call delegates if they exist
				if (postProcessPath != null) postProcessPath(path);
				// Loop through all modifiers and apply post processing
				for (int i = 0; i < modifiers.Count; i++) modifiers[i].Apply(path);
				Profiler.EndSample();
			}
		}

		/** Is the current path done calculating.
		 * Returns true if the current #path has been returned or if the #path is null.
		 *
		 * \note Do not confuse this with Pathfinding.Path.IsDone. They usually return the same value, but not always
		 * since the path might be completely calculated, but it has not yet been processed by the Seeker.
		 *
		 * \since Added in 3.0.8
		 * \version Behaviour changed in 3.2
		 */
		public bool IsDone () {
			return path == null || path.PipelineState >= PathState.Returned;
		}

		/** Called when a path has completed.
		 * This should have been implemented as optional parameter values, but that didn't seem to work very well with delegates (the values weren't the default ones)
		 * \see OnPathComplete(Path,bool,bool)
		 */
		void OnPathComplete (Path path) {
			OnPathComplete(path, true, true);
		}

		/** Called when a path has completed.
		 * Will post process it and return it by calling #tmpPathCallback and #pathCallback
		 */
		void OnPathComplete (Path p, bool runModifiers, bool sendCallbacks) {
			if (p != null && p != path && sendCallbacks) {
				return;
			}

			if (this == null || p == null || p != path)
				return;
			if (!path.error && runModifiers) {
				// This will send the path for post processing to modifiers attached to this Seeker
				RunModifiers(ModifierPass.PostProcess, path);
			}
			
			if (sendCallbacks) {
				p.Claim(this);

				lastCompletedNodePath = p.path;
				lastCompletedVectorPath = p.vectorPath;

				// This will send the path to the callback (if any) specified when calling StartPath
				if (tmpPathCallback != null) {
					tmpPathCallback(p);
				}

				// This will send the path to any script which has registered to the callback
				if (pathCallback != null) {
					pathCallback(p);
				}

				// Recycle the previous path to reduce the load on the GC
				if (prevPath != null) {
					prevPath.Release(this, true);
				}

				prevPath = p;

				// If not drawing gizmos, then storing prevPath is quite unecessary
				// So clear it and set prevPath to null
				if (!drawGizmos) ReleaseClaimedPath();
			}
		}

		/** Called for each path in a MultiTargetPath.
		 * Only post processes the path, does not return it.
		 * \astarpro */
		void OnPartialPathComplete (Path p) {
			OnPathComplete(p, true, false);
		}

		/** Called once for a MultiTargetPath. Only returns the path, does not post process.
		 * \astarpro */
		void OnMultiPathComplete (Path p) {
			OnPathComplete(p, false, true);
		}

		/** Call this function to start calculating a path.
		 * Since this method does not take a callback parameter, you should set the #pathCallback field before calling this method.
		 *
		 * \param start		The start point of the path
		 * \param end		The end point of the path
		 */
		public Path StartPath (Vector3 start, Vector3 end) {
			return StartPath(start, end, null);
		}

		/** Call this function to start calculating a path.
		 *
		 * \param start		The start point of the path
		 * \param end		The end point of the path
		 * \param callback	The function to call when the path has been calculated
		 *
		 * \a callback will be called when the path has completed.
		 * \a Callback will not be called if the path is canceled (e.g when a new path is requested before the previous one has completed) */
		public Path StartPath (Vector3 start, Vector3 end, OnPathDelegate callback) {
			return StartPath(ABPath.Construct(start, end, null), callback);
		}

		/** Call this function to start calculating a path.
		 *
		 * \param start		The start point of the path
		 * \param end		The end point of the path
		 * \param callback	The function to call when the path has been calculated
		 * \param graphMask	Mask used to specify which graphs should be searched for close nodes. See #Pathfinding.NNConstraint.graphMask. This will override #graphMask for this path request.
		 *
		 * \a callback will be called when the path has completed.
		 * \a Callback will not be called if the path is canceled (e.g when a new path is requested before the previous one has completed) */
		public Path StartPath (Vector3 start, Vector3 end, OnPathDelegate callback, int graphMask) {
			return StartPath(ABPath.Construct(start, end, null), callback, graphMask);
		}

		/** Call this function to start calculating a path.
		 *
		 * \param p			The path to start calculating
		 * \param callback	The function to call when the path has been calculated
		 *
		 * The \a callback will be called when the path has been calculated (which may be several frames into the future).
		 * The \a callback will not be called if a new path request is started before this path request has been calculated.
		 *
		 * \version Since 3.8.3 this method works properly if a MultiTargetPath is used.
		 * It now behaves identically to the StartMultiTargetPath(MultiTargetPath) method.
		 *
		 * \version Since 4.1.x this method will no longer overwrite the graphMask on the path unless it is explicitly passed as a parameter (see other overloads of this method).
		 */
		public Path StartPath (Path p, OnPathDelegate callback = null) {
			// Set the graph mask only if the user has not changed it from the default value.
			// This is not perfect as the user may have wanted it to be precisely -1
			// however it is the best detection that I can do.
			// The non-default check is primarily for compatibility reasons to avoid breaking peoples existing code.
			// The StartPath overloads with an explicit graphMask field should be used instead to set the graphMask.
			if (p.nnConstraint.graphMask == -1) p.nnConstraint.graphMask = graphMask;
			StartPathInternal(p, callback);
			return p;
		}

		/** Call this function to start calculating a path.
		 *
		 * \param p			The path to start calculating
		 * \param callback	The function to call when the path has been calculated
		 * \param graphMask	Mask used to specify which graphs should be searched for close nodes. See #Pathfinding.NNConstraint.graphMask. This will override #graphMask for this path request.
		 *
		 * The \a callback will be called when the path has been calculated (which may be several frames into the future).
		 * The \a callback will not be called if a new path request is started before this path request has been calculated.
		 *
		 * \version Since 3.8.3 this method works properly if a MultiTargetPath is used.
		 * It now behaves identically to the StartMultiTargetPath(MultiTargetPath) method.
		 */
		public Path StartPath (Path p, OnPathDelegate callback, int graphMask) {
			p.nnConstraint.graphMask = graphMask;
			StartPathInternal(p, callback);
			return p;
		}

		/** Internal method to start a path and mark it as the currently active path */
		void StartPathInternal (Path p, OnPathDelegate callback) {
			var mtp = p as MultiTargetPath;

			if (mtp != null) {
				// TODO: Allocation, cache
				var callbacks = new OnPathDelegate[mtp.targetPoints.Length];

				for (int i = 0; i < callbacks.Length; i++) {
					callbacks[i] = onPartialPathDelegate;
				}

				mtp.callbacks = callbacks;
				p.callback += OnMultiPathComplete;
			} else {
				p.callback += onPathDelegate;
			}

			p.enabledTags = traversableTags;
			p.tagPenalties = tagPenalties;

			// Cancel a previously requested path is it has not been processed yet and also make sure that it has not been recycled and used somewhere else
			if (path != null && path.PipelineState <= PathState.Processing && path.CompleteState != PathCompleteState.Error && lastPathID == path.pathID) {
				path.FailWithError("Canceled path because a new one was requested.\n"+
					"This happens when a new path is requested from the seeker when one was already being calculated.\n" +
					"For example if a unit got a new order, you might request a new path directly instead of waiting for the now" +
					" invalid path to be calculated. Which is probably what you want.\n" +
					"If you are getting this a lot, you might want to consider how you are scheduling path requests.");
				// No callback will be sent for the canceled path
			}

			// Set p as the active path
			path = p;
			tmpPathCallback = callback;

			// Save the path id so we can make sure that if we cancel a path (see above) it should not have been recycled yet.
			lastPathID = path.pathID;

			// Pre process the path
			RunModifiers(ModifierPass.PreProcess, path);

			// Send the request to the pathfinder
			AstarPath.StartPath(path);
		}

		/** Draws gizmos for the Seeker */
		public void OnDrawGizmos () {
			if (lastCompletedNodePath == null || !drawGizmos) {
				return;
			}

			if (detailedGizmos) {
				Gizmos.color = new Color(0.7F, 0.5F, 0.1F, 0.5F);

				if (lastCompletedNodePath != null) {
					for (int i = 0; i < lastCompletedNodePath.Count-1; i++) {
						Gizmos.DrawLine((Vector3)lastCompletedNodePath[i].position, (Vector3)lastCompletedNodePath[i+1].position);
					}
				}
			}

			Gizmos.color = new Color(0, 1F, 0, 1F);

			if (lastCompletedVectorPath != null) {
				for (int i = 0; i < lastCompletedVectorPath.Count-1; i++) {
					Gizmos.DrawLine(lastCompletedVectorPath[i], lastCompletedVectorPath[i+1]);
				}
			}
		}
	}
}
