using System;
using System.Collections.Generic;

namespace PF
{
    public static class AstarDeserializer
    {
		/** Deserializes graphs from the specified byte array.
		 * An error will be logged if deserialization fails.
		 */
		public static NavGraph[] DeserializeGraphs (byte[] bytes)
		{
			NavGraph[] graphs = { new RecastGraph() };
			DeserializeGraphsAdditive(graphs, bytes);
			return graphs;
		}

		/** Deserializes graphs from the specified byte array additively.
		 * An error will be logged if deserialization fails.
		 * This function will add loaded graphs to the current ones.
		 */
		public static void DeserializeGraphsAdditive (NavGraph[] graphs, byte[] bytes) {
			try {
				if (bytes != null) {
					var sr = new AstarSerializer();

					if (sr.OpenDeserialize(bytes)) {
						DeserializeGraphsPartAdditive(graphs, sr);
						sr.CloseDeserialize();
					} else {
						throw new Exception("Invalid data file (cannot read zip).\nThe data is either corrupt or it was saved using a 3.0.x or earlier version of the system");
					}
				} else {
					throw new System.ArgumentNullException("bytes");
				}
				//AstarPath.active.VerifyIntegrity();
			} catch (System.Exception e) {
#if !SERVER
				UnityEngine.Debug.LogError("Caught exception while deserializing data.\n"+e);
#endif
				throw;
			}
		}

		/** Helper function for deserializing graphs */
		public static void DeserializeGraphsPartAdditive (NavGraph[] graphs, AstarSerializer sr) {
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
#if !SERVER
						UnityEngine.Debug.LogWarning("Guid Conflict when importing graphs additively. Imported graph will get a new Guid.\nThis message is (relatively) harmless.");
#endif
						graphs[i].guid = Guid.NewGuid();
						break;
					}
				}
			}
		}
    }
}