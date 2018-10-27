
using System;
using FairyGUI;

namespace UI_Login
{
	public class UI_LoginILBinder
	{
		public static void BindAll()
		{
			UIObjectFactory.SetPackageItemExtension(Login_MainBg.URL, () => (GComponent)Activator.CreateInstance(typeof(Login_MainBg)));
		}
	}
}