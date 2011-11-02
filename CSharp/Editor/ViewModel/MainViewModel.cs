using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Egametang
{
	public class MainViewModel : ViewModelBase
	{
		private Main main = new Main();
		private RelayCommand loginCmd = null;
		private LoginRealm loginRealm = null;

		public MainViewModel()
		{
			loginCmd = new RelayCommand(Login);
			loginRealm = new LoginRealm(this);
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

		private void Login()
		{
			loginRealm.Login();
		}

		public override void Cleanup()
		{
		    base.Cleanup();
		}
	}
}