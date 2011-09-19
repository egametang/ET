using System.Windows;

namespace Egametang
{
	/// <summary>
	/// BehaviorNodeDialog.xaml 的交互逻辑
	/// </summary>
	public partial class BehaviorNodeDialog : Window
	{
		private BehaviorNode node;

		public BehaviorNode Node
		{
			get 
			{ 
				return node; 
			}
			set 
			{ 
				node = value; 
			}
		}

		public BehaviorNodeDialog()
		{
			InitializeComponent();
		}
	}
}
