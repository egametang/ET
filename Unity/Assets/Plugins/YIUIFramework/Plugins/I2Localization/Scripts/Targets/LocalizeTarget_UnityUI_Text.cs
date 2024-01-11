using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace I2.Loc
{
    #if UNITY_EDITOR
    [InitializeOnLoad] 
    #endif

    public class LocalizeTarget_UnityUI_Text : LocalizeTarget<Text>
	{
        static LocalizeTarget_UnityUI_Text() { AutoRegister(); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void AutoRegister() { LocalizationManager.RegisterTarget(new LocalizeTargetDesc_Type<Text, LocalizeTarget_UnityUI_Text> { Name = "Text", Priority = 100 }); }

        TextAnchor mAlignment_RTL = TextAnchor.UpperRight;
		TextAnchor mAlignment_LTR = TextAnchor.UpperLeft;
		bool mAlignmentWasRTL;
		bool mInitializeAlignment = true;

        public override eTermType GetPrimaryTermType(Localize cmp) { return eTermType.Text; }
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.Font; }
        public override bool CanUseSecondaryTerm ()		{ return true;   }
		public override bool AllowMainTermToBeRTL ()	{ return true;   }
		public override bool AllowSecondTermToBeRTL ()	{ return false;  }

		public override void GetFinalTerms ( Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm )
		{
            primaryTerm = mTarget ? mTarget.text : null;
            secondaryTerm = mTarget.font!=null ? mTarget.font.name : string.Empty;
		}


		public override void DoLocalize ( Localize cmp, string mainTranslation, string secondaryTranslation )
		{
            //--[ Localize Font Object ]----------
            Font newFont = cmp.GetSecondaryTranslatedObj<Font>( ref mainTranslation, ref secondaryTranslation );
			if (newFont!=null && newFont!=mTarget.font)
				mTarget.font = newFont;

			if (mInitializeAlignment)
			{
				mInitializeAlignment = false;
				mAlignmentWasRTL = LocalizationManager.IsRight2Left;
				InitAlignment( mAlignmentWasRTL, mTarget.alignment, out mAlignment_LTR, out mAlignment_RTL );
			}
			else
			{
				TextAnchor alignRTL, alignLTR;
				InitAlignment( mAlignmentWasRTL, mTarget.alignment, out alignLTR, out alignRTL );

				if (mAlignmentWasRTL && mAlignment_RTL!=alignRTL ||
					!mAlignmentWasRTL && mAlignment_LTR != alignLTR)
				{
					mAlignment_LTR = alignLTR;
					mAlignment_RTL = alignRTL;
				}
				mAlignmentWasRTL = LocalizationManager.IsRight2Left;
			}

			if (mainTranslation!=null && mTarget.text != mainTranslation)
			{
				if (cmp.CorrectAlignmentForRTL)
				{
					mTarget.alignment = LocalizationManager.IsRight2Left ? mAlignment_RTL : mAlignment_LTR;
				}


				mTarget.text = mainTranslation;
				mTarget.SetVerticesDirty();

				// In the editor, sometimes unity "forgets" to show the changes
                #if UNITY_EDITOR
				if (!Application.isPlaying)
					EditorUtility.SetDirty( mTarget );
                #endif
			}
		}

		void InitAlignment ( bool isRTL, TextAnchor alignment, out TextAnchor alignLTR, out TextAnchor alignRTL )
		{
			alignLTR = alignRTL = alignment;

			if (isRTL)
			{
				switch (alignment)
				{
					case TextAnchor.UpperRight: alignLTR = TextAnchor.UpperLeft; break;
					case TextAnchor.MiddleRight: alignLTR = TextAnchor.MiddleLeft; break;
					case TextAnchor.LowerRight: alignLTR = TextAnchor.LowerLeft; break;
					case TextAnchor.UpperLeft: alignLTR = TextAnchor.UpperRight; break;
					case TextAnchor.MiddleLeft: alignLTR = TextAnchor.MiddleRight; break;
					case TextAnchor.LowerLeft: alignLTR = TextAnchor.LowerRight; break;
				}
			}
			else
			{
				switch (alignment)
				{
					case TextAnchor.UpperRight: alignRTL = TextAnchor.UpperLeft; break;
					case TextAnchor.MiddleRight: alignRTL = TextAnchor.MiddleLeft; break;
					case TextAnchor.LowerRight: alignRTL = TextAnchor.LowerLeft; break;
					case TextAnchor.UpperLeft: alignRTL = TextAnchor.UpperRight; break;
					case TextAnchor.MiddleLeft: alignRTL = TextAnchor.MiddleRight; break;
					case TextAnchor.LowerLeft: alignRTL = TextAnchor.LowerRight; break;
				}
			}
		}
	}
}

