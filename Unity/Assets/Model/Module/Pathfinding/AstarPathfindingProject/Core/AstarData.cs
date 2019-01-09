using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PF;

#if UNITY_WINRT && !UNITY_EDITOR
//using MarkerMetro.Unity.WinLegacy.IO;
//using MarkerMetro.Unity.WinLegacy.Reflection;
#endif

namespace Pathfinding {
	[System.Serializable]
	/** Stores the navigation graphs for the A* Pathfinding System.
	 * \ingroup relevant
	 *
	 * An instance of this class is assigned to AstarPath.data, from it you can access all graphs loaded through the #graphs variable.\n
	 * This class also handles a lot of the high level serialization.
	 */
	public class AstarData {

		#region Fields

		/** Shortcut to the first RecastGraph.
		 * Updated at scanning time.
		 * \astarpro
		 */
		public RecastGraph recastGraph { get; private set; }

		/** All supported graph types.
		 * Populated through reflection search
		 */
		public System.Type[] graphTypes { get; private set; }

#if ASTAR_FAST_NO_EXCEPTIONS || UNITY_WINRT || UNITY_WEBGL
		/** Graph types to use when building with Fast But No Exceptions for iPhone.
		 * If you add any custom graph types, you need to add them to this hard-coded list.
		 */
		public static readonly System.Type[] DefaultGraphTypes = new System.Type[] {
#if !ASTAR_NO_GRID_GRAPH
			typeof(GridGraph),
#endif
#if !ASTAR_NO_POINT_GRAPH
			typeof(PointGraph),
#endif
			typeof(NavMeshGraph),
			typeof(RecastGraph),
			typeof(LayerGridGraph)
		};
#endif

		/** All graphs this instance holds.
		 * This will be filled only after deserialization has completed.
		 * May contain null entries if graph have been removed.
		 */
		[System.NonSerialized]
		public NavGraph[] graphs = new NavGraph[0];

		//Serialization Settings

		/** Serialized data for all graphs and settings.
		 * Stored as a base64 encoded string because otherwise Unity's Undo system would sometimes corrupt the byte data (because it only stores deltas).
		 *
		 * This can be accessed as a byte array from the #data property.
		 *
		 * \since 3.6.1
		 */
		[SerializeField]
		string dataString;

		/** Data from versions from before 3.6.1.
		 * Used for handling upgrades
		 * \since 3.6.1
		 */
		[SerializeField]
		[UnityEngine.Serialization.FormerlySerializedAs("data")]
		private byte[] upgradeData;

		/** Serialized data for all graphs and settings */
		private byte[] data {
			get {
				// Handle upgrading from earlier versions than 3.6.1
				if (upgradeData != null && upgradeData.Length > 0) {
					data = upgradeData;
					upgradeData = null;
				}
				return dataString != null ? System.Convert.FromBase64String(dataString) : null;
			}
			set {
				dataString = value != null ? System.Convert.ToBase64String(value) : null;
			}
		}

		/** Serialized data for cached startup.
		 * If set, on start the graphs will be deserialized from this file.
		 */
		public TextAsset file_cachedStartup;

		/** Serialized data for cached startup.
		 *
		 * \deprecated Deprecated since 3.6, AstarData.file_cachedStartup is now used instead
		 */
		public byte[] data_cachedStartup;

		/** Should graph-data be cached.
		 * Caching the startup means saving the whole graphs - not only the settings - to a file (#file_cachedStartup) which can
		 * be loaded when the game starts. This is usually much faster than scanning the graphs when the game starts. This is configured from the editor under the "Save & Load" tab.
		 *
		 * \see \ref save-load-graphs
		 */
		[SerializeField]
		public bool cacheStartup;

		//End Serialization Settings

		List<bool> graphStructureLocked = new List<bool>();

		#endregion

		public byte[] GetData () {
			return data;
		}

		public void SetData (byte[] data) {
			this.data = data;
		}

		/** Loads the graphs from memory, will load cached graphs if any exists */
		public void Awake () {
			graphs = new NavGraph[0];

			if (cacheStartup && file_cachedStartup != null) {
				LoadFromCache();
			} else {
				DeserializeGraphs();
			}
		}

		/** Prevent the graph structure from changing during the time this lock is held.
		 * This prevents graphs from being added or removed and also prevents graphs from being serialized or deserialized.
		 * This is used when e.g an async scan is happening to ensure that for example a graph that is being scanned is not destroyed.
		 *
		 * Each call to this method *must* be paired with exactly one call to #UnlockGraphStructure.
		 * The calls may be nested.
		 */
		internal void LockGraphStructure (bool allowAddingGraphs = false) {
			graphStructureLocked.Add(allowAddingGraphs);
		}

		/** Allows the graph structure to change again.
		 * \see #LockGraphStructure
		 */
		internal void UnlockGraphStructure () {
			if (graphStructureLocked.Count == 0) throw new System.InvalidOperationException();
			graphStructureLocked.RemoveAt(graphStructureLocked.Count - 1);
		}

		PathProcessor.GraphUpdateLock AssertSafe (bool onlyAddingGraph = false) {
			if (graphStructureLocked.Count > 0) {
				bool allowAdding = true;
				for (int i = 0; i < graphStructureLocked.Count; i++) allowAdding &= graphStructureLocked[i];
				if (!(onlyAddingGraph && allowAdding)) throw new System.InvalidOperationException("Graphs cannot be added, removed or serialized while the graph structure is locked. This is the case when a graph is currently being scanned and when executing graph updates and work items.\nHowever as a special case, graphs can be added inside work items.");
			}

			// Pause the pathfinding threads
			var graphLock = AstarPath.active.PausePathfinding();
			if (!AstarPath.active.IsInsideWorkItem) {
				// Make sure all graph updates and other callbacks are done
				// Only do this if this code is not being called from a work item itself as that would cause a recursive wait that could never complete.
				// There are some valid cases when this can happen. For example it may be necessary to add a new graph inside a work item.
				AstarPath.active.FlushWorkItems();

				// Paths that are already calculated and waiting to be returned to the Seeker component need to be
				// processed immediately as their results usually depend on graphs that currently exist. If this was
				// not done then after destroying a graph one could get a path result with destroyed nodes in it.
				AstarPath.active.pathReturnQueue.ReturnPaths(false);
			}
			return graphLock;
		}

		/** Updates shortcuts to the first graph of different types.
		 * Hard coding references to some graph types is not really a good thing imo. I want to keep it dynamic and flexible.
		 * But these references ease the use of the system, so I decided to keep them.
		 */
		public void UpdateShortcuts () {
			recastGraph = (RecastGraph)FindGraphOfType(typeof(RecastGraph));
		}

		/** Load from data from #file_cachedStartup */
		public void LoadFromCache () {
			var graphLock = AssertSafe();

			if (file_cachedStartup != null) {
				var bytes = file_cachedStartup.bytes;
				DeserializeGraphs(bytes);

				GraphModifier.TriggerEvent(GraphModifier.EventType.PostCacheLoad);
			} else {
				Debug.LogError("Can't load from cache since the cache is empty");
			}
			graphLock.Release();
		}

		#region Serialization

		/** Serializes all graphs settings to a byte array.
		 * \see DeserializeGraphs(byte[])
		 */
		public byte[] SerializeGraphs () {
			return SerializeGraphs(SerializeSettings.Settings);
		}

		/** Serializes all graphs settings and optionally node data to a byte array.
		 * \see DeserializeGraphs(byte[])
		 * \see Pathfinding.Serialization.SerializeSettings
		 */
		public byte[] SerializeGraphs (SerializeSettings settings) {
			uint checksum;

			return SerializeGraphs(settings, out checksum);
		}

		/** Main serializer function.
		 * Serializes all graphs to a byte array
		 * A similar function exists in the AstarPathEditor.cs script to save additional info */
		public byte[] SerializeGraphs (SerializeSettings settings, out uint checksum) {
			var graphLock = AssertSafe();
			var sr = new AstarSerializer(settings);

			sr.OpenSerialize();
			sr.SerializeGraphs(graphs);
			sr.SerializeExtraInfo();
			byte[] bytes = sr.CloseSerialize();
			checksum = sr.GetChecksum();
	#if ASTARDEBUG
			Debug.Log("Got a whole bunch of data, "+bytes.Length+" bytes");
	#endif
			graphLock.Release();
			return bytes;
		}

		/** Deserializes graphs from #data */
		public void DeserializeGraphs () {
			if (data != null) {
				DeserializeGraphs(data);
			}
		}

		/** Destroys all graphs and sets graphs to null */
		void ClearGraphs () {
			if (graphs == null) return;
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] != null) {
					((IGraphInternals)graphs[i]).OnDestroy();
				}
			}
			graphs = null;
			UpdateShortcuts();
		}

		public void OnDestroy () {
			ClearGraphs();
		}

		/** Deserializes graphs from the specified byte array.
		 * An error will be logged if deserialization fails.
		 */
		public void DeserializeGraphs (byte[] bytes) {
			var graphLock = AssertSafe();

			ClearGraphs();
			DeserializeGraphsAdditive(bytes);
			graphLock.Release();
		}

		/** Deserializes graphs from the specified byte array additively.
		 * An error will be logged if deserialization fails.
		 * This function will add loaded graphs to the current ones.
		 */
		public void DeserializeGraphsAdditive (byte[] bytes) {
			var graphLock = AssertSafe();

			try {
				if (bytes != null) {
					var sr = new AstarSerializer();

					if (sr.OpenDeserialize(bytes)) {
						DeserializeGraphsPartAdditive(sr);
						sr.CloseDeserialize();
					} else {
						Debug.Log("Invalid data file (cannot read zip).\nThe data is either corrupt or it was saved using a 3.0.x or earlier version of the system");
					}
				} else {
					throw new System.ArgumentNullException("bytes");
				}
				AstarPath.active.VerifyIntegrity();
			} catch (System.Exception e) {
				Debug.LogError("Caught exception while deserializing data.\n"+e);
				graphs = new NavGraph[0];
			}

			UpdateShortcuts();
			graphLock.Release();
		}

		/** Helper function for deserializing graphs */
		void DeserializeGraphsPartAdditive (AstarSerializer sr) {
			if (graphs == null) graphs = new NavGraph[0];

			var gr = new List<NavGraph>(graphs);

			// Set an offset so that the deserializer will load
			// the graphs with the correct graph indexes
			sr.SetGraphIndexOffset(gr.Count);

			gr.AddRange(sr.DeserializeGraphs());
			graphs = gr.ToArray();

			sr.DeserializeEditorSettingsCompatibility();
			sr.DeserializeExtraInfo();

			//Assign correct graph indices.
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] == null) continue;
				graphs[i].GetNodes(node => node.GraphIndex = (uint)i);
			}

			for (int i = 0; i < graphs.Length; i++) {
				for (int j = i+1; j < graphs.Length; j++) {
					if (graphs[i] != null && graphs[j] != null && graphs[i].guid == graphs[j].guid) {
						Debug.LogWarning("Guid Conflict when importing graphs additively. Imported graph will get a new Guid.\nThis message is (relatively) harmless.");
						graphs[i].guid = Guid.NewGuid();
						break;
					}
				}
			}

			sr.PostDeserialization();
		}

		#endregion

		/** Find all graph types supported in this build.
		 * Using reflection, the assembly is searched for types which inherit from NavGraph. */
		public void FindGraphTypes () {
#if !ASTAR_FAST_NO_EXCEPTIONS && !UNITY_WINRT && !UNITY_WEBGL
			var assembly = WindowsStoreCompatibility.GetTypeInfo(typeof(AstarPath)).Assembly;
			System.Type[] types = assembly.GetTypes();
			var graphList = new List<System.Type>();

			foreach (System.Type type in types) {
#if NETFX_CORE && !UNITY_EDITOR
				System.Type baseType = type.GetTypeInfo().BaseType;
#else
				System.Type baseType = type.BaseType;
#endif
				while (baseType != null) {
					if (System.Type.Equals(baseType, typeof(NavGraph))) {
						graphList.Add(type);

						break;
					}

#if NETFX_CORE && !UNITY_EDITOR
					baseType = baseType.GetTypeInfo().BaseType;
#else
					baseType = baseType.BaseType;
#endif
				}
			}

			graphTypes = graphList.ToArray();

#if ASTARDEBUG
			Debug.Log("Found "+graphTypes.Length+" graph types");
#endif
#else
			graphTypes = DefaultGraphTypes;
#endif
		}

		#region GraphCreation

		/** Creates a new graph instance of type \a type
		 * \see #CreateGraph(string)
		 */
		internal NavGraph CreateGraph (System.Type type) {
			var graph = System.Activator.CreateInstance(type) as NavGraph;

			return graph;
		}

		/** Adds a graph of type \a type to the #graphs array */
		public NavGraph AddGraph (System.Type type) {
			NavGraph graph = null;

			for (int i = 0; i < graphTypes.Length; i++) {
				if (System.Type.Equals(graphTypes[i], type)) {
					graph = CreateGraph(graphTypes[i]);
				}
			}

			if (graph == null) {
				Debug.LogError("No NavGraph of type '"+type+"' could be found, "+graphTypes.Length+" graph types are avaliable");
				return null;
			}

			AddGraph(graph);

			return graph;
		}

		/** Adds the specified graph to the #graphs array */
		void AddGraph (NavGraph graph) {
			// Make sure to not interfere with pathfinding
			var graphLock = AssertSafe(true);

			// Try to fill in an empty position
			bool foundEmpty = false;

			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] == null) {
					graphs[i] = graph;
					graph.graphIndex = (uint)i;
					foundEmpty = true;
					break;
				}
			}

			if (!foundEmpty) {
				if (graphs != null && graphs.Length >= GraphNode.MaxGraphIndex) {
					throw new System.Exception("Graph Count Limit Reached. You cannot have more than " + GraphNode.MaxGraphIndex + " graphs.");
				}

				// Add a new entry to the list
				var graphList = new List<NavGraph>(graphs ?? new NavGraph[0]);
				graphList.Add(graph);
				graphs = graphList.ToArray();
				graph.graphIndex = (uint)(graphs.Length-1);
			}

			UpdateShortcuts();
			graphLock.Release();
		}

		/** Removes the specified graph from the #graphs array and Destroys it in a safe manner.
		 * To avoid changing graph indices for the other graphs, the graph is simply nulled in the array instead
		 * of actually removing it from the array.
		 * The empty position will be reused if a new graph is added.
		 *
		 * \returns True if the graph was sucessfully removed (i.e it did exist in the #graphs array). False otherwise.
		 *
		 * \version Changed in 3.2.5 to call SafeOnDestroy before removing
		 * and nulling it in the array instead of removing the element completely in the #graphs array.
		 */
		public bool RemoveGraph (NavGraph graph) {
			// Make sure the pathfinding threads are stopped
			// If we don't wait until pathfinding that is potentially running on
			// this graph right now we could end up with NullReferenceExceptions
			var graphLock = AssertSafe();

			((IGraphInternals)graph).OnDestroy();

			int i = System.Array.IndexOf(graphs, graph);
			if (i != -1) graphs[i] = null;

			UpdateShortcuts();
			graphLock.Release();
			return i != -1;
		}

		#endregion

		#region GraphUtility

		/** Returns the first graph which satisfies the predicate. Returns null if no graph was found. */
		public NavGraph FindGraph (System.Func<NavGraph, bool> predicate) {
			if (graphs != null) {
				for (int i = 0; i < graphs.Length; i++) {
					if (graphs[i] != null && predicate(graphs[i])) {
						return graphs[i];
					}
				}
			}
			return null;
		}

		/** Returns the first graph of type \a type found in the #graphs array. Returns null if no graph was found. */
		public NavGraph FindGraphOfType (System.Type type) {
			return FindGraph(graph => System.Type.Equals(graph.GetType(), type));
		}

		/** Returns the first graph which inherits from the type \a type. Returns null if no graph was found. */
		public NavGraph FindGraphWhichInheritsFrom (System.Type type) {
			return FindGraph(graph => WindowsStoreCompatibility.GetTypeInfo(type).IsAssignableFrom(WindowsStoreCompatibility.GetTypeInfo(graph.GetType())));
		}

		/** Loop through this function to get all graphs of type 'type'
		 * \code
		 * foreach (GridGraph graph in AstarPath.data.FindGraphsOfType (typeof(GridGraph))) {
		 *     //Do something with the graph
		 * }
		 * \endcode
		 * \see AstarPath.RegisterSafeNodeUpdate */
		public IEnumerable FindGraphsOfType (System.Type type) {
			if (graphs == null) yield break;
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] != null && System.Type.Equals(graphs[i].GetType(), type)) {
					yield return graphs[i];
				}
			}
		}

		/** Gets the index of the NavGraph in the #graphs array */
		public int GetGraphIndex (NavGraph graph) {
			if (graph == null) throw new System.ArgumentNullException("graph");

			var index = -1;
			if (graphs != null) {
				index = System.Array.IndexOf(graphs, graph);
				if (index == -1) Debug.LogError("Graph doesn't exist");
			}
			return index;
		}

		#endregion
	}
}
