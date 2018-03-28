namespace ETModel
{
	public class NodeDesignerProto: IConfig
	{
		public string name;
		public float x = 0;
		public float y = 0;
		public bool fold = false;

		public long Id { get; set; }
	}
}