using UnityEngine;
using System.Collections.Generic;
using PF;

namespace Pathfinding.Examples {
	/** Helper script in the example scene 'Turn Based' */
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_examples_1_1_turn_based_a_i.php")]
	public class TurnBasedAI : VersionedMonoBehaviour {
		public int movementPoints = 2;
		public BlockManager blockManager;
		public SingleNodeBlocker blocker;
		public GraphNode targetNode;
		public BlockManager.TraversalProvider traversalProvider;

		void Start () {
			blocker.BlockAtCurrentPosition();
		}

		protected override void Awake () {
			base.Awake();
			// Set the traversal provider to block all nodes that are blocked by a SingleNodeBlocker
			// except the SingleNodeBlocker owned by this AI (we don't want to be blocked by ourself)
			traversalProvider = new BlockManager.TraversalProvider(blockManager, BlockManager.BlockMode.AllExceptSelector, new List<SingleNodeBlocker>() { blocker });
		}
	}
}
