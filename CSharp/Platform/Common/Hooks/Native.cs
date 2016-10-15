using System;
using System.Runtime.InteropServices;

namespace Common.Hooks
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true
			)]
	public delegate int DRecv(IntPtr handle, IntPtr buf, int count, int flag);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true
			)]
	public delegate int DSend(IntPtr handle, IntPtr buf, int count, int flag);

	public static class Native
	{
		[DllImport("Ws2_32.dll", EntryPoint = "recv")]
		public static extern int Recv(IntPtr handle, IntPtr buf, int count, int flag);

		[DllImport("Ws2_32.dll", EntryPoint = "send")]
		public static extern int Send(IntPtr handle, IntPtr buf, int count, int flag);
	}
}