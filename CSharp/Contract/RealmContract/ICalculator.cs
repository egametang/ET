using System.ServiceModel;

namespace RealmContract
{
	[ServiceContract(Name = "Calculator")]
	public interface ICalculator
	{
		[OperationContract]
		double Add(double x, double y);
	}
}