using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace cn.sharesdk.unity3d.sdkporter
{
	public class XCodeEditorMenu
	{

		//[MenuItem ("Build Tools/XCode Editor/DebugTest %t")]
		static void DebugTest()
		{	
//			string projectPath = Path.Combine( Directory.GetParent( Application.dataPath ).ToString(), "XCode" );
//			Debug.Log( "XcodePath: " + projectPath );
			
//			XCProject currentProject = new XCProject( projectPath );
			//XCProject.ApplyMod( projectPath, "/Users/Elyn/Projects/UnityPlugins/Unity Sandbox Project/Assets/Modules/GameCenter/Editor/iOS/GameCenter.projmods" );
			
			//Debug.Log(
//			PBXDictionary test = new PBXDictionary();
//			bool result = false;
//			if( test is Dictionary<string, object> )
//				result = true;
//			
//			Debug.Log( result );
			
//			PBXType type = new PBXType();
//			Debug.Log( "TYPE: " + type["isa"] );
//			
//			PBXBuildFile build = new PBXBuildFile( "" );
//			Debug.Log( "BUILDFILE: " + build["isa"] );
			
//			Debug.Log( PBXObject.GenerateGuid().ToUpper() );
//			PBXList testList = currentProject.GetObjectOfType( "XCBuildConfiguration" );
//			Debug.Log( testList.Count );
//			Debug.Log( currentProject.rootGroup.guid + " " + currentProject.rootGroup.name + " " + currentProject.rootGroup.path);
//			string path1 = "Data/mainData";

//			string path2 = "/Users/Elyn/Projects/UnityPlugins/Modules/GameCenter/Editor/iOS/";
//			Debug.Log( "Objects: " + currentProject._objects.Count );
//			Debug.Log( "Files: " + currentProject.buildFiles.Count );
//			Debug.Log( "Groups: " + currentProject.groups.Count );
//			string[] excludes = new string[] {"^.*\\.meta$", "^.*\\.mdown^", "^.*\\.pdf$"};
//			currentProject.AddFolder( path2, null, excludes );
//			currentProject.Consolidate();
//			Debug.Log( "Objects: " + currentProject._objects.Count );
//			currentProject.Save();
			
			//ALTRO
//			currentProject.AddOtherCFlags( "TEST_FLAG" );
//			
//			foreach( KeyValuePair<string, XCBuildConfiguration> config in currentProject.buildConfigurations ) {
//				Debug.Log( "C: " + config.Value.buildSettings["OTHER_CFLAGS"] );
//				foreach( string keys in (PBXList)config.Value.buildSettings["OTHER_CFLAGS"]  )
//					Debug.Log( keys );
//			}
			
//			currentProject.Save();
			
		}
		
		
		//[MenuItem ("Build Tools/XCode Editor/DebugTest2 %y")]
		static void DebugTest2()
		{
			string path1 = "/Users/Elyn/Projects/UnityPlugins/Unity Sandbox Project/Assets/Modules/GameCenter/Editor/iOS/GameCenterManager.m";
			string path2 = "/Users/Elyn/Projects/UnityPlugins/Unity Sandbox Project/XCode/.";
			
			System.Uri fileURI = new System.Uri( path1 );
			System.Uri rootURI = new System.Uri( path2 );
			Debug.Log( fileURI.MakeRelativeUri( rootURI ).ToString() );
			Debug.Log( rootURI.MakeRelativeUri( fileURI ).ToString() );
			
//			string projectPath = Path.Combine( Directory.GetParent( Application.dataPath ).ToString(), "XCode" );
			
//			string[] files = System.IO.Directory.GetFiles( projectPath, "Info.plist" );
//			string contents = System.IO.File.OpenText( files[0] ).ReadToEnd();
			
//			string[] projects = System.IO.Directory.GetDirectories( projectPath, "*.xcodeproj" );
//			string projPath = System.IO.Path.Combine( projects[0], "project.pbxproj" );
//			string contents = System.IO.File.OpenText( projPath ).ReadToEnd();
//			Debug.Log( System.IO.File.OpenText( projPath ).ReadToEnd );

//			PBXParser parser = new PBXParser();
//			Hashtable test = (Hashtable)parser.Decode( contents );
//			PBXDictionary test = parser.Decode( contents );
//			Debug.Log( MiniJSON.jsonEncode( test ) );
//			Debug.Log( test + " - " + test.Count );
//			Debug.Log( parser.Encode( test ) );
			
			
		}

	}
}