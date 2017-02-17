using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace cn.sharesdk.unity3d.sdkporter
{
	public class PBXBuildPhase : PBXObject
	{
		protected const string FILES_KEY = "files";
		
		public PBXBuildPhase() :base()
		{
//			Debug.Log( "base" );
		}
		
		public PBXBuildPhase( string guid, PBXDictionary dictionary ) : base ( guid, dictionary )
		{
//			Debug.Log( "constructor " + GetType().Name );
		}
		
		public bool AddBuildFile( PBXBuildFile file )
		{
//			if( ((string)file[ ISA_KEY ]).CompareTo( "PBXBuildFile" ) != 0 )
//				return false;
//			Debug.Log( "--> buildphase " + (string)_data[ ISA_KEY ] );
			
			
			if( !ContainsKey( FILES_KEY ) ){
//				Debug.Log( "key not present" );
				this.Add( FILES_KEY, new PBXList() );
			}
//			Debug.Log( "key: " + _data[ FILES_KEY ] );
//			Debug.Log( "Adding: " + file.guid );
			((PBXList)_data[ FILES_KEY ]).Add( file.guid );
//			if( ((PBXList)_data[ FILES_KEY ]).Contains( file.guid ) ) {
//				Debug.Log( "AGGIUNTO" );
//			}
//			else {
//				Debug.Log( "MANCA" );
//			}
			
			return true;
		}
		
		public void RemoveBuildFile( string id )
		{
			if( !ContainsKey( FILES_KEY ) ) {
				this.Add( FILES_KEY, new PBXList() );
				return;
			}
			
			((PBXList)_data[ FILES_KEY ]).Remove( id );
		}
		
		public bool HasBuildFile( string id )
		{
			if( !ContainsKey( FILES_KEY ) ) {
				this.Add( FILES_KEY, new PBXList() );
				return false;
			}
			
			if( !IsGuid( id ) )
				return false;
			
			return ((PBXList)_data[ FILES_KEY ]).Contains( id );
		}
		
//	class PBXBuildPhase(PBXObject):
//    def add_build_file(self, bf):
//        if bf.get('isa') != 'PBXBuildFile':
//            return False
//
//        if not self.has_key('files'):
//            self['files'] = PBXList()
//
//        self['files'].add(bf.id)
//
//        return True
//
//    def remove_build_file(self, id):
//        if not self.has_key('files'):
//            self['files'] = PBXList()
//            return
//
//        self['files'].remove(id)
//
//    def has_build_file(self, id):
//        if not self.has_key('files'):
//            self['files'] = PBXList()
//            return False
//
//        if not PBXObject.IsGuid(id):
//            id = id.id
//
//        return id in self['files']
	}
	
	public class PBXFrameworksBuildPhase : PBXBuildPhase
	{
		public PBXFrameworksBuildPhase( string guid, PBXDictionary dictionary ) : base ( guid, dictionary )
		{
//			Debug.Log( "constructor child" + GetType().Name );
		}
	}

	public class PBXResourcesBuildPhase : PBXBuildPhase
	{
		public PBXResourcesBuildPhase( string guid, PBXDictionary dictionary ) : base ( guid, dictionary )
		{
//			Debug.Log( "constructor child" + GetType().Name );
		}
	}

	public class PBXShellScriptBuildPhase : PBXBuildPhase
	{
		public PBXShellScriptBuildPhase( string guid, PBXDictionary dictionary ) : base ( guid, dictionary )
		{
//			Debug.Log( "constructor child" + GetType().Name );
		}
	}

	public class PBXSourcesBuildPhase : PBXBuildPhase
	{
		public PBXSourcesBuildPhase( string guid, PBXDictionary dictionary ) : base ( guid, dictionary )
		{
//			Debug.Log( "constructor child" + GetType().Name );
		}
	}

	public class PBXCopyFilesBuildPhase : PBXBuildPhase
	{
		public PBXCopyFilesBuildPhase( string guid, PBXDictionary dictionary ) : base ( guid, dictionary )
		{
//			Debug.Log( "constructor child" + GetType().Name );
		}
	}
}
