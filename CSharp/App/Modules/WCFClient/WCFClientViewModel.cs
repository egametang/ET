using System.ComponentModel.Composition;

namespace Modules.WCFClient
{
	[Export(contractType: typeof(WCFClientViewModel)),
		PartCreationPolicy(creationPolicy: CreationPolicy.NonShared)]
	public class WCFClientViewModel
	{
	}
}
