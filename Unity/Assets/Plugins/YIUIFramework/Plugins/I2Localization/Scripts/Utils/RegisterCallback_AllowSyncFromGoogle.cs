using UnityEngine;

namespace I2.Loc
{
    public class RegisterCallback_AllowSyncFromGoogle : MonoBehaviour
    {
        public void Awake()
        {
            LocalizationManager.Callback_AllowSyncFromGoogle = AllowSyncFromGoogle;
        }

        public void OnEnable()
        {
            LocalizationManager.Callback_AllowSyncFromGoogle = AllowSyncFromGoogle;
        }
        
        public void OnDisable()
        {
            LocalizationManager.Callback_AllowSyncFromGoogle = null;
        }

        public virtual bool AllowSyncFromGoogle(LanguageSourceData Source)
        {
            return true;
        }
    }
}