using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace cn.sharesdk.unity3d.sdkporter
{
	public class PBXDictionary : Dictionary<string, object>
	{
		
		public void Append( PBXDictionary dictionary )
		{
			foreach( var item in dictionary) {
				this.Add( item.Key, item.Value );
			}
		}
		
		public void Append<T>( PBXDictionary<T> dictionary ) where T : PBXObject
		{
			foreach( var item in dictionary) {
				this.Add( item.Key, item.Value );
			}
		}
	}
	
	public class PBXDictionary<T> : Dictionary<string, T> where T : PBXObject
	{
		public PBXDictionary()
		{
			
		}
		
		public PBXDictionary( PBXDictionary genericDictionary )
		{
			foreach( KeyValuePair<string, object> currentItem in genericDictionary ) {
				if( ((string)((PBXDictionary)currentItem.Value)[ "isa" ]).CompareTo( typeof(T).Name ) == 0 ) {
					T instance = (T)System.Activator.CreateInstance( typeof(T), currentItem.Key, (PBXDictionary)currentItem.Value );
					this.Add( currentItem.Key, instance );
				}
			}	
		}
		
		public void Add( T newObject )
		{
			this.Add( newObject.guid, newObject );
		}
		
		public void Append( PBXDictionary<T> dictionary )
		{
			foreach( KeyValuePair<string, T> item in dictionary) {
				this.Add( item.Key, (T)item.Value );
			}
		}
		
	}
}
