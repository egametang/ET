using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BehaviorTree
{
	public class TreeNode : ContentControl, ITreeNode
	{
		public static readonly DependencyProperty CollapsedProperty =
			DependencyProperty.Register(
				"Collapsed",
				typeof(bool),
				typeof(TreeNode),
				new PropertyMetadata(
					false,
					CollapsePropertyChange
				));

		static public void CollapsePropertyChange(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			TreeNode tn = o as TreeNode;
			if (tn != null && tn.Collapsible)
			{
				bool fCollapsed = ((bool)e.NewValue);
				foreach (TreeNode tnCur in LayeredTreeDraw.VisibleDescendants<TreeNode>(tn))
				{
					tnCur.Visibility = fCollapsed ? Visibility.Collapsed : Visibility.Visible;
				}
			}
		}

		public bool Collapsed
		{
			get { return (bool)GetValue(CollapsedProperty); }
			set { SetValue(CollapsedProperty, value); }
		}

		public static readonly DependencyProperty CollapsibleProperty =
			DependencyProperty.Register(
				"Collapsible",
				typeof(bool),
				typeof(TreeNode),
				new PropertyMetadata(
					true,
					CollapsiblePropertyChange
				));

		static public void CollapsiblePropertyChange(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			TreeNode tn = o as TreeNode;
			if (((bool)e.NewValue) == false && tn != null)
			{
				tn.Collapsed = false;
			}
		}

		public bool Collapsible
		{
			get { return (bool)GetValue(CollapsibleProperty); }
			set { SetValue(CollapsibleProperty, value); }
		}

		public static readonly DependencyProperty TreeParentProperty =
			DependencyProperty.Register(
				"TreeParent",
				typeof(string),
				typeof(TreeNode),
				new PropertyMetadata(
					null,
					null
				));

		public static TreeNode GetParentElement(TreeNode tn)
		{
			BehaviorPanel tc;
			TreeNode tnParent;

			if (tn == null)
			{
				return null;
			}
			tc = tn.Parent as BehaviorPanel;
			if (tc == null)
			{
				return null;
			}
			string strParent = tn.TreeParent;
			if (strParent == null)
			{
				return null;
			}

			tnParent = tc.FindName(strParent) as TreeNode;
			if (tnParent == null)
			{
				return null;
			}
			return tnParent;
		}

		public string TreeParent
		{
			get { return (string)GetValue(TreeParentProperty); }
			set { SetValue(TreeParentProperty, value); }
		}

		public TreeNode()
		{
			TreeChildren = new TreeNodeGroup();
			Background = new SolidColorBrush();
		}

		static TreeNode()
		{
		}

		internal void ClearParent()
		{
			TreeChildren = new TreeNodeGroup();
		}

		internal bool SetParent()
		{
			TreeNode tn = GetParentElement(this);
			if (tn == null)
			{
				return false;
			}
			tn.TreeChildren.Add(this);
			return true;
		}

		public object PrivateNodeInfo { get; set; }

		public TreeNodeGroup TreeChildren { get; private set; }

		internal Size NodeSize()
		{
			return DesiredSize;
		}

		public double TreeHeight
		{
			get
			{
				return NodeSize().Height;
			}
		}

		public double TreeWidth
		{
			get
			{
				return NodeSize().Width;
			}
		}
	}
}
