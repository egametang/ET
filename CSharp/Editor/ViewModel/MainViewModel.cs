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
		private Main main = new Main();
		private RelayCommand loginCmd = null;

		public MainViewModel()
		{
			loginCmd = new RelayCommand(AsyncLogin);
		}

		public string LoginResult
		{
			get
			{
				return main.LoginResult;
			}
			set
			{
				if (main.LoginResult == value)
				{
					return;
				}
				main.LoginResult = value;
				RaisePropertyChanged("LoginResult");
			}
		}

		public RelayCommand LoginCmd
		{
			get
			{
				return loginCmd;
			}
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
			});
			task.Start(App.OrderedTaskScheduler);
		}

		public override void Cleanup()
		{
		    base.Cleanup();
		}
	}
}