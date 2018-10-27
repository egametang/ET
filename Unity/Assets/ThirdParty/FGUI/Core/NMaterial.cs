using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class NMaterial
	{
		/// <summary>
		/// 
		/// </summary>
		public uint frameId;

		/// <summary>
		/// 
		/// </summary>
		public uint clipId;

		/// <summary>
		/// 
		/// </summary>
		public bool stencilSet;

		/// <summary>
		/// 
		/// </summary>
		public BlendMode blendMode;

		/// <summary>
		/// 
		/// </summary>
		public bool combined;

		/// <summary>
		/// 
		/// </summary>
		public Material material;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="shader"></param>
		public NMaterial(Shader shader)
		{
			material = new Material(shader);
		}
	}
}
