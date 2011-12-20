using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace BehaviorTree
{
	public struct TreeConnection
	{
		public ITreeNode IgnParent { get; private set; }
		public ITreeNode IgnChild { get; private set; }
		public List<Point> LstPt { get; private set; }

		public TreeConnection(ITreeNode ignParent, ITreeNode ignChild, List<Point> lstPt)
			: this()
		{
			IgnChild = ignChild;
			IgnParent = ignParent;
			LstPt = lstPt;
		}
	}
}
