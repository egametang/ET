using PF;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace ETModel
{
	[ObjectSystem]
	public class UnitAwakeSystem : AwakeSystem<Unit, GameObject>
	{
		public override void Awake(Unit self, GameObject gameObject)
		{
			self.Awake(gameObject);
		}
	}
	
	[HideInHierarchy]
	public sealed class Unit: Entity
	{
		public void Awake(GameObject gameObject)
		{
			this.ViewGO = gameObject;
			this.ViewGO.AddComponent<ComponentView>().Component = this;
		}
		
		public Vector3 Position
		{
			get
			{
				return ViewGO.transform.position;
			}
			set
			{
				ViewGO.transform.position = value;
			}
		}

		public Quaternion Rotation
		{
			get
			{
				return ViewGO.transform.rotation;
			}
			set
			{
				ViewGO.transform.rotation = value;
			}
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();
		}
	}
}