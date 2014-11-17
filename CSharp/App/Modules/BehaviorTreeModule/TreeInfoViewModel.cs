namespace Modules.BehaviorTreeModule
{
	public class TreeInfoViewModel
	{
		private readonly int id;
		private readonly string comment;

		public TreeInfoViewModel(int id, string comment)
		{
			this.id = id;
			this.comment = comment;
		}

		public int Id
		{
			get
			{
				return this.id;
			}
		}

		public string Comment
		{
			get
			{
				return this.comment;
			}
		}
	}
}