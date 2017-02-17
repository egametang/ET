using UnityEngine;
using System.Collections;

namespace cn.sharesdk.unity3d.sdkporter
{
	public class XCBuildConfiguration : PBXObject
	{
		protected const string BUILDSETTINGS_KEY = "buildSettings";
		protected const string HEADER_SEARCH_PATHS_KEY = "HEADER_SEARCH_PATHS";
		protected const string LIBRARY_SEARCH_PATHS_KEY = "LIBRARY_SEARCH_PATHS";
		protected const string FRAMEWORK_SEARCH_PATHS_KEY = "FRAMEWORK_SEARCH_PATHS";
		protected const string OTHER_C_FLAGS_KEY = "OTHER_CFLAGS";
		protected const string OTHER_LD_FLAGS_KEY = "OTHER_LDFLAGS";
		protected const string GCC_ENABLE_CPP_EXCEPTIONS_KEY = "GCC_ENABLE_CPP_EXCEPTIONS";
		protected const string GCC_ENABLE_OBJC_EXCEPTIONS_KEY = "GCC_ENABLE_OBJC_EXCEPTIONS";

		public XCBuildConfiguration( string guid, PBXDictionary dictionary ) : base( guid, dictionary )
		{
			
		}
		
		public PBXDictionary buildSettings {
			get {
				if( ContainsKey( BUILDSETTINGS_KEY ) )
					return (PBXDictionary)_data[BUILDSETTINGS_KEY];
			
				return null;
			}
		}
		
		protected bool AddSearchPaths( string path, string key, bool recursive = true )
		{
			PBXList paths = new PBXList();
			paths.Add( path );
			return AddSearchPaths( paths, key, recursive );
		}
		
		protected bool AddSearchPaths( PBXList paths, string key, bool recursive = true )
		{	
			bool modified = false;
			
			if( !ContainsKey( BUILDSETTINGS_KEY ) )
				this.Add( BUILDSETTINGS_KEY, new PBXDictionary() );
			
			foreach( string path in paths ) {
				string currentPath = path;
				if( recursive && !path.EndsWith( "/**" ) )
					currentPath += "/**";
				
//				Debug.Log( "adding: " + currentPath );
				if( !((PBXDictionary)_data[BUILDSETTINGS_KEY]).ContainsKey( key ) ) {
					((PBXDictionary)_data[BUILDSETTINGS_KEY]).Add( key, new PBXList() );
				}
				else if( ((PBXDictionary)_data[BUILDSETTINGS_KEY])[key] is string ) {
					PBXList list = new PBXList();
					list.Add( ((PBXDictionary)_data[BUILDSETTINGS_KEY])[key] );
					((PBXDictionary)_data[BUILDSETTINGS_KEY])[key] = list;
				}
				
				currentPath = "\\\"" + currentPath + "\\\"";
				
				if( !((PBXList)((PBXDictionary)_data[BUILDSETTINGS_KEY])[key]).Contains( currentPath ) ) {
					((PBXList)((PBXDictionary)_data[BUILDSETTINGS_KEY])[key]).Add( currentPath );
					modified = true;
				}
			}
		
			return modified;
		}
		
		public bool AddHeaderSearchPaths( PBXList paths, bool recursive = true )
		{
			return this.AddSearchPaths( paths, HEADER_SEARCH_PATHS_KEY, recursive );
		}
		
		public bool AddLibrarySearchPaths( PBXList paths, bool recursive = true )
		{
			return this.AddSearchPaths( paths, LIBRARY_SEARCH_PATHS_KEY, recursive );
		}

		public bool AddFrameworkSearchPaths(PBXList paths, bool recursive = true)
		{
			return this.AddSearchPaths(paths, FRAMEWORK_SEARCH_PATHS_KEY, recursive);
		}
		
		public bool AddOtherCFlags( string flag )
		{
			//Debug.Log( "INIZIO 1" );
			PBXList flags = new PBXList();
			flags.Add( flag );
			return AddOtherCFlags( flags );
		}
		
		public bool AddOtherCFlags( PBXList flags )
		{
			//Debug.Log( "INIZIO 2" );
			
			bool modified = false;
			
			if( !ContainsKey( BUILDSETTINGS_KEY ) )
				this.Add( BUILDSETTINGS_KEY, new PBXDictionary() );
			
			foreach( string flag in flags ) {
				
				if( !((PBXDictionary)_data[BUILDSETTINGS_KEY]).ContainsKey( OTHER_C_FLAGS_KEY ) ) {
					((PBXDictionary)_data[BUILDSETTINGS_KEY]).Add( OTHER_C_FLAGS_KEY, new PBXList() );
				}
				else if ( ((PBXDictionary)_data[BUILDSETTINGS_KEY])[ OTHER_C_FLAGS_KEY ] is string ) {
					string tempString = (string)((PBXDictionary)_data[BUILDSETTINGS_KEY])[OTHER_C_FLAGS_KEY];
					((PBXDictionary)_data[BUILDSETTINGS_KEY])[ OTHER_C_FLAGS_KEY ] = new PBXList();
					((PBXList)((PBXDictionary)_data[BUILDSETTINGS_KEY])[OTHER_C_FLAGS_KEY]).Add( tempString );
				}
				
				if( !((PBXList)((PBXDictionary)_data[BUILDSETTINGS_KEY])[OTHER_C_FLAGS_KEY]).Contains( flag ) ) {
					((PBXList)((PBXDictionary)_data[BUILDSETTINGS_KEY])[OTHER_C_FLAGS_KEY]).Add( flag );
					modified = true;
				}
			}
			
			return modified;
		}

		public bool AddOtherLDFlags( string flag )
		{
			//Debug.Log( "INIZIO A" );
			PBXList flags = new PBXList();
			flags.Add( flag );
			return AddOtherLDFlags( flags );
		}

		public bool AddOtherLDFlags( PBXList flags )
		{
			//Debug.Log( "INIZIO B" );
			
			bool modified = false;
			
			if( !ContainsKey( BUILDSETTINGS_KEY ) )
				this.Add( BUILDSETTINGS_KEY, new PBXDictionary() );
			
			foreach( string flag in flags ) {
				
				if( !((PBXDictionary)_data[BUILDSETTINGS_KEY]).ContainsKey( OTHER_LD_FLAGS_KEY ) ) {
					((PBXDictionary)_data[BUILDSETTINGS_KEY]).Add( OTHER_LD_FLAGS_KEY, new PBXList() );
				}
				else if ( ((PBXDictionary)_data[BUILDSETTINGS_KEY])[ OTHER_LD_FLAGS_KEY ] is string ) {
					string tempString = (string)((PBXDictionary)_data[BUILDSETTINGS_KEY])[OTHER_LD_FLAGS_KEY];
					((PBXDictionary)_data[BUILDSETTINGS_KEY])[ OTHER_LD_FLAGS_KEY ] = new PBXList();
					((PBXList)((PBXDictionary)_data[BUILDSETTINGS_KEY])[OTHER_LD_FLAGS_KEY]).Add( tempString );
				}
				
				if( !((PBXList)((PBXDictionary)_data[BUILDSETTINGS_KEY])[OTHER_LD_FLAGS_KEY]).Contains( flag ) ) {
					((PBXList)((PBXDictionary)_data[BUILDSETTINGS_KEY])[OTHER_LD_FLAGS_KEY]).Add( flag );
					modified = true;
				}
			}
			
			return modified;
		}

		public bool GccEnableCppExceptions (string value)
		{
			if (!ContainsKey (BUILDSETTINGS_KEY))
				this.Add (BUILDSETTINGS_KEY, new PBXDictionary ());

			((PBXDictionary)_data [BUILDSETTINGS_KEY])[GCC_ENABLE_CPP_EXCEPTIONS_KEY] = value;
			return true;
		}

		public bool GccEnableObjCExceptions (string value)
		{
			if (!ContainsKey (BUILDSETTINGS_KEY))
				this.Add (BUILDSETTINGS_KEY, new PBXDictionary ());

			((PBXDictionary)_data [BUILDSETTINGS_KEY])[GCC_ENABLE_OBJC_EXCEPTIONS_KEY] = value;
			return true;
		}
		
//	class XCBuildConfiguration(PBXType):
//    def add_search_paths(self, paths, base, key, recursive=True):
//        modified = False
//
//        if not isinstance(paths, list):
//            paths = [paths]
//
//        if not self.has_key(base):
//            self[base] = PBXDict()
//
//        for path in paths:
//            if recursive and not path.endswith('/**'):
//                path = os.path.join(path, '**')
//
//            if not self[base].has_key(key):
//                self[base][key] = PBXList()
//            elif isinstance(self[base][key], basestring):
//                self[base][key] = PBXList(self[base][key])
//
//            if self[base][key].add('\\"%s\\"' % path):
//                modified = True
//
//        return modified
//
//    def add_header_search_paths(self, paths, recursive=True):
//        return self.add_search_paths(paths, 'buildSettings', 'HEADER_SEARCH_PATHS', recursive=recursive)
//
//    def add_library_search_paths(self, paths, recursive=True):
//        return self.add_search_paths(paths, 'buildSettings', 'LIBRARY_SEARCH_PATHS', recursive=recursive)
//
//    def add_other_cflags(self, flags):
//        modified = False
//
//        base = 'buildSettings'
//        key = 'OTHER_CFLAGS'
//
//        if isinstance(flags, basestring):
//            flags = PBXList(flags)
//
//        if not self.has_key(base):
//            self[base] = PBXDict()
//
//        for flag in flags:
//
//            if not self[base].has_key(key):
//                self[base][key] = PBXList()
//            elif isinstance(self[base][key], basestring):
//                self[base][key] = PBXList(self[base][key])
//
//            if self[base][key].add(flag):
//                self[base][key] = [e for e in self[base][key] if e]
//                modified = True
//
//        return modified
	}
}