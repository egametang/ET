using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.Prism.Commands;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Security.Cryptography;
using System;
using NLog;

namespace Module.Login
{
	enum RealmLoginState : byte
	{
		RealmAuthLoginChallenge = 0,
		RealmAuthLonginProof = 1,
		AuthReconnectChallenge = 2,
		AuthReconnectProof = 3,
		AuthLoginPermit = 4,
		RealmList = 10,
		Surver = 48,
	}

	[Export(typeof(LoginViewModel))]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	public class LoginViewModel : NotificationObject
	{
		private string username = "";
		private string password = "";
		private string logInfo = "";

		public LoginViewModel()
		{
		}

		public string Username
		{
			get
			{
				return username;
			}
			set
			{
				if (username == value)
				{
					return;
				}
				username = value;
				RaisePropertyChanged("Username");
			}
		}

		public string Password
		{
			get
			{
				return password;
			}
			set
			{
				if (password == value)
				{
					return;
				}
				password = value;
				RaisePropertyChanged("Password");
			}
		}

		public string LogInfo
		{
			get
			{
				return logInfo;
			}
			set
			{
				if (logInfo == value)
				{
					return;
				}
				logInfo = value;
				RaisePropertyChanged("LogInfo");
			}
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

			string username = Username + new string(' ', 128 - Username.Length);
			byte[] usernameBuffer = System.Text.Encoding.Default.GetBytes(username);
			await stream.WriteAsync(usernameBuffer, 0, usernameBuffer.Length);

			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] password = System.Text.Encoding.Default.GetBytes(Password);
			byte[] passMD5Buffer = md5.ComputeHash(password);

			string passMD5 = BitConverter.ToString(passMD5Buffer);
			passMD5 = passMD5.Replace("-", "");
			passMD5 = passMD5.ToLower();

			passMD5Buffer = System.Text.Encoding.Default.GetBytes(passMD5);
			await stream.WriteAsync(passMD5Buffer, 0, passMD5Buffer.Length);

			LogInfo += "username: " + username.Trim() + " password md5: " + passMD5 + Environment.NewLine;
			Logger logger = LogManager.GetCurrentClassLogger();
			logger.Debug("11111111111");
		}
	}
}
