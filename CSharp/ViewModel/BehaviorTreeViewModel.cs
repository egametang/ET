using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Egametang
{
	public class BehaviorTreeViewModel : INotifyPropertyChanged
	{
		private BehaviorNode root = new BehaviorNode();
		private ObservableCollection<BehaviorNodeViewModel> children =
			new ObservableCollection<BehaviorNodeViewModel>();

		public BehaviorTreeViewModel()
		{
			root.Name = "root";
			root.Type = 1;
			children.Add(new BehaviorNodeViewModel(root, null));
		}

		public ObservableCollection<BehaviorNodeViewModel> Children
		{
			get
			{
				return children;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
