namespace Model
{
	public class Location: EntityDB
	{
		public string Address;

		public Location(long id, string address): base(id)
		{
			this.Address = address;
		}
	}
}
