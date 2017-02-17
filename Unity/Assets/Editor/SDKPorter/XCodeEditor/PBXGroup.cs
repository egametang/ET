using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace cn.sharesdk.unity3d.sdkporter
{
	public class PBXGroup : PBXObject
	{
		protected const string NAME_KEY = "name";
		protected const string CHILDREN_KEY = "children";
		protected const string PATH_KEY = "path";
		protected const string SOURCETREE_KEY = "sourceTree";
		
		#region Constructor
		
		public PBXGroup( string name, string path = null, string tree = "SOURCE_ROOT" ) : base()
		{	
			this.Add( NAME_KEY, name );
			this.Add( CHILDREN_KEY, new PBXList() );
			
			if( path != null ) {
				this.Add( PATH_KEY, path );
				this.Add( SOURCETREE_KEY, tree );
			}
			else {
				this.Add( SOURCETREE_KEY, "<group>" );
			}
		}
		
		public PBXGroup( string guid, PBXDictionary dictionary ) : base( guid, dictionary )
		{
			
		}
		
		#endregion
		#region Properties
		
		public string name {
			get {
				if( !ContainsKey( NAME_KEY ) ) {
					return null;
				}
				return (string)_data[NAME_KEY];
			}
		}
		
		public PBXList children {
			get {
				if( !ContainsKey( CHILDREN_KEY ) ) {
					this.Add( CHILDREN_KEY, new PBXList() );
				}
				return (PBXList)_data[CHILDREN_KEY];
			}
		}
		
		public string path {
			get {
				if( !ContainsKey( PATH_KEY ) ) {
					return null;
				}
				return (string)_data[PATH_KEY];
			}
		}
		
		public string sourceTree {
			get {
				return (string)_data[SOURCETREE_KEY];
			}
		}
		
		#endregion
		
		
		public string AddChild( PBXObject child )
		{
			if( child is PBXFileReference || child is PBXGroup ) {
				children.Add( child.guid );
				return child.guid;
			}
				
			return null;
		}
		
		public void RemoveChild( string id )
		{
			if( !IsGuid( id ) )
				return;
			
			children.Remove( id );
		}
		
		public bool HasChild( string id )
		{
			if( !ContainsKey( CHILDREN_KEY ) ) {
				this.Add( CHILDREN_KEY, new PBXList() );
				return false;
			}
			
			if( !IsGuid( id ) )
				return false;
			
			return ((PBXList)_data[ CHILDREN_KEY ]).Contains( id );
		}
		
		public string GetName()
		{
			return (string)_data[ NAME_KEY ];
		}
		
//	class PBXGroup(PBXObject):
//    def add_child(self, ref):
//        if not isinstance(ref, PBXDict):
//            return None
//
//        isa = ref.get('isa')
//
//        if isa != 'PBXFileReference' and isa != 'PBXGroup':
//            return None
//
//        if not self.has_key('children'):
//            self['children'] = PBXList()
//
//        self['children'].add(ref.id)
//
//        return ref.id
//
//    def remove_child(self, id):
//        if not self.has_key('children'):
//            self['children'] = PBXList()
//            return
//
//        if not PBXObject.IsGuid(id):
//            id = id.id
//
//        self['children'].remove(id)
//
//    def has_child(self, id):
//        if not self.has_key('children'):
//            self['children'] = PBXList()
//            return False
//
//        if not PBXObject.IsGuid(id):
//            id = id.id
//
//        return id in self['children']
//
//    def get_name(self):
//        path_name = os.path.split(self.get('path',''))[1]
//        return self.get('name', path_name)
//
//    @classmethod
//    def Create(cls, name, path=None, tree='SOURCE_ROOT'):
//        grp = cls()
//        grp.id = cls.GenerateId()
//        grp['name'] = name
//        grp['children'] = PBXList()
//
//        if path:
//            grp['path'] = path
//            grp['sourceTree'] = tree
//        else:
//            grp['sourceTree'] = '<group>'
//
//        return grp
	}
}
