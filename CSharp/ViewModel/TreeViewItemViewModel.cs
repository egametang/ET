using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Egametang
{
	/// <summary>
	/// Base class for all ViewModel classes displayed by TreeViewItems.  
	/// This acts as an adapter between a raw data object and a TreeViewItem.
	/// </summary>
	public class TreeViewItemViewModel : INotifyPropertyChanged
	{
		#region Data

		static readonly TreeViewItemViewModel dummyChild = new TreeViewItemViewModel();

		readonly ObservableCollection<TreeViewItemViewModel> children;
		readonly TreeViewItemViewModel parent;

		bool isExpanded;
		bool isSelected;

		#endregion // Data

		#region Constructors

		protected TreeViewItemViewModel(TreeViewItemViewModel parent, bool lazyLoadChildren)
		{
			this.parent = parent;

			children = new ObservableCollection<TreeViewItemViewModel>();

			if (lazyLoadChildren)
			{
				children.Add(dummyChild);
			}
		}

		// This is used to create the DummyChild instance.
		private TreeViewItemViewModel()
		{
		}

		#endregion // Constructors

		#region Children

		/// <summary>
		/// Returns the logical child items of this object.
		/// </summary>
		public ObservableCollection<TreeViewItemViewModel> Children
		{
			get 
			{
				return children; 
			}
		}

		#endregion // Children

		#region HasLoadedChildren

		/// <summary>
		/// Returns true if this object's Children have not yet been populated.
		/// </summary>
		public bool HasDummyChild
		{
			get 
			{ 
				return this.Children.Count == 1 && this.Children[0] == dummyChild; 
			}
		}

		#endregion // HasLoadedChildren

		#region IsExpanded

		/// <summary>
		/// Gets/sets whether the TreeViewItem 
		/// associated with this object is expanded.
		/// </summary>
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

				// Expand all the way up to the root.
				if (isExpanded && parent != null)
				{
					parent.IsExpanded = true;
				}

				// Lazy load the child items, if necessary.
				if (this.HasDummyChild)
				{
					this.Children.Remove(dummyChild);
					this.LoadChildren();
				}
			}
		}

		#endregion // IsExpanded

		#region IsSelected

		/// <summary>
		/// Gets/sets whether the TreeViewItem 
		/// associated with this object is selected.
		/// </summary>
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

		#endregion // IsSelected

		#region LoadChildren

		/// <summary>
		/// Invoked when the child items need to be loaded on demand.
		/// Subclasses can override this to populate the Children collection.
		/// </summary>
		protected virtual void LoadChildren()
		{
		}

		#endregion // LoadChildren

		#region Parent

		public TreeViewItemViewModel Parent
		{
			get 
			{ 
				return parent; 
			}
		}

		#endregion // Parent

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion // INotifyPropertyChanged Members
	}
}
