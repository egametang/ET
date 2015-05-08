using System;
using System.Collections.ObjectModel;
using System.IO;
using Common.Helper;

namespace Modules.BehaviorTreeModule
{
	public class AllTreeViewModel
	{
        private static readonly AllTreeViewModel instance = new AllTreeViewModel();
	    public static AllTreeViewModel Instance
	    {
	        get
	        {
	            return instance;
	        }
	    }
		public int MaxTreeId { get; set; }

		private readonly ObservableCollection<TreeViewModel> treeViewModels =
				new ObservableCollection<TreeViewModel>();

		public ObservableCollection<TreeViewModel> TreeViewModels
		{
			get
			{
				return this.treeViewModels;
			}
		}

		public void Open(string file)
		{
			this.treeViewModels.Clear();
			string content = File.ReadAllText(file);
			foreach (string line in content.Split(new[] { "\r\n" }, StringSplitOptions.None))
			{
				if (line.Trim() == "")
				{
					continue;
				}
				TreeViewModel treeViewModel = MongoHelper.FromJson<TreeViewModel>(line);
				this.treeViewModels.Add(treeViewModel);
				TreeLayout layout = new TreeLayout(treeViewModel);
				layout.ExcuteLayout();
				if (treeViewModel.Id > this.MaxTreeId)
				{
					this.MaxTreeId = treeViewModel.Id;
				}
			}
		}

		public void Save(string file)
		{
			using (StreamWriter stream = new StreamWriter(new FileStream(file, FileMode.Create, FileAccess.Write)))
			{
				foreach (TreeViewModel value in this.treeViewModels)
				{
					string content = MongoHelper.ToJson(value);
					stream.Write(content);
					stream.Write("\r\n");
				}
			}
		}

		public TreeViewModel New()
		{
			TreeViewModel treeViewModel = new TreeViewModel();
			treeViewModel.Id = ++this.MaxTreeId;
			this.treeViewModels.Add(treeViewModel);
			return treeViewModel;
		}

		public void Remove(TreeViewModel treeViewModel)
		{
			this.treeViewModels.Remove(treeViewModel);
		}

		public TreeViewModel Clone(TreeViewModel treeViewModel)
		{
			TreeViewModel newTreeViewModel = (TreeViewModel)treeViewModel.Clone();
			newTreeViewModel.Id = ++this.MaxTreeId;
			this.treeViewModels.Add(newTreeViewModel);
			return newTreeViewModel;
		}
	}
}