namespace Pathfinding {
	/** Added to editors of custom graph types */
	[System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = true)]
	public class CustomGraphEditorAttribute : System.Attribute {
		/** Graph type which this is an editor for */
		public System.Type graphType;

		/** Name displayed in the inpector */
		public string displayName;

		/** Type of the editor for the graph */
		public System.Type editorType;

		public CustomGraphEditorAttribute (System.Type t, string displayName) {
			graphType = t;
			this.displayName = displayName;
		}
	}
}
