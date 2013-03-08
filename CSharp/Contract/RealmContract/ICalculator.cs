using System.ServiceModel;

namespace RealmContract
{
	[ServiceContract(Name = "Calculator", Namespace = "http://www.egametang.com/")]
	public interface ICalculator
	{
		[OperationContract]
		double Add(double x, double y);
	}
}