using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Egametang
{
	public class BehaviorNodeViewModel : TreeViewItemViewModel
	{
		private readonly BehaviorNode node;

		public BehaviorNodeViewModel(BehaviorNode node, BehaviorNodeViewModel parent) :
			base(parent, false)
		{
			this.node = node;
		}

		public string Name
		{
			get
			{
				return node.Name;
			}
		}

		public virtual void LoadChildren()
		{
			foreach (var child in node.Children)
			{
				base.Children.Add(new BehaviorNodeViewModel(child, this));
			}
		}
	}
}
