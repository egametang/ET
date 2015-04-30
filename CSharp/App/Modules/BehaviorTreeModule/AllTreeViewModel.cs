using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using Common.Helper;

namespace Modules.BehaviorTreeModule
{
	[Export(typeof(AllTreeViewModel)), PartCreationPolicy(CreationPolicy.NonShared)]
	public class AllTreeViewModel
	{
		public int MaxNodeId { get; set; }
		public int MaxTreeId { get; set; }

		private readonly Dictionary<int, TreeViewModel> treeViewModelsDict =
				new Dictionary<int, TreeViewModel>();

		public readonly ObservableCollection<TreeNodeViewModel> rootList =
				new ObservableCollection<TreeNodeViewModel>();

		public ObservableCollection<TreeNodeViewModel> RootList
		{
			get
			{
				return this.rootList;
			}
		}

		public void Open(string file)
		{
			this.rootList.Clear();
			this.treeViewModelsDict.Clear();

			var treeDict = new Dictionary<int, List<TreeNodeData>>();

			AllTreeData allTreeData = new AllTreeData();
			string content = File.ReadAllText(file);
			foreach (string line in content.Split(new[] { "\r\n" }, StringSplitOptions.None))
			{
				if (line.Trim() == "")
				{
					continue;
				}
				TreeNodeData treeNodeData = MongoHelper.FromJson<TreeNodeData>(line);
				allTreeData.TreeNodeDatas.Add(treeNodeData);
			}

			this.MaxNodeId = 0;
			this.MaxTreeId = 0;
			foreach (TreeNodeData treeNodeData in allTreeData.TreeNodeDatas)
			{
				List<TreeNodeData> tree;
				treeDict.TryGetValue(treeNodeData.TreeId, out tree);
				if (tree == null)
				{
					tree = new List<TreeNodeData>();
					treeDict[treeNodeData.TreeId] = tree;
				}
				tree.Add(treeNodeData);
				if (treeNodeData.Id > this.MaxNodeId)
				{
					this.MaxNodeId = treeNodeData.Id;
				}
				if (treeNodeData.TreeId > this.MaxTreeId)
				{
					this.MaxTreeId = treeNodeData.TreeId;
				}
			}

			foreach (KeyValuePair<int, List<TreeNodeData>> pair in treeDict)
			{
				TreeViewModel treeViewModel = new TreeViewModel(this, pair.Value);
				this.treeViewModelsDict[pair.Key] = treeViewModel;
				this.RootList.Add(treeViewModel.Root);
			}
		}

		public void Save(string file)
		{
			AllTreeData allTreeData = new AllTreeData();
			foreach (TreeViewModel value in this.treeViewModelsDict.Values)
			{
				List<TreeNodeData> list = value.GetDatas();
				allTreeData.TreeNodeDatas.AddRange(list);
			}

			using (StreamWriter stream = new StreamWriter(new FileStream(file, FileMode.Create, FileAccess.Write)))
			{
				foreach (TreeNodeData treeNodeData in allTreeData.TreeNodeDatas)
				{
					string content = MongoHelper.ToJson(treeNodeData);
					stream.Write(content);
					stream.Write("\r\n");
				}
			}
		}

		public TreeViewModel New()
		{
			TreeViewModel treeViewModel = new TreeViewModel(this);
			this.treeViewModelsDict[treeViewModel.TreeId] = treeViewModel;
			this.rootList.Add(treeViewModel.Root);
			return treeViewModel;
		}

		public void Remove(int treeId)
		{
			TreeViewModel treeViewModel = this.treeViewModelsDict[treeId];
			this.treeViewModelsDict.Remove(treeId);
			this.rootList.Remove(treeViewModel.Root);
		}

		public TreeViewModel Clone(TreeNodeViewModel treeNodeViewModel)
		{
			TreeViewModel treeViewModel = (TreeViewModel)treeNodeViewModel.TreeViewModel.Clone();
			this.treeViewModelsDict[treeViewModel.TreeId] = treeViewModel;
			this.rootList.Add(treeViewModel.Root);
			return treeViewModel;
		}

		public TreeViewModel Get(int treeId)
		{
			return this.treeViewModelsDict[treeId];
		}
	}
}