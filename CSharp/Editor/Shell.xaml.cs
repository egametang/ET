using System.ComponentModel.Composition;
using System.Windows;

namespace Editor
{
	[Export]
	public partial class Shell : Window
	{
		public Shell()
		{
			InitializeComponent();
		}

		[Import]
		private ShellViewModel ViewModel
		{
			set
			{
				DataContext = value;
			}
		}
	}
}