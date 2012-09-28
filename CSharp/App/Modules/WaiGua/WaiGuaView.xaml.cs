using System.ComponentModel.Composition;
using Infrastructure;

namespace Modules.WaiGua
{
	/// <summary>
	/// RobotView.xaml 的交互逻辑
	/// </summary>
	[ViewExport(RegionName = "WaiGuaRegion"), PartCreationPolicy(CreationPolicy.NonShared)]
	public partial class WaiGuaView
	{
		public WaiGuaView()
		{
			this.InitializeComponent();
		}

		[Import]
		private WaiGuaViewModel ViewModel
		{
			get
			{
				return this.DataContext as WaiGuaViewModel;
			}
			set
			{
				this.DataContext = value;
			}
		}
	}
}