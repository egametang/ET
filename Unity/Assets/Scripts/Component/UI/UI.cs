using Base;
using UnityEngine;
using Component = Base.Component;

namespace Model
{
	public sealed class UI: Component
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
	}
}