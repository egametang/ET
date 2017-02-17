using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace cn.sharesdk.unity3d.sdkporter
{
	public class PBXProject : PBXObject
	{
		protected string MAINGROUP_KEY = "mainGroup";
		
		public PBXProject() : base() {
		}
		
		public PBXProject( string guid, PBXDictionary dictionary ) : base( guid, dictionary ) {	
		}
		
		public string mainGroupID {
			get {
				return (string)_data[ MAINGROUP_KEY ];
			}
		}
	}
}
