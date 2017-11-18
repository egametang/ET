#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace MyEditor
{
	public class BuildIOSEditor : EditorWindow
	{
		[PostProcessBuild]
		private static void OnPostprocessBuild(BuildTarget target, string pathToBuildProject)
		{
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
			string _projPath = PBXProject.GetPBXProjectPath(pathToBuildProject);
			PBXProject project = new PBXProject();
			project.ReadFromString(File.ReadAllText(_projPath));
			string targetGuid = project.TargetGuidByName("Unity-iPhone");

			//project.AddFrameworkToProject(targetGuid, "Security.framework", false);

			project.WriteToFile(_projPath);


			// plist
			string plistPath = pathToBuildProject + "/Info.plist";
			PlistDocument plist = new PlistDocument();
			plist.ReadFromString(File.ReadAllText(plistPath));
			PlistElementDict rootDict = plist.root;
			
			PlistElementDict dictTmp = rootDict.CreateDict("NSAppTransportSecurity");
			dictTmp.SetBoolean("NSAllowsArbitraryLoads", true);

			// 保存plist  
			plist.WriteToFile(plistPath);
		}
	}
}
#endif