namespace Model
{
	public class ClientConfig: AConfigComponent
	{
		public string Host = "";
		public int Port;
		
		public string Address
		{
			get
			{
				return $"{this.Host}:{this.Port}";
			}
		}
	}
}