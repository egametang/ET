/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UI_CheckUpdate
{
	public partial class UI_GalaxyBg : GComponent
	{
		public GImage p_bg;
		public GTextField p_clientversionlabel;
		public GTextField p_serverversionlabel;
		public GTextField p_tipslabel;
		public GImage p_n4;
		public GImage p_n5;
		public GImage p_n6;
		public Transition p_t0;

		public const string URL = "ui://lb36ex7olvy42";

		public static UI_GalaxyBg CreateInstance()
		{
			return (UI_GalaxyBg)UIPackage.CreateObject("UI_CheckUpdate","GalaxyBg");
		}

		public UI_GalaxyBg()
		{
		}

		public override void ConstructFromXML(XML xml)
		{
			base.ConstructFromXML(xml);

			p_bg = (GImage)this.GetChildAt(0);
			p_clientversionlabel = (GTextField)this.GetChildAt(1);
			p_serverversionlabel = (GTextField)this.GetChildAt(2);
			p_tipslabel = (GTextField)this.GetChildAt(3);
			p_n4 = (GImage)this.GetChildAt(4);
			p_n5 = (GImage)this.GetChildAt(5);
			p_n6 = (GImage)this.GetChildAt(6);
			p_t0 = this.GetTransitionAt(0);
		}
	}
}