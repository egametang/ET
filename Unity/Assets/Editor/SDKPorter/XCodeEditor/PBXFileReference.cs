using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace cn.sharesdk.unity3d.sdkporter
{
	public class PBXFileReference : PBXObject
	{
		protected const string PATH_KEY = "path";
		protected const string NAME_KEY = "name";
		protected const string SOURCETREE_KEY = "sourceTree";
		protected const string EXPLICIT_FILE_TYPE_KEY = "explicitFileType";
		protected const string LASTKNOWN_FILE_TYPE_KEY = "lastKnownFileType";
		protected const string ENCODING_KEY = "fileEncoding";
		
		public string buildPhase;
		public readonly Dictionary<TreeEnum, string> trees = new Dictionary<TreeEnum, string> {
			{ TreeEnum.ABSOLUTE, "<absolute>" },
			{ TreeEnum.GROUP, "<group>" },
			{ TreeEnum.BUILT_PRODUCTS_DIR, "BUILT_PRODUCTS_DIR" },
        	{ TreeEnum.DEVELOPER_DIR, "DEVELOPER_DIR" },
        	{ TreeEnum.SDKROOT, "SDKROOT" },
        	{ TreeEnum.SOURCE_ROOT, "SOURCE_ROOT" }
		};
		
		public static readonly Dictionary<string, string> typeNames = new Dictionary<string, string> {
			{ ".a", "archive.ar" },
			{ ".app", "wrapper.application" },
			{ ".s", "sourcecode.asm" },
			{ ".c", "sourcecode.c.c" },
			{ ".cpp", "sourcecode.cpp.cpp" },
			{ ".framework", "wrapper.framework" },
			{ ".h", "sourcecode.c.h" },
			{ ".icns", "image.icns" },
			{ ".m", "sourcecode.c.objc" },
			{ ".mm", "sourcecode.cpp.objcpp" },
			{ ".nib", "wrapper.nib" },
			{ ".plist", "text.plist.xml" },
			{ ".png", "image.png" },
			{ ".rtf", "text.rtf" },
			{ ".tiff", "image.tiff" },
			{ ".txt", "text" },
			{ ".xcodeproj", "wrapper.pb-project" },
			{ ".xib", "file.xib" },
			{ ".strings", "text.plist.strings" },
			{ ".bundle", "wrapper.plug-in" },
			{ ".dylib", "compiled.mach-o.dylib" }
   		 };
		
		public static readonly Dictionary<string, string> typePhases = new Dictionary<string, string> {
			{ ".a", "PBXFrameworksBuildPhase" },
			{ ".app", null },
			{ ".s", "PBXSourcesBuildPhase" },
			{ ".c", "PBXSourcesBuildPhase" },
			{ ".cpp", "PBXSourcesBuildPhase" },
			{ ".framework", "PBXFrameworksBuildPhase" },
			{ ".h", null },
			{ ".icns", "PBXResourcesBuildPhase" },
			{ ".m", "PBXSourcesBuildPhase" },
			{ ".mm", "PBXSourcesBuildPhase" },
			{ ".nib", "PBXResourcesBuildPhase" },
			{ ".plist", "PBXResourcesBuildPhase" },
			{ ".png", "PBXResourcesBuildPhase" },
			{ ".rtf", "PBXResourcesBuildPhase" },
			{ ".tiff", "PBXResourcesBuildPhase" },
			{ ".txt", "PBXResourcesBuildPhase" },
			{ ".xcodeproj", null },
			{ ".xib", "PBXResourcesBuildPhase" },
			{ ".strings", "PBXResourcesBuildPhase" },
			{ ".bundle", "PBXResourcesBuildPhase" },
			{ ".dylib", "PBXFrameworksBuildPhase" }
    	};
		
		public PBXFileReference( string guid, PBXDictionary dictionary ) : base( guid, dictionary )
		{
			
		}
		
		public PBXFileReference( string filePath, TreeEnum tree = TreeEnum.SOURCE_ROOT ) : base()
		{
			this.Add( PATH_KEY, filePath );
			this.Add( NAME_KEY, System.IO.Path.GetFileName( filePath ) );
			this.Add( SOURCETREE_KEY, (string)( System.IO.Path.IsPathRooted( filePath ) ? trees[TreeEnum.ABSOLUTE] : trees[tree] ) );
			this.GuessFileType();
		}
		
		public string name {
			get {
				if( !ContainsKey( NAME_KEY ) ) {
					return null;
				}
				return (string)_data[NAME_KEY];
			}
		}
		
		private void GuessFileType()
		{
			this.Remove( EXPLICIT_FILE_TYPE_KEY );
			this.Remove( LASTKNOWN_FILE_TYPE_KEY );
			string extension = System.IO.Path.GetExtension( (string)_data[ PATH_KEY ] );
			if( !PBXFileReference.typeNames.ContainsKey( extension ) ){
//				Debug.LogWarning( "Unknown file extension: " + extension + "\nPlease add extension and Xcode type to PBXFileReference.types" );
				return;
			}
			
			this.Add( LASTKNOWN_FILE_TYPE_KEY, PBXFileReference.typeNames[ extension ] );
			this.buildPhase = PBXFileReference.typePhases[ extension ];
		}
		
		private void SetFileType( string fileType )
		{
			this.Remove( EXPLICIT_FILE_TYPE_KEY );
			this.Remove( LASTKNOWN_FILE_TYPE_KEY );
			
			this.Add( EXPLICIT_FILE_TYPE_KEY, fileType );
		}
	}
	
	public enum TreeEnum {
		ABSOLUTE,
        GROUP,
        BUILT_PRODUCTS_DIR,
        DEVELOPER_DIR,
        SDKROOT,
        SOURCE_ROOT
	}
}
