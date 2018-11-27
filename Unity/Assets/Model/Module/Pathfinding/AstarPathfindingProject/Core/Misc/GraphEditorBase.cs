using PF;

namespace Pathfinding {
	[JsonOptIn]
	/** Defined here only so non-editor classes can use the #target field */
	public class GraphEditorBase {
		/** NavGraph this editor is exposing */
		public NavGraph target;
	}
}
