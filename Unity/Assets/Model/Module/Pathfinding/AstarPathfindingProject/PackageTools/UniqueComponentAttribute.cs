namespace Pathfinding {
	[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
	public class UniqueComponentAttribute : System.Attribute {
		public string tag;
	}
}
