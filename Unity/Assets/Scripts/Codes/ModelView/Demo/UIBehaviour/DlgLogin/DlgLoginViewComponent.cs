
using UnityEngine;
using UnityEngine.UI;
namespace ET
{
	[ComponentOf(typeof(UIBaseWindow))]
	[EnableMethod]
	public  class DlgLoginViewComponent : Entity,IAwake,IDestroy 
	{
		public UnityEngine.UI.InputField E_AccountInputField
     	{
     		get
     		{
     			if (this.uiTransform == null)
     			{
     				Log.Error("uiTransform is null.");
     				return null;
     			}
     			if( this.m_E_AccountInputField == null )
     			{
		    		this.m_E_AccountInputField = UIFindHelper.FindDeepChild<UnityEngine.UI.InputField>(this.uiTransform.gameObject,"Panel/E_Account");
     			}
     			return this.m_E_AccountInputField;
     		}
     	}

		public UnityEngine.UI.Image E_AccountImage
     	{
     		get
     		{
     			if (this.uiTransform == null)
     			{
     				Log.Error("uiTransform is null.");
     				return null;
     			}
     			if( this.m_E_AccountImage == null )
     			{
		    		this.m_E_AccountImage = UIFindHelper.FindDeepChild<UnityEngine.UI.Image>(this.uiTransform.gameObject,"Panel/E_Account");
     			}
     			return this.m_E_AccountImage;
     		}
     	}

		public UnityEngine.UI.InputField E_PasswordInputField
     	{
     		get
     		{
     			if (this.uiTransform == null)
     			{
     				Log.Error("uiTransform is null.");
     				return null;
     			}
     			if( this.m_E_PasswordInputField == null )
     			{
		    		this.m_E_PasswordInputField = UIFindHelper.FindDeepChild<UnityEngine.UI.InputField>(this.uiTransform.gameObject,"Panel/E_Password");
     			}
     			return this.m_E_PasswordInputField;
     		}
     	}

		public UnityEngine.UI.Image E_PasswordImage
     	{
     		get
     		{
     			if (this.uiTransform == null)
     			{
     				Log.Error("uiTransform is null.");
     				return null;
     			}
     			if( this.m_E_PasswordImage == null )
     			{
		    		this.m_E_PasswordImage = UIFindHelper.FindDeepChild<UnityEngine.UI.Image>(this.uiTransform.gameObject,"Panel/E_Password");
     			}
     			return this.m_E_PasswordImage;
     		}
     	}

		public UnityEngine.UI.Button E_LoginBtnButton
     	{
     		get
     		{
     			if (this.uiTransform == null)
     			{
     				Log.Error("uiTransform is null.");
     				return null;
     			}
     			if( this.m_E_LoginBtnButton == null )
     			{
		    		this.m_E_LoginBtnButton = UIFindHelper.FindDeepChild<UnityEngine.UI.Button>(this.uiTransform.gameObject,"Panel/E_LoginBtn");
     			}
     			return this.m_E_LoginBtnButton;
     		}
     	}

		public UnityEngine.UI.Image E_LoginBtnImage
     	{
     		get
     		{
     			if (this.uiTransform == null)
     			{
     				Log.Error("uiTransform is null.");
     				return null;
     			}
     			if( this.m_E_LoginBtnImage == null )
     			{
		    		this.m_E_LoginBtnImage = UIFindHelper.FindDeepChild<UnityEngine.UI.Image>(this.uiTransform.gameObject,"Panel/E_LoginBtn");
     			}
     			return this.m_E_LoginBtnImage;
     		}
     	}

		public void DestroyWidget()
		{
			this.m_E_AccountInputField = null;
			this.m_E_AccountImage = null;
			this.m_E_PasswordInputField = null;
			this.m_E_PasswordImage = null;
			this.m_E_LoginBtnButton = null;
			this.m_E_LoginBtnImage = null;
			this.uiTransform = null;
		}

		private UnityEngine.UI.InputField m_E_AccountInputField = null;
		private UnityEngine.UI.Image m_E_AccountImage = null;
		private UnityEngine.UI.InputField m_E_PasswordInputField = null;
		private UnityEngine.UI.Image m_E_PasswordImage = null;
		private UnityEngine.UI.Button m_E_LoginBtnButton = null;
		private UnityEngine.UI.Image m_E_LoginBtnImage = null;
		public Transform uiTransform = null;
	}
}
