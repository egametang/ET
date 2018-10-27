/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UI_CheckUpdate
{
	public partial class UI_EnterButton : GButton
	{
		public Controller p_button;
		public GImage p_n0;
		public GTextField p_buttonlabel;

		public const string URL = "ui://lb36ex7oav6ga";

		public static UI_EnterButton CreateInstance()
		{
			return (UI_EnterButton)UIPackage.CreateObject("UI_CheckUpdate","EnterButton");
		}

		public UI_EnterButton()
		{
		}

		public override void ConstructFromXML(XML xml)
		{
			base.ConstructFromXML(xml);

			p_button = this.GetControllerAt(0);
			p_n0 = (GImage)this.GetChildAt(0);
			p_buttonlabel = (GTextField)this.GetChildAt(1);
		}
	}
}