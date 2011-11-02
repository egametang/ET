using GalaSoft.MvvmLight.Messaging;
using System.Windows.Controls;

namespace Egametang
{
	/// <summary>
	/// MainView.xaml 的交互逻辑
	/// </summary>
	public partial class MainView : UserControl
	{
		public MainView()
		{
			InitializeComponent();
			Messenger.Default.Register<NotificationMessage>(this, TextBoxInfoScrollToEnd);
		}

		private void TextBoxInfoScrollToEnd(NotificationMessage msg)
		{
			tBInfo.ScrollToEnd();
		}
	}
}
