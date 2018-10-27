/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UI_CheckUpdate
{
	public partial class UI_Window : GComponent
	{
		public GImage p_n11;
		public GTextField p_title;
		public GImage p_n9;
		public GImage p_character;
		public GTextField p_helptitle;
		public GTextField p_progrress;
		public GTextField p_finishcount;
		public GTextField p_downloading;
		public GTextField p_failcount;
		public UI_EnterButton p_enterbutton;

		public const string URL = "ui://lb36ex7oav6g5";

		public static UI_Window CreateInstance()
		{
			return (UI_Window)UIPackage.CreateObject("UI_CheckUpdate","Window");
		}

		public UI_Window()
		{
		}

		public override void ConstructFromXML(XML xml)
		{
			base.ConstructFromXML(xml);

			p_n11 = (GImage)this.GetChildAt(0);
			p_title = (GTextField)this.GetChildAt(1);
			p_n9 = (GImage)this.GetChildAt(2);
			p_character = (GImage)this.GetChildAt(3);
			p_helptitle = (GTextField)this.GetChildAt(4);
			p_progrress = (GTextField)this.GetChildAt(5);
			p_finishcount = (GTextField)this.GetChildAt(6);
			p_downloading = (GTextField)this.GetChildAt(7);
			p_failcount = (GTextField)this.GetChildAt(8);
			p_enterbutton = (UI_EnterButton)this.GetChildAt(9);
		}
	}
}