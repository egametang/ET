using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public enum DestroyMethod
	{
		Destroy,
		Unload,
		None
	}

	/// <summary>
	/// 
	/// </summary>
	public class NTexture
	{
		/// <summary>
		/// 
		/// </summary>
		public Rect uvRect;

		/// <summary>
		/// 
		/// </summary>
		public bool rotated;

		/// <summary>
		/// 
		/// </summary>
		public int refCount;

		/// <summary>
		/// 
		/// </summary>
		public float lastActive;

		/// <summary>
		/// 
		/// </summary>
		public DestroyMethod destroyMethod;

		Texture _nativeTexture;
		Texture _alphaTexture;
		Rect _region;
		NTexture _root;
		Dictionary<string, MaterialManager> _materialManagers;

		internal static Texture2D CreateEmptyTexture()
		{
			Texture2D emptyTexture = new Texture2D(1, 1, TextureFormat.RGB24, false);
			emptyTexture.name = "White Texture";
			emptyTexture.hideFlags = DisplayOptions.hideFlags;
			emptyTexture.SetPixel(0, 0, Color.white);
			emptyTexture.Apply();
			return emptyTexture;
		}

		static NTexture _empty;

		/// <summary>
		/// 
		/// </summary>
		public static NTexture Empty
		{
			get
			{
				if (_empty == null)
					_empty = new NTexture(CreateEmptyTexture());

				return _empty;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static void DisposeEmpty()
		{
			if (_empty != null)
			{
				NTexture tmp = _empty;
				_empty = null;
				tmp.Dispose();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		public NTexture(Texture texture)
		{
			_root = this;
			_nativeTexture = texture;
			uvRect = new Rect(0, 0, 1, 1);
			_region = new Rect(0, 0, texture.width, texture.height);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="xScale"></param>
		/// <param name="yScale"></param>
		public NTexture(Texture texture, Texture alphaTexture, float xScale, float yScale)
		{
			_root = this;
			_nativeTexture = texture;
			_alphaTexture = alphaTexture;
			uvRect = new Rect(0, 0, xScale, yScale);
			_region = new Rect(0, 0, texture.width, texture.height);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="region"></param>
		public NTexture(Texture texture, Rect region)
		{
			_root = this;
			_nativeTexture = texture;
			_region = region;
			uvRect = new Rect(region.x / _nativeTexture.width, 1 - region.yMax / _nativeTexture.height,
				region.width / _nativeTexture.width, region.height / _nativeTexture.height);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="root"></param>
		/// <param name="region"></param>
		public NTexture(NTexture root, Rect region, bool rotated)
		{
			_root = root;
			this.rotated = rotated;
			region.x += root._region.x;
			region.y += root._region.y;
			uvRect = new Rect(region.x * root.uvRect.width / root.width, 1 - region.yMax * root.uvRect.height / root.height,
				region.width * root.uvRect.width / root.width, region.height * root.uvRect.height / root.height);
			if (rotated)
			{
				float tmp = region.width;
				region.width = region.height;
				region.height = tmp;

				tmp = uvRect.width;
				uvRect.width = uvRect.height;
				uvRect.height = tmp;
			}
			_region = region;
		}

		/// <summary>
		/// 
		/// </summary>
		public int width
		{
			get { return (int)_region.width; }
		}

		/// <summary>
		/// 
		/// </summary>
		public int height
		{
			get { return (int)_region.height; }
		}

		/// <summary>
		/// 
		/// </summary>
		public NTexture root
		{
			get { return _root; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool disposed
		{
			get { return _root == null; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Texture nativeTexture
		{
			get { return _root != null ? _root._nativeTexture : null; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Texture alphaTexture
		{
			get { return _root != null ? _root._alphaTexture : null; }
		}

		static uint _gCounter;

		/// <summary>
		/// 
		/// </summary>
		public MaterialManager GetMaterialManager(string shaderName, string[] keywords)
		{
			if (_root != this)
				return _root.GetMaterialManager(shaderName, keywords);

			if (_materialManagers == null)
				_materialManagers = new Dictionary<string, MaterialManager>();

			string key = shaderName;
			if (keywords != null)
			{
				//对于带指定关键字的，目前的设计是不参加共享材质了，因为逻辑会变得更复杂
				key = shaderName + "_" + _gCounter++;
			}

			MaterialManager mm;
			if (!_materialManagers.TryGetValue(key, out mm))
			{
				mm = new MaterialManager(this, ShaderConfig.GetShader(shaderName), keywords);
				mm._managerKey = key;
				_materialManagers.Add(key, mm);
			}

			return mm;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="manager"></param>
		public void DestroyMaterialManager(MaterialManager manager)
		{
			_materialManagers.Remove(manager._managerKey);
			manager.DestroyMaterials();
		}

		/// <summary>
		/// 
		/// </summary>
		public void Unload()
		{
			Unload(false);
		}

		/// <summary>
		/// 
		/// </summary>
		public void Unload(bool destroyMaterials)
		{
			if (this == _empty)
				return;

			if (_root != this)
				throw new System.Exception("Unload is not allow to call on none root NTexture.");

			if (_nativeTexture != null)
			{
				if (destroyMethod == DestroyMethod.Destroy)
				{
					Object.DestroyImmediate(_nativeTexture, true);
					if (_alphaTexture != null)
						Object.DestroyImmediate(_alphaTexture, true);
				}
				else if (destroyMethod == DestroyMethod.Unload)
				{
					Resources.UnloadAsset(_nativeTexture);
					if (_alphaTexture != null)
						Resources.UnloadAsset(_alphaTexture);
				}

				_nativeTexture = null;
				_alphaTexture = null;

				if (destroyMaterials)
					DestroyMaterials();
				else
					RefreshMaterials();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nativeTexture"></param>
		/// <param name="alphaTexture"></param>
		public void Reload(Texture nativeTexture, Texture alphaTexture)
		{
			if (_root != this)
				throw new System.Exception("Reload is not allow to call on none root NTexture.");

			_nativeTexture = nativeTexture;
			_alphaTexture = alphaTexture;

			RefreshMaterials();
		}

		void RefreshMaterials()
		{
			if (_materialManagers != null && _materialManagers.Count > 0)
			{
				Dictionary<string, MaterialManager>.Enumerator iter = _materialManagers.GetEnumerator();
				while (iter.MoveNext())
					iter.Current.Value.RefreshMaterials();
				iter.Dispose();
			}
		}

		void DestroyMaterials()
		{
			if (_materialManagers != null && _materialManagers.Count > 0)
			{
				Dictionary<string, MaterialManager>.Enumerator iter = _materialManagers.GetEnumerator();
				while (iter.MoveNext())
					iter.Current.Value.DestroyMaterials();
				iter.Dispose();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			if (this == _empty)
				return;

			if (_root == this)
				Unload(true);
			_root = null;
		}
	}
}
