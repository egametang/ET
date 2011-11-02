using System;
using System.Net.Sockets;
using GalaSoft.MvvmLight.Threading;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;

namespace Egametang
{
	enum RealmOpcode
	{
		REALM_AUTH_LOGON_CHALLENGE = 0x0000,
		REALM_AUTH_LOGON_PROOF = 0x0001,
		AUTH_RECONNECT_CHALLENGE = 0x02,
		AUTH_RECONNECT_PROOF = 0x03,
		AUTH_LOGON_PERMIT = 0x04,
		REALM_LIST = 0x10,
		SURVEY = 48,
	}

	class LoginRealm
	{
		private MainViewModel mainViewModel = null;

		public LoginRealm(MainViewModel mainViewModel)
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
			byte[] opcodeBuffer = System.BitConverter.GetBytes((Int32)RealmOpcode.AUTH_LOGON_PERMIT);
			await stream.WriteAsync(opcodeBuffer, 0, opcodeBuffer.Length);

			string username = "egametang@163.com";
			username += new string(' ', 128 - username.Length);
			byte[] usernameBuffer = System.Text.Encoding.Default.GetBytes(username);
			await stream.WriteAsync(usernameBuffer, 0, usernameBuffer.Length);

			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] password = System.Text.Encoding.Default.GetBytes("163bio1");
			byte[] passMD5Buffer = md5.ComputeHash(password);

			string passMD5 = BitConverter.ToString(passMD5Buffer);
			passMD5 = passMD5.Replace("-", "");
			passMD5 = passMD5.ToLower();
			App.Logger.Debug("passMD5: " + passMD5);

			passMD5Buffer = System.Text.Encoding.Default.GetBytes(passMD5);
			await stream.WriteAsync(passMD5Buffer, 0, passMD5Buffer.Length);

			await DispatcherHelper.UIDispatcher.InvokeAsync(new Action(() =>
			{
				mainViewModel.LoginResult = "Login Permit!";
			}));
		}
	}
}
