using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel.Composition;
using Infrastructure;

namespace Module.TreeCanvas
{
	/// <summary>
	/// TreeCanvasView.xaml 的交互逻辑
	/// </summary>
	[ViewExport(RegionName = "TreeCanvasRegion")]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	public partial class TreeCanvasView : UserControl
	{
		public TreeCanvasView()
		{
			InitializeComponent();
		}

		[Import]
		TreeCanvasViewModel ViewModel
		{
			get
			{
				return this.DataContext as TreeCanvasViewModel;
			}
			set
			{
				this.DataContext = value;
			}
		}
	}
}
