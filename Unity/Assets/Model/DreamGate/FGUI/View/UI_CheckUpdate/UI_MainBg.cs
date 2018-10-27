/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UI_CheckUpdate
{
	public partial class UI_MainBg : GComponent
	{
		public UI_GalaxyBg p_bg;
		public UI_Window p_window;

		public const string URL = "ui://lb36ex7oneu01";

		public static UI_MainBg CreateInstance()
		{
			return (UI_MainBg)UIPackage.CreateObject("UI_CheckUpdate","MainBg");
		}

		public UI_MainBg()
		{
		}

		public override void ConstructFromXML(XML xml)
		{
			base.ConstructFromXML(xml);

			p_bg = (UI_GalaxyBg)this.GetChildAt(0);
			p_window = (UI_Window)this.GetChildAt(1);
		}
	}
}