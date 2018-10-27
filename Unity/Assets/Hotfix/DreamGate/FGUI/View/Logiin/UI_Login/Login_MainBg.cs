/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UI_Login
{
	public partial class Login_MainBg : GComponent
	{
		public GImage galaxy;
		public GTextField clientversionlabel;
		public GTextField serverversionlabel;
		public GTextField tipslabel;
		public GImage n7;
		public GImage n4;
		public GImage n5;
		public GImage n6;
		public Transition t0;

		public const string URL = "ui://myg1yo3ffhnj4";

		public static Login_MainBg CreateInstance()
		{
			return (Login_MainBg)UIPackage.CreateObject("UI_Login","Login_MainBg");
		}

		public Login_MainBg()
		{
		}

		public override void ConstructFromXML(XML xml)
		{
			base.ConstructFromXML(xml);

			galaxy = (GImage)this.GetChildAt(0);
			clientversionlabel = (GTextField)this.GetChildAt(1);
			serverversionlabel = (GTextField)this.GetChildAt(2);
			tipslabel = (GTextField)this.GetChildAt(3);
			n7 = (GImage)this.GetChildAt(4);
			n4 = (GImage)this.GetChildAt(5);
			n5 = (GImage)this.GetChildAt(6);
			n6 = (GImage)this.GetChildAt(7);
			t0 = this.GetTransitionAt(0);
		}
	}
}