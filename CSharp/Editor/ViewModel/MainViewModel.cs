using System;
using System.Windows.Threading;
using Editor.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;

namespace Editor.ViewModel
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
			Action showLoginResult = () =>
				{
					LoginResult = "Login OK!";
				};
			AsyncCallback callback = (obj) =>
				{
					DispatcherHelper.UIDispatcher.BeginInvoke(
						DispatcherPriority.Normal, showLoginResult);
				};

			Action asynLogin = () =>
				{
					
				};
			asynLogin.BeginInvoke(callback, null);
		}

		public override void Cleanup()
		{
		    base.Cleanup();
		}
	}
}