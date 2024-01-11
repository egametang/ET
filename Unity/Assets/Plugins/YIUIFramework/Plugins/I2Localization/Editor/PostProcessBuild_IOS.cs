#if UNITY_IOS || UNITY_IPHONE
using UnityEditor.Callbacks;
using System.Collections;
using UnityEditor.iOS_I2Loc.Xcode;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;


namespace I2.Loc
{
    public class PostProcessBuild_IOS
    {
        [PostProcessBuild(10000)]
        public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget != BuildTarget.iOS)
                return;

			if (LocalizationManager.Sources.Count <= 0)
				LocalizationManager.UpdateSources();
            var langCodes = LocalizationManager.GetAllLanguagesCode(false).Concat(LocalizationManager.GetAllLanguagesCode(true)).Distinct().ToList();
            if (langCodes.Count <= 0)
				return;
				
            try
            {
			//----[ Export localized languages to the info.plist ]---------

                string plistPath = pathToBuiltProject + "/Info.plist";
                PlistDocument plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(plistPath));

				PlistElementDict rootDict = plist.root;

                // Get Language root
                var langArray = rootDict.CreateArray("CFBundleLocalizations");

                // Set the Language Codes
                foreach (var code in langCodes)
                {
                    if (code == null || code.Length < 2)
                        continue;
                    langArray.AddString(code);
                }

				rootDict.SetString("CFBundleDevelopmentRegion", langCodes[0]);

                // Write to file
                File.WriteAllText(plistPath, plist.WriteToString());

			//--[ Localize App Name ]----------

				string LocalizationRoot = pathToBuiltProject + "/I2Localization";
				if (!Directory.Exists(LocalizationRoot))
					Directory.CreateDirectory(LocalizationRoot);
				
				var project = new PBXProject();
				string projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
				//if (!projPath.EndsWith("xcodeproj"))
					//projPath = projPath.Substring(0, projPath.LastIndexOfAny("/\\".ToCharArray()));

				project.ReadFromFile(projPath);
				//var targetName = PBXProject.GetUnityTargetName();
				//string projBuild = project.TargetGuidByName( targetName );

				project.RemoveLocalizationVariantGroup("I2 Localization");
				// Set the Language Overrides
				foreach (var code in langCodes)
				{
					if (code == null || code.Length < 2)
						continue;

					var LanguageDirRoot = LocalizationRoot + "/" + code + ".lproj";
					if (!Directory.Exists(LanguageDirRoot))
						Directory.CreateDirectory(LanguageDirRoot);

					var infoPlistPath = LanguageDirRoot + "/InfoPlist.strings";
					var InfoPlist = string.Format("CFBundleDisplayName = \"{0}\";", LocalizationManager.GetAppName(code));
					File.WriteAllText(infoPlistPath, InfoPlist);

					var langProjectRoot = "I2Localization/"+code+".lproj";

					var stringPaths = LanguageDirRoot + "/Localizable.strings";
					File.WriteAllText(stringPaths, string.Empty);
	
					project.AddLocalization(langProjectRoot + "/Localizable.strings", langProjectRoot + "/Localizable.strings", "I2 Localization");
					project.AddLocalization(langProjectRoot + "/InfoPlist.strings", langProjectRoot + "/InfoPlist.strings", "I2 Localization");
				}

				project.WriteToFile(projPath);
				
            }
            catch (System.Exception e)
            { 
				Debug.Log (e);
			}
        }
    }
}
#endif