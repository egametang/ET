using FairyGUI;
using Cysharp.Threading.Tasks;

namespace ET
{
    public partial class UILoginPanel : BaseFGUIWrapper
    {
		[FGUIField]
		public GButton btnLogin;
		[FGUIField]
		public GTextInput inputAccount;
		[FGUIField]
		public GTextInput inputPassword;

        public static string PackageName = "UILoginPanel";

        public static string ResName = "UILoginPanel";

        public static string Url = "ui://brnqjgmabz2i0";
        public static async UniTask<UILoginPanel> CreateAsync(Enums.FGUILayer layer = Enums.FGUILayer.Panel, GComponent root = null, bool show = true)
        {
            if (root == null)
            {
                root = GRoot.inst;
            }
            await FGUI.Instance.AddUIPackageAsync(PackageName);
            GObject obj = UIPackage.CreateObjectFromURL(Url);
            root.AddChild(obj);
            root.Center();
            UILoginPanel wrapper = obj.displayObject.gameObject.GetOrAddComponent<UILoginPanel>();
            wrapper.Init(obj.asCom, layer, show);
            return wrapper;
        }
    }
}
