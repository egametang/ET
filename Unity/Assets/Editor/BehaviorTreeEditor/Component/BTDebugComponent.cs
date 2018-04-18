using System.Collections.Generic;
using ETModel;

namespace ETEditor
{
	public class BTDebugComponent: Component
	{
		public long OwnerId;
		public bool IsFrameSelected = false;
		public List<List<long>> TreePathList = new List<List<long>>();

		public Dictionary<long, List<List<long>>> DictPathList = new Dictionary<long, List<List<long>>>();

		public void Add(long id, List<long> list)
		{
			this.TreePathList.Add(list);
			if (id != 0)
			{
				List<List<long>> lst;
				if (!this.DictPathList.TryGetValue(id, out lst))
				{
					lst = new List<List<long>>();
					this.DictPathList.Add(id, lst);
				}
				lst.Add(list);
			}
		}

		public List<List<long>> Get(long id)
		{
			if (id == 0)
			{
				return this.TreePathList;
			}

			return this.DictPathList[id];
		}

		public void Clear()
		{
			this.TreePathList.Clear();
			this.DictPathList.Clear();
		}
	}
}
