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
