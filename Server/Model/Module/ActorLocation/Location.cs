namespace ETModel
{
	public class Location: Entity
	{
		public string Address;

		public Location(long id, string address): base(id)
		{
			this.Address = address;
		}
	}
}
