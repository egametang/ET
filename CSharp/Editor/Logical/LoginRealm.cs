using System;
using System.Net.Sockets;
using GalaSoft.MvvmLight.Threading;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;

namespace Egametang
{
	enum RealmLoginState: byte
	{
		RealmAuthLoginChallenge = 0,
		RealmAuthLonginProof = 1,
		AuthReconnectChallenge = 2,
		AuthReconnectProof = 3,
		AuthLoginPermit = 4,
		RealmList = 10,
		Surver = 48,
	}

	class RealmLogin
	{
		private MainViewModel mainViewModel = null;

		public RealmLogin(MainViewModel mainViewModel)
		{
			this.mainViewModel = mainViewModel;
		}

		public async void Login()
		{
			using (TcpClient tcpClient = new TcpClient())
			{
				// 异步连接
				await tcpClient.ConnectAsync("192.168.10.246", 19000);
				var stream = tcpClient.GetStream();

				// 验证通行证
				await LoginPermit(stream);
			}
		}

		public async Task LoginPermit(NetworkStream stream)
		{
			byte[] opcodeBuffer = new byte[1] { (byte)RealmLoginState.AuthLoginPermit };
			await stream.WriteAsync(opcodeBuffer, 0, opcodeBuffer.Length);

			string username = mainViewModel.Username;
			username += new string(' ', 128 - username.Length);
			byte[] usernameBuffer = System.Text.Encoding.Default.GetBytes(username);
			await stream.WriteAsync(usernameBuffer, 0, usernameBuffer.Length);

			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] password = System.Text.Encoding.Default.GetBytes(mainViewModel.Password);
			byte[] passMD5Buffer = md5.ComputeHash(password);

			string passMD5 = BitConverter.ToString(passMD5Buffer);
			passMD5 = passMD5.Replace("-", "");
			passMD5 = passMD5.ToLower();
			App.Logger.Debug(string.Format("username: {0}, passMD5: {1}", username, passMD5));

			passMD5Buffer = System.Text.Encoding.Default.GetBytes(passMD5);
			App.Logger.Debug("passMD5Buffer len: " + passMD5Buffer.Length.ToString());
			await stream.WriteAsync(passMD5Buffer, 0, passMD5Buffer.Length);

			await DispatcherHelper.UIDispatcher.InvokeAsync(new Action(() =>
			{
				mainViewModel.LoginInfo += "Login Permit!" + Environment.NewLine;
			}));
		}
	}
}
