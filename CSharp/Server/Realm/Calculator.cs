using RealmContract;

namespace Realm
{
	public class Calculator : ICalculator
	{
		public double Add(double x, double y)
		{
			return x + y;
		}
	}
}
