using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.ViewModel;
using NLog;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using System.Collections.ObjectModel;
using System.Windows;

namespace Module.TreeCanvas
{
	[Export(typeof(TreeCanvasViewModel))]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	class TreeCanvasViewModel : NotificationObject
	{
		private Logger logger = LogManager.GetCurrentClassLogger();

		private ObservableCollection<Node> nodes = new ObservableCollection<Node>();
		private ObservableCollection<Arrow> arrows = new ObservableCollection<Arrow>();
				
		public TreeCanvasViewModel()
		{
			logger.Debug("TreeCanvasViewModel");
		}

		public ObservableCollection<Node> Nodes
		{
			get
			{
				return nodes;
			}
		}

		public ObservableCollection<Arrow> Arrows
		{
			get
			{
				return arrows;
			}
		}

		public void NewNode(Point point)
		{
			Nodes.Add(new Node(point));
		}
	}
}

