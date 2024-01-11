using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace I2.Loc
{
    public static class PersistentStorage
    {
        static I2CustomPersistentStorage mStorage;

        public enum eFileType { Raw, Persistent, Temporal, Streaming }

        #region PlayerPrefs
        public static void SetSetting_String(string key, string value)
        {
            if (mStorage == null) mStorage = new I2CustomPersistentStorage();
            mStorage.SetSetting_String(key, value);
        }

        public static string GetSetting_String(string key, string defaultValue)
        {
            if (mStorage == null) mStorage = new I2CustomPersistentStorage();
            return mStorage.GetSetting_String(key, defaultValue);
        }

        public static void DeleteSetting(string key)
        {
            if (mStorage == null) mStorage = new I2CustomPersistentStorage();
            mStorage.DeleteSetting(key);
        }

        public static bool HasSetting( string key )
        {
            if (mStorage == null) mStorage = new I2CustomPersistentStorage();
            return mStorage.HasSetting(key);
        }

        public static void ForceSaveSettings()
        {
            if (mStorage == null) mStorage = new I2CustomPersistentStorage();
            mStorage.ForceSaveSettings();
        }

        #endregion

        #region File Management

        public static bool CanAccessFiles()
        {
            if (mStorage == null) mStorage = new I2CustomPersistentStorage();
            return mStorage.CanAccessFiles();
        }

        public static bool SaveFile(eFileType fileType, string fileName, string data, bool logExceptions = true)
        {
            if (mStorage == null) mStorage = new I2CustomPersistentStorage();
            return mStorage.SaveFile(fileType, fileName, data, logExceptions);
        }

        public static string LoadFile(eFileType fileType, string fileName, bool logExceptions=true)
        {
            if (mStorage == null) mStorage = new I2CustomPersistentStorage();
            return mStorage.LoadFile(fileType, fileName, logExceptions);
        }

        public static bool DeleteFile(eFileType fileType, string fileName, bool logExceptions = true)
        {
            if (mStorage == null) mStorage = new I2CustomPersistentStorage();
            return mStorage.DeleteFile(fileType, fileName, logExceptions);
        }

        public static bool HasFile(eFileType fileType, string fileName, bool logExceptions = true)
        {
            if (mStorage == null) mStorage = new I2CustomPersistentStorage();
            return mStorage.HasFile(fileType, fileName, logExceptions);
        }

        #endregion
    }

    public abstract class I2BasePersistentStorage
    {
        #region PlayerPrefs
        public virtual void SetSetting_String(string key, string value)
        {
            try
            {
                // Use PlayerPrefs, but if the data is bigger than the limit, split it into multiple entries
                var len = value.Length;
                int maxLength = 8000;
                if (len<=maxLength)
                {
                    PlayerPrefs.SetString(key, value);
                }
                else
                {
                    int numSections = Mathf.CeilToInt(len / (float)maxLength);
                    for (int i=0; i<numSections; ++i)
                    {
                        int iStart = maxLength * i;
                        PlayerPrefs.SetString($"[I2split]{i}{key}", value.Substring(iStart, Mathf.Min(maxLength, len-iStart)));
                    }
                    PlayerPrefs.SetString(key, "[$I2#@div$]" + numSections);
                }                
            }
            catch (Exception) { Debug.LogError("Error saving PlayerPrefs " + key); }
        }

        public virtual string GetSetting_String(string key, string defaultValue)
        {
            try
            {
                var data = PlayerPrefs.GetString(key, defaultValue);

                // Check if the data is splitted, if so, concat all the sections
                if (!string.IsNullOrEmpty(data) && data.StartsWith("[I2split]", StringComparison.Ordinal))
                {
                    int nSections = int.Parse(data.Substring("[I2split]".Length), CultureInfo.InvariantCulture);
                    data = "";
                    for (int i=0; i<nSections; ++i)
                    {
                        data += PlayerPrefs.GetString($"[I2split]{i}{key}", "");
                    }
                }
                return data;
            }
            catch (Exception)
            {
                Debug.LogError("Error loading PlayerPrefs " + key);
                return defaultValue;
            }
        }

        public virtual void DeleteSetting( string key)
        {
            try
            {
                var data = PlayerPrefs.GetString(key, null);

                // If the data is splitted, delete each section as well
                if (!string.IsNullOrEmpty(data) && data.StartsWith("[I2split]", StringComparison.Ordinal))
                {
                    int nSections = int.Parse(data.Substring("[I2split]".Length), CultureInfo.InvariantCulture);
                    for (int i = 0; i < nSections; ++i)
                    {
                        PlayerPrefs.DeleteKey($"[I2split]{i}{key}");
                    }
                }
                PlayerPrefs.DeleteKey(key);
            }
            catch (Exception)
            {
                Debug.LogError("Error deleting PlayerPrefs " + key);
            }
        }

        public virtual void ForceSaveSettings()
        {
            PlayerPrefs.Save();
        }

        public virtual bool HasSetting(string key)
        {
            return PlayerPrefs.HasKey(key);
        }




        #endregion

        #region Files

        public virtual bool CanAccessFiles()
        {
            #if UNITY_SWITCH || UNITY_WSA
                return false;
            #else
                return true;
            #endif
        }

        string UpdateFilename(PersistentStorage.eFileType fileType, string fileName)
        {
            switch (fileType)
            {
                case PersistentStorage.eFileType.Persistent: fileName = Application.persistentDataPath + "/" + fileName; break;
                case PersistentStorage.eFileType.Temporal:   fileName = Application.temporaryCachePath + "/" + fileName; break;
                case PersistentStorage.eFileType.Streaming: fileName = Application.streamingAssetsPath + "/" + fileName; break;
            }
            return fileName;
        }

        public virtual bool SaveFile(PersistentStorage.eFileType fileType, string fileName, string data, bool logExceptions = true)
        {
            if (!CanAccessFiles())
                return false;

            try
            {
                fileName = UpdateFilename(fileType, fileName);
                File.WriteAllText(fileName, data, Encoding.UTF8);
                return true;
            }
            catch (Exception e)
            {
                if (logExceptions)
                    Debug.LogError("Error saving file '" + fileName + "'\n" + e);
                return false;
            }
        }

        public virtual string LoadFile(PersistentStorage.eFileType fileType, string fileName, bool logExceptions = true)
        {
            if (!CanAccessFiles())
                return null;

            try
            {
                fileName = UpdateFilename(fileType, fileName);
                return File.ReadAllText(fileName, Encoding.UTF8);
            }
            catch (Exception e)
            {
                if (logExceptions)
                    Debug.LogError("Error loading file '" + fileName + "'\n" + e);
                return null;
            }
        }

        public virtual bool DeleteFile(PersistentStorage.eFileType fileType, string fileName, bool logExceptions = true)
        {
            if (!CanAccessFiles())
                return false;

            try
            {
                fileName = UpdateFilename(fileType, fileName);
                File.Delete(fileName);
                return true;
            }
            catch (Exception e)
            {
                if (logExceptions)
                    Debug.LogError("Error deleting file '" + fileName + "'\n" + e);
                return false;
            }
        }

        public virtual bool HasFile(PersistentStorage.eFileType fileType, string fileName, bool logExceptions = true)
        {
            if (!CanAccessFiles())
                return false;

            try
            {
                fileName = UpdateFilename(fileType, fileName);
                return File.Exists(fileName);
            }
            catch (Exception e)
            {
                if (logExceptions) Debug.LogError("Error requesting file '" + fileName + "'\n" + e);
                return false;
            }
        }

#endregion
    }

    public class I2CustomPersistentStorage : I2BasePersistentStorage
    {
        //public override void SetSetting_String(string key, string value)
        //public override string GetSetting_String(string key, string defaultValue)
        //public override void DeleteSetting(string key)
        //public override void ForceSaveSettings()
        //public override bool HasSetting(string key)

        //public virtual bool CanAccessFiles();
        //public override bool SaveFile(PersistentStorage.eFileType fileType, string fileName, string data, bool logExceptions = true);
        //public override string LoadFile(PersistentStorage.eFileType fileType, string fileName, bool logExceptions = true);
        //public override bool DeleteFile(PersistentStorage.eFileType fileType, string fileName, bool logExceptions = true);
        //public override bool HasFile(PersistentStorage.eFileType fileType, string fileName, bool logExceptions = true);
    }
}