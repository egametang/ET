using Common.Logger;
using UNetTest;

namespace Profile
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			UServiceTest test = new UServiceTest();
			Log.Debug("Profile start");
			test.ClientSendToServer();
			Log.Debug("Profile stop");
		}
	}
}