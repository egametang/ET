using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class PackageItem
	{
		public UIPackage owner;

		public PackageItemType type;
		public ObjectType objectType;

		public string id;
		public string name;
		public int width;
		public int height;
		public string file;
		public bool exported;
		public NTexture texture;
		public ByteBuffer rawData;

		//image
		public Rect? scale9Grid;
		public bool scaleByTile;
		public int tileGridIndice;
		public PixelHitTestData pixelHitTestData;

		//movieclip
		public float interval;
		public float repeatDelay;
		public bool swing;
		public MovieClip.Frame[] frames;

		//component
		public bool translated;
		public UIObjectFactory.GComponentCreator extensionCreator;

		//font
		public BitmapFont bitmapFont;

		//sound
		public NAudioClip audioClip;

		public object Load()
		{
			return owner.GetItemAsset(this);
		}
	}
}
