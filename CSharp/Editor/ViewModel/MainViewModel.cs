using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Egametang
{
	public class MainViewModel : ViewModelBase
	{
		private Main main = new Main();
		private RelayCommand loginCmd = null;
		private RealmLogin loginRealm = null;

		public MainViewModel()
		{
			loginCmd = new RelayCommand(Login);
			loginRealm = new RealmLogin(this);
		}

		public string LoginInfo
		{
			get
			{
				return main.LoginInfo;
			}
			set
			{
				if (main.LoginInfo == value)
				{
					return;
				}
				main.LoginInfo = value;
				RaisePropertyChanged("LoginInfo");
			}
		}

		public string Username
		{
			get
			{
				return main.Username;
			}
			set
			{
				if (main.Username == value)
				{
					return;
				}
				main.Username = value;
				RaisePropertyChanged("Username");
			}
		}

		public string Password
		{
			get
			{
				return main.Password;
			}
			set
			{
				if (main.Password == value)
				{
					return;
				}
				main.Password = value;
				RaisePropertyChanged("Password");
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