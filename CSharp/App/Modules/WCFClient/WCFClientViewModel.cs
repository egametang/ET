using System.ComponentModel.Composition;

namespace WCFClient
{
	[Export(contractType: typeof(WCFClientViewModel)),
		PartCreationPolicy(creationPolicy: CreationPolicy.NonShared)]
	public class WCFClientViewModel
	{
	}
}
