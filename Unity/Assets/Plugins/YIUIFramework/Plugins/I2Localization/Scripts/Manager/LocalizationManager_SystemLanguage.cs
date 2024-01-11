using UnityEngine;

namespace I2.Loc
{
    public static partial class LocalizationManager
    {
        static string mCurrentDeviceLanguage;

        public static string GetCurrentDeviceLanguage( bool force = false )
        {
            if (force || string.IsNullOrEmpty(mCurrentDeviceLanguage))
                DetectDeviceLanguage();

            return mCurrentDeviceLanguage;
        }

        static void DetectDeviceLanguage()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            try { 
                        AndroidJavaObject locale = new AndroidJavaClass("java/util/Locale").CallStatic<AndroidJavaObject>("getDefault");
                        mCurrentDeviceLanguage = locale.Call<string>("toString");
                        //https://stackoverflow.com/questions/4212320/get-the-current-language-in-device


                        if (!string.IsNullOrEmpty(mCurrentDeviceLanguage))
                        {
                            mCurrentDeviceLanguage = mCurrentDeviceLanguage.Replace('_', '-');
                            mCurrentDeviceLanguage = GoogleLanguages.GetLanguageName(mCurrentDeviceLanguage, true, true);
                            if (!string.IsNullOrEmpty(mCurrentDeviceLanguage))
                                return;
                        }
            }
            catch (System.Exception)
            { 
            }
            #endif

            mCurrentDeviceLanguage = Application.systemLanguage.ToString();
            if (mCurrentDeviceLanguage == "ChineseSimplified") mCurrentDeviceLanguage = "Chinese (Simplified)";
            if (mCurrentDeviceLanguage == "ChineseTraditional") mCurrentDeviceLanguage = "Chinese (Traditional)";
        }
    }
}