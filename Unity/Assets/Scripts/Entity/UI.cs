using Base;
using UnityEngine;
using Component = Base.Component;

namespace Model
{
	public sealed class UI: Entity
	{
		public Entity Scene { get; set; }

		public UIType UIType { get; set; }

		public string Name { get; set; }

		public GameObject GameObject { get; set; }
		
		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();
		}

		public UI(): base(EntityType.UI)
		{
		}

		public UI(long id): base(id, EntityType.UI)
		{
		}
	}
}