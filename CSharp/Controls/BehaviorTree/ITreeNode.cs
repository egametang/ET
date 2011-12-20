using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviorTree
{
	public interface ITreeNode
	{
		// PrivateNodeInfo is a cookie used by GraphLayout to keep track of information on
		// a per node basis.  The ITreeNode implementer just has to provide a way to
		// save and retrieve this cookie.
		Object PrivateNodeInfo { get; set; }
		TreeNodeGroup TreeChildren { get; }
		double TreeWidth { get; }
		double TreeHeight { get; }
		bool Collapsed { get; }
	}
}
