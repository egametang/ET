using System.ComponentModel.Composition;
using System.Windows;

namespace Editor
{
	[Export]
	public partial class Shell : Window
	{
		public Shell()
		{
			this.InitializeComponent();
		}

		[Import]
		private ShellViewModel ViewModel
		{
			set
			{
				this.DataContext = value;
			}
		}
	}
}