using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.ViewModel;
using NLog;

namespace Module.TreeCanvas
{
	[Export(typeof(TreeCanvasViewModel))]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	class TreeCanvasViewModel : NotificationObject
	{
		private Logger logger = LogManager.GetCurrentClassLogger();
		public TreeCanvasViewModel()
		{
			logger.Debug("11111");
		}
	}
}

