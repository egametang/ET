using System.ComponentModel.Composition;
using Infrastructure;
using Log;
using RealmClient.Proxy;

namespace Modules.WCFClient
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

		private async void Button_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var calculator = new CalculatorClient();
			double result = await calculator.AddAsync(1, 1);
			Logger.Trace("WCF Calculator Add: 1 + 1 = {0}", result);
		}
    }
}
