using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;

namespace Egametang
{
	public class MainViewModel : ViewModelBase
	{
		private readonly IDataService dataService;
		private string loginResult = "";

		public MainViewModel(IDataService dataService)
		{
			this.dataService = dataService;
			LoginCmd = new RelayCommand(AsyncLogin);
		}

		public string LoginResult
		{
			get
			{
				return loginResult;
			}
			set
			{
				if (loginResult == value)
				{
					return;
				}
				loginResult = value;
				RaisePropertyChanged("LoginResult");
			}
		}

		public RelayCommand LoginCmd
		{
			get;
			private set;
		}

		private void AsyncLogin()
		{
			var task = new Task(() =>
			{

			});
			task.ContinueWith(_ =>
			{
				DispatcherHelper.UIDispatcher.BeginInvoke(DispatcherPriority.Normal,
					new Action(() =>
					{
						LoginResult = "Login OK!";
					}));
			}, App.OrderedTaskScheduler);
			task.Start();
		}

		public override void Cleanup()
		{
		    base.Cleanup();
		}
	}
}