using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Interop
{
	internal enum HookType //枚举，钩子的类型
	{

		// MSGFILTER        = -1,
		// JOURNALRECORD    = 0,
		// JOURNALPLAYBACK  = 1,
		KEYBOARD            = 2,
		GETMESSAGE          = 3,
		CALLWNDPROC         = 4,
		// CBT              = 5,
		SYSMSGFILTER        = 6,
		//MOUSE             = 7,
		HARDWARE            = 8,
		// DEBUG            = 9,
		// SHELL            = 10,
		// FOREGROUNDIDLE   = 11,
		CALLWNDPROCRET      = 12,
		KEYBOARDLL          = 13,
		// MOUSELL          = 14,

	};


	public enum WMsG
	{
		WM_NULL            = 0x0000,
		WM_CREATE          = 0x0001,
		WM_DESTROY         = 0x0002,
		WM_MOVE            = 0x0003,
		WM_SIZE            = 0x0005,
		WM_ACTIVATE        = 0x0006,
		WA_INACTIVE        = 0,
		WA_ACTIVE          = 1,
		WA_CLICKACTIVE     = 2,

		WM_SETFOCUS        = 0x0007,
		WM_KILLFOCUS       = 0x0008,
		WM_ENABLE          = 0x000A,
		WM_SETREDRAW       = 0x000B,
		WM_SETTEXT         = 0x000C,
		WM_GETTEXT         = 0x000D,
		WM_GETTEXTLENGTH   = 0x000E,
		WM_PAINT           = 0x000F,
		WM_CLOSE           = 0x0010,

		WM_QUERYENDSESSION = 0x0011,
		WM_QUERYOPEN       = 0x0013,
		WM_ENDSESSION      = 0x0016,
		WM_QUIT            = 0x0012,
		WM_ERASEBKGND      = 0x0014,
		WM_SYSCOLORCHANGE  = 0x0015,
		WM_SHOWWINDOW      = 0x0018,
		WM_WININICHANGE    = 0x001A,
		WM_DEVMODECHANGE   = 0x001B,
		WM_ACTIVATEAPP     = 0x001C,
		WM_FONTCHANGE      = 0x001D,
		WM_TIMECHANGE      = 0x001E,
		WM_CANCELMODE      = 0x001F,
		WM_SETCURSOR       = 0x0020,
		WM_MOUSEACTIVATE   = 0x0021,
		WM_CHILDACTIVATE   = 0x0022,
		WM_QUEUESYNC       = 0x0023,
		WM_GETMINMAXINFO   = 0x0024,

		WM_KEYFIRST        = 0x0100,
		WM_KEYDOWN         = 0x0100,
		WM_KEYUP           = 0x0101,
		WM_CHAR            = 0x0102,
		WM_DEADCHAR        = 0x0103,
		WM_SYSKEYDOWN      = 0x0104,
		WM_SYSKEYUP        = 0x0105,
		WM_SYSCHAR         = 0x0106,
		WM_SYSDEADCHAR     = 0x0107,

		WM_MOUSEFIRST      = 0x0200,
		WM_MOUSEMOVE       = 0x0200,
		// 移动鼠标
		WM_LBUTTONDOWN     = 0x0201,
		//按下鼠标左键
		WM_LBUTTONUP       = 0x0202,
		//释放鼠标左键
		WM_LBUTTONDBLCLK   = 0x0203,
		//双击鼠标左键
		WM_RBUTTONDOWN     = 0x0204,
		//按下鼠标右键
		WM_RBUTTONUP       = 0x0205,
		//释放鼠标右键
		WM_RBUTTONDBLCLK   = 0x0206,
		//双击鼠标右键
		WM_MBUTTONDOWN     = 0x0207,
		//按下鼠标中键 
		WM_MBUTTONUP       = 0x0208,
		//释放鼠标中键
		WM_MBUTTONDBLCLK   = 0x0209,
		//双击鼠标中键
		WM_MOUSEWHEEL      = 0x020A,
	}

	/// <summary>
	/// 鼠标动作枚举
	/// </summary>
	public enum MouseEventFlag : uint
	{
		MOVE        = 0X0001,
		LEFTDOWN    = 0X0002,
		LEFTUP      = 0X0004,
		RIGHTDOWN   = 0X0008,
		RIGHTUP     = 0X0010,
		MIDDLEDOWN  = 0X0020,
		MIDDLEUP    = 0X0040,
		XDOWN       = 0X0080,
		XUP         = 0X0100,
		WHEEL       = 0X0800,
		VIRTUALDESK = 0X4000,
		ABSOLUTE    = 0X8000
	}
	/// <summary>
	/// 键盘动作枚举
	/// </summary>
	public enum VirtualKeys : byte
	{
		// VK_NUMLOCK    = 0x90, // 数字锁定键
		// VK_SCROLL     = 0x91, // 滚动锁定
		// VK_CAPITAL    = 0x14, // 大小写锁定
		// VK_A          = 62,   // 键盘A
		VK_LBUTTON     = 1,    // 鼠标左键 
		VK_RBUTTON     = 2,    // 鼠标右键 
		VK_CANCEL      = 3,    // Ctrl+Break(通常不需要处理) 
		VK_MBUTTON     = 4,    // 鼠标中键 
		VK_BACK        = 8,    // Backspace 
		VK_TAB         = 9,    // Tab 
		VK_CLEAR       = 12,   // Num Lock关闭时的数字键盘5 
		VK_RETURN      = 13,   // Enter(或者另一个) 
		VK_SHIFT       = 16,   // Shift(或者另一个) 
		VK_CONTROL     = 17,   // Ctrl(或者另一个） 
		VK_MENU        = 18,   // Alt(或者另一个) 
		VK_PAUSE       = 19,   // Pause 
		VK_CAPITAL     = 20,   // Caps Lock 
		VK_ESCAPE      = 27,   // Esc 
		VK_SPACE       = 32,   // Spacebar 
		VK_PRIOR       = 33,   // Page Up 
		VK_NEXT        = 34,   // Page Down 
		VK_END         = 35,   // End 
		VK_HOME        = 36,   // Home 
		VK_LEFT        = 37,   // 左箭头 
		VK_UP          = 38,   // 上箭头 
		VK_RIGHT       = 39,   // 右箭头 
		VK_DOWN        = 40,   // 下箭头 
		VK_SELECT      = 41,   // 可选 
		VK_PRINT       = 42,   // 可选 
		VK_EXECUTE     = 43,   // 可选 
		VK_SNAPSHOT    = 44,   // Print Screen 
		VK_INSERT      = 45,   // Insert 
		VK_DELETE      = 46,   // Delete 
		VK_HELP        = 47,   // 可选 
		VK_NUM0        = 48,   // 0
		VK_NUM1        = 49,   // 1
		VK_NUM2        = 50,   // 2
		VK_NUM3        = 51,   // 3
		VK_NUM4        = 52,   // 4
		VK_NUM5        = 53,   // 5
		VK_NUM6        = 54,   // 6
		VK_NUM7        = 55,   // 7
		VK_NUM8        = 56,   // 8
		VK_NUM9        = 57,   // 9
		VK_A           = 65,   // A
		VK_B           = 66,   // B
		VK_C           = 67,   // C
		VK_D           = 68,   // D
		VK_E           = 69,   // E
		VK_F           = 70,   // F
		VK_G           = 71,   // G
		VK_H           = 72,   // H
		VK_I           = 73,   // I
		VK_J           = 74,   // J
		VK_K           = 75,   // K
		VK_L           = 76,   // L
		VK_M           = 77,   // M
		VK_N           = 78,   // N
		VK_O           = 79,   // O
		VK_P           = 80,   // P
		VK_Q           = 81,   // Q
		VK_R           = 82,   // R
		VK_S           = 83,   // S
		VK_T           = 84,   // T
		VK_U           = 85,   // U
		VK_V           = 86,   // V
		VK_W           = 87,   // W
		VK_X           = 88,   // X
		VK_Y           = 89,   // Y
		VK_Z           = 90,   // Z
		VK_NUMPAD0     = 96,   // 0
		VK_NUMPAD1     = 97,   // 1
		VK_NUMPAD2     = 98,   // 2
		VK_NUMPAD3     = 99,   // 3
		VK_NUMPAD4     = 100,  // 4
		VK_NUMPAD5     = 101,  // 5
		VK_NUMPAD6     = 102,  // 6
		VK_NUMPAD7     = 103,  // 7
		VK_NUMPAD8     = 104,  // 8
		VK_NUMPAD9     = 105,  // 9
		VK_NULTIPLY    = 106,  // 数字键盘上的* 
		VK_ADD         = 107,  // 数字键盘上的+ 
		VK_SEPARATOR   = 108,  // 可选 
		VK_SUBTRACT    = 109,  // 数字键盘上的- 
		VK_DECIMAL     = 110,  // 数字键盘上的. 
		VK_DIVIDE      = 111,  // 数字键盘上的/
		VK_F1          = 112,
		VK_F2          = 113,
		VK_F3          = 114,
		VK_F4          = 115,
		VK_F5          = 116,
		VK_F6          = 117,
		VK_F7          = 118,
		VK_F8          = 119,
		VK_F9          = 120,
		VK_F10         = 121,
		VK_F11         = 122,
		VK_F12         = 123,
		VK_NUMLOCK     = 144,  // Num Lock 
		VK_SCROLL      = 145   // Scroll Lock 
	}

	public enum NCmdShow : uint
	{
		SW_FORCEMINIMIZE      = 0x0,
		SW_HIDE               = 0x1,
		SW_MAXIMIZE           = 0x2,
		SW_MINIMIZE           = 0x3,
		SW_RESTORE            = 0x4,
		SW_SHOW               = 0x5,
		SW_SHOWDEFAULT        = 0x6,
		SW_SHOWMAXIMIZED      = 0x7,
		SW_SHOWMINIMIZED      = 0x8,
		SW_SHOWMINNOACTIVE    = 0x9,
		SW_SHOWNA             = 0xA,
		SW_SHOWNOACTIVATE     = 0xB,
		SW_SHOWNORMAL         = 0xC,
		WM_CLOSE              = 0x10,
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct KbDllHook
	{
		public int vkCode; 
		public int scanCode; 
		public int flags; 
		public int time; 
		public int dwExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Rect
	{
		public int left;
		public int right;
		public int top;
		public int button;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Point
	{
		public int x;
		public int y;
	}  

	[StructLayout(LayoutKind.Sequential)]
	public struct WindowPlacement
	{
		public int length;
		public int flags;
		public int showCmd;
		public Point ptMinPosition;
		public Point ptMaxPosition;
		public Rect rcNormalPosition;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct Luid
	{
		public uint lowPart;
		public uint highPart;
	};

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct TokenPrivileges
	{
		public uint privilegeCount;
		public Luid luid;
		public uint attributes;
	};

	public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
	public delegate int EnumWindowProc(IntPtr hWnd, IntPtr parameter, string className);

	public class Hooks
	{
		[DllImport("user32.dll", EntryPoint = "GetClassName")]
		public static extern int GetClassName(IntPtr hwnd, StringBuilder lpClassName, int nMaxCount);

		[DllImport("user32.dll")]
		public static extern int GetWindowText(int hWnd, StringBuilder lpString, int nMaxCount);

		[DllImport("user32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		//IMPORTANT : LPARAM  must be a pointer (InterPtr) in VS2005, otherwise an exception will be thrown
		private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr i);


		//the callback function for the EnumChildWindows
		//用于系统关机等权限操作
		[DllImport("user32.dll", EntryPoint = "ExitWindowsEx", CharSet = CharSet.Auto)]
		private static extern int ExitWindowsEx(int uFlags, int dwReserved);
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		private static extern int GetCurrentProcess();
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		private static extern int GetLastError();
		[DllImport("advapi32", CharSet = CharSet.Auto)]
		private static extern int OpenProcessToken(int ProcessHandle, uint DesiredAccess, ref int TokenHandle);
		[DllImport("advapi32", CharSet = CharSet.Auto)]
		private static extern int LookupPrivilegeValue(String lpSystemName, String lpName, ref  Luid lpLuid);
		[DllImport("advapi32", CharSet = CharSet.Auto)]
		private static extern int AdjustTokenPrivileges(int TokenHandle, bool DisableAllPrivileges, ref TokenPrivileges NewState, int BufferLength, int PreviousState, int ReturnLength);

		[DllImport("activ.dll", CharSet = CharSet.Auto)]
		public static extern bool ForceForegroundWindow(int hwnd);

		//主要用于更改程序标题
		//这个函数用来置顶显示,参数hwnd为窗口句柄 
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool SetWindowTextA(IntPtr hwn, IntPtr lpString);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern void SetForegroundWindow(int hwnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool BringWindowToTop(IntPtr hwnd);

		//这个函数用来显示窗口,参数hwnd为窗口句柄,nCmdShow是显示类型的枚举 
		[DllImport("user32.dll")]
		public static extern bool ShowWindow(int hWnd, NCmdShow nCmdShow);

		[DllImport("user32.dll")]
		public static extern bool SetWindowPos(int hwnd, int hWndInsertAfter, int x, int y, int cx, int cy, uint wFlags);

		//得到窗体句柄的函数,FindWindow函数用来返回符合指定的类名( ClassName )和窗口名( WindowTitle )的窗口句柄
		[DllImport("user32.dll")]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		// 查找窗口
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr FindWindowEx(IntPtr hWndFather, IntPtr hWndPreChild, string lpszClass, string lpszWindows);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern bool GetWindowPlacement(int hwnd, ref WindowPlacement lpwndpl);

		[DllImport("user32.dll")]
		private static extern int GetWindowThreadProcessId(int id, int pid);

		[DllImport("user32.dll")]
		private static extern bool GetWindowRect(int hwnd, ref Rect lpwndpl);

		[DllImport("kernel32.dll")]
		private static extern void CloseHandle(uint hObject);  //Handle to object

		// 读取进程内存的函数
		[DllImport("kernel32.dll")]
		static extern bool ReadProcessMemory(
			uint hProcess, 
			int lpBaseAddress,
			out int lpBuffer, 
			uint nSize, 
			int lpNumberOfBytesRead
		);

		[DllImport("kernel32.dll")]
		static extern bool ReadProcessMemory(
			uint hProcess, 
			int lpBaseAddress,
			char[] lpBuffer, 
			uint nSize, 
			uint lpNumberOfBytesRead
		);

		[DllImport("kernel32.dll")]
		static extern bool ReadProcessMemory(
			uint hProcess, 
			int lpBaseAddress,
			string lpBuffer, 
			uint nSize, 
			uint lpNumberOfBytesRead
		);
		[DllImport("kernel32.dll")]
		public static extern bool ReadProcessMemory(
			uint hProcess,
			int lpBaseAddress,
			byte[] lpBuffer,
			int nSize,
			uint lpNumberOfBytesRead
		);

		// 得到目标进程句柄的函数
		[DllImport("kernel32.dll")]
		public static extern uint OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);
		// 鼠标事件声明
		[DllImport("user32.dll")]
		static extern bool setcursorpos(int x, int y);
		[DllImport("user32.dll")]
		static extern void mouse_event(MouseEventFlag flags, int dx, int dy, uint data, UIntPtr extrainfo);
		// 键盘事件声明
		[DllImport("user32.dll")]
		static extern byte MapVirtualKey(byte wCode, int wMap);
		[DllImport("user32.dll")]
		static extern short GetKeyState(int nVirtKey);
		[DllImport("user32.dll")]
		static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
		// 键盘事件声明winio
		[DllImport("winio.dll")]
		public static extern bool InitializeWinIo();
		[DllImport("winio.dll")]
		public static extern bool GetPortVal(IntPtr wPortAddr, out int pdwPortVal, byte bSize);
		[DllImport("winio.dll")]
		public static extern bool SetPortVal(uint wPortAddr, IntPtr dwPortVal, byte bSize);
		[DllImport("winio.dll")]
		public static extern byte MapPhysToLin(byte pbPhysAddr, uint dwPhysSize, IntPtr PhysicalMemoryHandle);
		[DllImport("winio.dll")]
		public static extern bool UnmapPhysicalMemory(IntPtr PhysicalMemoryHandle, byte pbLinAddr);
		[DllImport("winio.dll")]
		public static extern bool GetPhysLong(IntPtr pbPhysAddr, byte pdwPhysVal);
		[DllImport("winio.dll")]
		public static extern bool SetPhysLong(IntPtr pbPhysAddr, byte dwPhysVal);
		[DllImport("winio.dll")]
		public static extern void ShutdownWinIo();

		// 全局键盘钩子
		// 第一个参数:指定钩子的类型，有WH_MOUSE、WH_KEYBOARD等十多种(具体参见MSDN)
		// 第二个参数:标识钩子函数的入口地址
		// 第三个参数:钩子函数所在模块的句柄；
		// 第四个参数:钩子相关函数的ID用以指定想让钩子去钩哪个线程，为0时则拦截整个系统的消息。
		// 安装在钩子链表中的钩子子程
		[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

		[DllImport("user32")]
		private static extern int GetKeyboardState(byte[] pbKeyState);

		// 取得模块句柄 
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);

		// 移除由SetWindowsHookEx方法安装在钩子链表中的钩子子程
		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern bool UnhookWindowsHookEx(IntPtr hhook);

		// 对一个事件处理的hook可能有多个，它们成链状，使用CallNextHookEx一级一级地调用。简单解释过来就是“调用下一个HOOK”
		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern IntPtr CallNextHookEx(IntPtr hhook, int code, IntPtr wparam, IntPtr lparam);

		// 发送系统消息
		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

		// 发送系统消息
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int SendMessage(IntPtr hWnd, int msg, byte[] wParam, int lParam);

		// 函数功能描述:将一块内存的数据从一个位置复制到另一个位置
		[DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
		public static extern void CopyMemory(ref KbDllHook Source, IntPtr Destination, int Length);

		// 函数功能描述:将一块内存的数据从一个位置复制到另一个位置
		[DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
		public static extern void CopyMemory(KbDllHook Source, IntPtr Destination, int Length);

		// 取得当前线程编号的API
		[DllImport("kernel32.dll")]
		static extern int GetCurrentThreadId();

		//********************************************************************************************
		// 获取屏幕1024*768图像
		[DllImport("gdi32.dll")]
		public static extern int BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, UInt32 dwRop);

		// 创建桌面句柄
		[DllImportAttribute("gdi32.dll")]
		public static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, int lpInitData);

		[DllImport("gdi32.dll")]
		public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		// 创建与系统匹配的图像资源
		[DllImport("gdi32.dll")]
		public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

		[DllImport("gdi32.dll")]
		public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

		// 删除用过的资源
		[DllImport("gdi32.dll")]
		public static extern int DeleteDC(IntPtr hdc);

		// 释放用过的句柄等资源
		[DllImport("user32.dll")]
		public static extern bool ReleaseDC(IntPtr hwnd, IntPtr hdc);

		// 释放用过的画笔，等图像资源
		[DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hdc);

		// 用于像素放大,最后一参数cc0020
		[DllImport("gdi32.dll")]
		public static extern bool StretchBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest, IntPtr hdcSrc, int xSrc, int ySrc, int wSrc, int hSrc, IntPtr rop);
	}
}
