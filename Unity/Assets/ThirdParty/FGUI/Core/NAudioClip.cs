using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class NAudioClip
	{
		/// <summary>
		/// 
		/// </summary>
		public DestroyMethod destroyMethod;

		/// <summary>
		/// 
		/// </summary>
		public AudioClip nativeClip;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="audioClip"></param>
		public NAudioClip(AudioClip audioClip)
		{
			nativeClip = audioClip;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Unload()
		{
			if (nativeClip == null)
				return;

			if (destroyMethod == DestroyMethod.Unload)
				Resources.UnloadAsset(nativeClip);
			else if (destroyMethod == DestroyMethod.Destroy)
				Object.DestroyImmediate(nativeClip, true);

			nativeClip = null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="audioClip"></param>
		public void Reload(AudioClip audioClip)
		{
			nativeClip = audioClip;
		}
	}
}
