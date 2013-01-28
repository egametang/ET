using System;
using System.Runtime.InteropServices;
using EasyHook;
using Log;

namespace Hooks
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, 
		SetLastError = true)]
	public delegate int DRecv(IntPtr handle, IntPtr buf, int count, int flag);

	public class RecvHook: IDisposable
	{
		[DllImport("Ws2_32.dll", EntryPoint = "recv")]
		public static extern int Recv(IntPtr handle, IntPtr buf, int count, int flag);

		private readonly LocalHook localHook;

		public RecvHook(DRecv dRecv)
		{
			try
			{
				this.localHook = LocalHook.Create(
					LocalHook.GetProcAddress("Ws2_32.dll", "recv"), new DRecv(dRecv), this);
				this.localHook.ThreadACL.SetInclusiveACL(new[] { 0 });
			}
			catch (Exception)
			{
				Logger.Debug("Error creating recv Hook");
				throw;
			}
		}

		public void Dispose()
		{
			this.localHook.Dispose();
		}

		//static int RecvHooked(IntPtr socketHandle, IntPtr buf, int count, int socketFlags)
		//{
		//	int bytesCount = recv(socketHandle, buf, count, socketFlags);
		//	if (bytesCount > 0)
		//	{
		//		var newBuffer = new byte[bytesCount];
		//		Marshal.Copy(buf, newBuffer, 0, bytesCount);
		//		string s = Encoding.ASCII.GetString(newBuffer);
		//		TextWriter tw = new StreamWriter("recv.txt");
		//		tw.Write(s);
		//		tw.Close();
		//		Logger.Debug(string.Format("Hooked:>{0}", s));
		//	}
		//	return bytesCount;
		//}
	}
}