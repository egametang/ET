
using UnityEngine;
using UnityEngine.UI;
namespace ET
{
	[ComponentOf(typeof(UIBaseWindow))]
	[EnableMethod]
	public  class DlgHelpViewComponent : Entity,IAwake,IDestroy 
	{
		public UnityEngine.UI.Text E_TextText
     	{
     		get
     		{
     			if (this.uiTransform == null)
     			{
     				Log.Error("uiTransform is null.");
     				return null;
     			}
     			if( this.m_E_TextText == null )
     			{
		    		this.m_E_TextText = UIFindHelper.FindDeepChild<UnityEngine.UI.Text>(this.uiTransform.gameObject,"E_Text");
     			}
     			return this.m_E_TextText;
     		}
     	}

		public void DestroyWidget()
		{
			this.m_E_TextText = null;
			this.uiTransform = null;
		}

		private UnityEngine.UI.Text m_E_TextText = null;
		public Transform uiTransform = null;
	}
}
