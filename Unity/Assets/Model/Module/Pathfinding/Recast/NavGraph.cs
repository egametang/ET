namespace PF {
	/** Exposes internal methods for graphs.
	 * This is used to hide methods that should not be used by any user code
	 * but still have to be 'public' or 'internal' (which is pretty much the same as 'public'
	 * as this library is distributed with source code).
	 *
	 * Hiding the internal methods cleans up the documentation and IntelliSense suggestions.
	 */
	public interface IGraphInternals {
		string SerializedEditorSettings { get; set; }
		void OnDestroy ();
		void DestroyAllNodes ();
		void SerializeExtraInfo (GraphSerializationContext ctx);
		void DeserializeExtraInfo (GraphSerializationContext ctx);
		void PostDeserialization (GraphSerializationContext ctx);
		void DeserializeSettingsCompatibility (GraphSerializationContext ctx);
	}

	/** Base class for all graphs */
	public abstract class NavGraph : IGraphInternals {
		/** Used as an ID of the graph, considered to be unique.
		 * \note This is Pathfinding.Util.Guid not System.Guid. A replacement for System.Guid was coded for better compatibility with iOS
		 */
		[JsonMember]
		public Guid guid;

		/** Default penalty to apply to all nodes */
		[JsonMember]
		public uint initialPenalty;

		/** Is the graph open in the editor */
		[JsonMember]
		public bool open;

		/** Index of the graph, used for identification purposes */
		public uint graphIndex;

		/** Name of the graph.
		 * Can be set in the unity editor
		 */
		[JsonMember]
		public string name;

		/** Enable to draw gizmos in the Unity scene view.
		 * In the inspector this value corresponds to the state of
		 * the 'eye' icon in the top left corner of every graph inspector.
		 */
		[JsonMember]
		public bool drawGizmos = true;

		/** Used in the editor to check if the info screen is open.
		 * Should be inside UNITY_EDITOR only \#ifs but just in case anyone tries to serialize a NavGraph instance using Unity, I have left it like this as it would otherwise cause a crash when building.
		 * Version 3.0.8.1 was released because of this bug only
		 */
		[JsonMember]
		public bool infoScreenOpen;

		/** Used in the Unity editor to store serialized settings for graph inspectors */
		[JsonMember]
		string serializedEditorSettings;

		/** Number of nodes in the graph.
		 * Note that this is, unless the graph type has overriden it, an O(n) operation.
		 *
		 * This is an O(1) operation for grid graphs and point graphs.
		 * For layered grid graphs it is an O(n) operation.
		 */
		public virtual int CountNodes () {
			int count = 0;

			GetNodes(node => count++);
			return count;
		}

		/** Calls a delegate with all nodes in the graph until the delegate returns false */
		public void GetNodes (System.Func<GraphNode, bool> action) {
			bool cont = true;

			GetNodes(node => {
				if (cont) cont &= action(node);
			});
		}

		/** Calls a delegate with all nodes in the graph.
		 * This is the primary way of iterating through all nodes in a graph.
		 *
		 * Do not change the graph structure inside the delegate.
		 *
		 * \snippet MiscSnippets.cs NavGraph.GetNodes1
		 *
		 * If you want to store all nodes in a list you can do this
		 *
		 * \snippet MiscSnippets.cs NavGraph.GetNodes2
		 */
		public abstract void GetNodes (System.Action<GraphNode> action);

		/** Returns the nearest node to a position using the specified NNConstraint.
		 * \param position The position to try to find a close node to
		 * \param constraint Can for example tell the function to try to return a walkable node. If you do not get a good node back, consider calling GetNearestForce. */
		public NNInfoInternal GetNearest (Vector3 position, NNConstraint constraint) {
			return GetNearest(position, constraint, null);
		}

		/** Returns the nearest node to a position using the specified NNConstraint.
		 * \param position The position to try to find a close node to
		 * \param hint Can be passed to enable some graph generators to find the nearest node faster.
		 * \param constraint Can for example tell the function to try to return a walkable node. If you do not get a good node back, consider calling GetNearestForce.
		 */
		public virtual NNInfoInternal GetNearest (Vector3 position, NNConstraint constraint, GraphNode hint) {
			// This is a default implementation and it is pretty slow
			// Graphs usually override this to provide faster and more specialised implementations

			float maxDistSqr = constraint == null || constraint.constrainDistance ? PathFindHelper.GetConfig().maxNearestNodeDistanceSqr : float.PositiveInfinity;

			float minDist = float.PositiveInfinity;
			GraphNode minNode = null;

			float minConstDist = float.PositiveInfinity;
			GraphNode minConstNode = null;

			// Loop through all nodes and find the closest suitable node
			GetNodes(node => {
				float dist = (position-(Vector3)node.position).sqrMagnitude;

				if (dist < minDist) {
					minDist = dist;
					minNode = node;
				}

				if (dist < minConstDist && dist < maxDistSqr && (constraint == null || constraint.Suitable(node))) {
					minConstDist = dist;
					minConstNode = node;
				}
			});

			var nnInfo = new NNInfoInternal(minNode);

			nnInfo.constrainedNode = minConstNode;

			if (minConstNode != null) {
				nnInfo.constClampedPosition = (Vector3)minConstNode.position;
			} else if (minNode != null) {
				nnInfo.constrainedNode = minNode;
				nnInfo.constClampedPosition = (Vector3)minNode.position;
			}

			return nnInfo;
		}

		/**
		 * Returns the nearest node to a position using the specified \link Pathfinding.NNConstraint constraint \endlink.
		 * \returns an NNInfo. This method will only return an empty NNInfo if there are no nodes which comply with the specified constraint.
		 */
		public virtual NNInfoInternal GetNearestForce (Vector3 position, NNConstraint constraint) {
			return GetNearest(position, constraint);
		}

		/** Function for cleaning up references.
		 * This will be called on the same time as OnDisable on the gameObject which the AstarPath script is attached to (remember, not in the editor).
		 * Use for any cleanup code such as cleaning up static variables which otherwise might prevent resources from being collected.
		 * Use by creating a function overriding this one in a graph class, but always call base.OnDestroy () in that function.
		 * All nodes should be destroyed in this function otherwise a memory leak will arise.
		 */
		protected virtual void OnDestroy () {
			DestroyAllNodes();
		}

		/** Destroys all nodes in the graph.
		 * \warning This is an internal method. Unless you have a very good reason, you should probably not call it.
		 */
		protected virtual void DestroyAllNodes () {
			GetNodes(node => node.Destroy());
		}

		/** Serializes graph type specific node data.
		 * This function can be overriden to serialize extra node information (or graph information for that matter)
		 * which cannot be serialized using the standard serialization.
		 * Serialize the data in any way you want and return a byte array.
		 * When loading, the exact same byte array will be passed to the DeserializeExtraInfo function.\n
		 * These functions will only be called if node serialization is enabled.\n
		 */
		protected virtual void SerializeExtraInfo (GraphSerializationContext ctx) {
		}

		/** Deserializes graph type specific node data.
		 * \see SerializeExtraInfo
		 */
		protected virtual void DeserializeExtraInfo (GraphSerializationContext ctx) {
		}

		/** Called after all deserialization has been done for all graphs.
		 * Can be used to set up more graph data which is not serialized
		 */
		protected virtual void PostDeserialization (GraphSerializationContext ctx) {
		}

		/** An old format for serializing settings.
		 * \deprecated This is deprecated now, but the deserialization code is kept to
		 * avoid loosing data when upgrading from older versions.
		 */
		protected virtual void DeserializeSettingsCompatibility (GraphSerializationContext ctx) {
			guid = new Guid(ctx.reader.ReadBytes(16));
			initialPenalty = ctx.reader.ReadUInt32();
			open = ctx.reader.ReadBoolean();
			name = ctx.reader.ReadString();
			drawGizmos = ctx.reader.ReadBoolean();
			infoScreenOpen = ctx.reader.ReadBoolean();
		}

		#region IGraphInternals implementation
		string IGraphInternals.SerializedEditorSettings { get { return serializedEditorSettings; } set { serializedEditorSettings = value; } }
		void IGraphInternals.OnDestroy () { OnDestroy(); }
		void IGraphInternals.DestroyAllNodes () { DestroyAllNodes(); }
		void IGraphInternals.SerializeExtraInfo (GraphSerializationContext ctx) { SerializeExtraInfo(ctx); }
		void IGraphInternals.DeserializeExtraInfo (GraphSerializationContext ctx) { DeserializeExtraInfo(ctx); }
		void IGraphInternals.PostDeserialization (GraphSerializationContext ctx) { PostDeserialization(ctx); }
		void IGraphInternals.DeserializeSettingsCompatibility (GraphSerializationContext ctx) { DeserializeSettingsCompatibility(ctx); }

		#endregion
	}


}
