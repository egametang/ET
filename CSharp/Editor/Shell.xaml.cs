using System.Windows;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;

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
		ShellViewModel ViewModel
		{
			set
			{
				this.DataContext = value;
			}
		}
	}
}
