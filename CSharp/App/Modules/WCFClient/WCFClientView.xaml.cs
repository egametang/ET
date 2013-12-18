using System.ComponentModel.Composition;
using Infrastructure;

namespace WCFClient
{
	/// <summary>
	/// WCFClientView.xaml 的交互逻辑
	/// </summary>
	[ViewExport(RegionName = "WCFClientRegion"), PartCreationPolicy(CreationPolicy.NonShared)]
	public partial class WCFClientView
	{
		public WCFClientView()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
		{
		}
	}
}
