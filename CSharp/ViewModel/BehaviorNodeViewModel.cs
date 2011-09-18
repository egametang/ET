using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Egametang
{
	public class BehaviorNodeViewModel : INotifyPropertyChanged
	{
		private BehaviorNode node;
		private BehaviorNodeViewModel parent;
		private ObservableCollection<BehaviorNodeViewModel> children = 
			new ObservableCollection<BehaviorNodeViewModel>();

		private bool isExpanded;
		private bool isSelected;

		public BehaviorNodeViewModel(BehaviorNode node, BehaviorNodeViewModel parent)
		{
			this.node = node;
			this.parent = parent;
		}

		public ObservableCollection<BehaviorNodeViewModel> Children
		{
			get
			{
				return children;
			}
			set
			{
				children = value; 
			}
		}

		public string Name
		{
			get
			{
				return node.Name;
			}
		}

		public bool IsSelected
		{
			get 
			{ 
				return isSelected; 
			}
			set
			{
				if (value != isSelected)
				{
					isSelected = value;
					this.OnPropertyChanged("IsSelected");
				}
			}
		}

		public bool IsExpanded
		{
			get 
			{ 
				return isExpanded; 
			}
			set
			{
				if (value != isExpanded)
				{
					isExpanded = value;
					this.OnPropertyChanged("IsExpanded");
				}

				if (isExpanded && parent != null)
				{
					parent.IsExpanded = true;
				}

				this.LoadChildren();
			}
		}

		protected void LoadChildren()
		{
			foreach (var child in node.Children)
			{
				children.Add(new BehaviorNodeViewModel(child, this));
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
