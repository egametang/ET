using System;
using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;
#if !SERVER
using UnityEngine;
#endif

namespace ET
{
    public abstract class Object: ISupportInitialize, IDisposable
    {
#if UNITY_EDITOR
		public static GameObject Global
		{
			get
			{
				return GameObject.Find("/Global");
			}
		}

		[BsonIgnore]
		public GameObject ViewGO { get; }
#endif
		
        public Object()
        {
#if UNITY_EDITOR
			if (!this.GetType().IsDefined(typeof (HideInHierarchy), true))
			{
				this.ViewGO = new GameObject();
				this.ViewGO.name = this.GetType().Name;
				this.ViewGO.layer = LayerMask.NameToLayer("Hidden");
				this.ViewGO.transform.SetParent(Global.transform, false);
				this.ViewGO.AddComponent<ComponentView>().Component = this;
			}
#endif
        }
		
        public virtual void BeginInit()
        {
        }

        public virtual void EndInit()
        {
        }

        public virtual void Dispose()
        {
#if UNITY_EDITOR
			if (this.ViewGO != null)
			{
				UnityEngine.Object.Destroy(this.ViewGO);
			}
#endif
        }
    }
}