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
        public string[] branches;
        public string[] highResolution;

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

        //spine/dragonbones
        public Vector2 skeletonAnchor;
        public object skeletonAsset;

        public object Load()
        {
            return owner.GetItemAsset(this);
        }

        public PackageItem getBranch()
        {
            if (branches != null && owner._branchIndex != -1)
            {
                string itemId = branches[owner._branchIndex];
                if (itemId != null)
                    return owner.GetItem(itemId);
            }

            return this;
        }

        public PackageItem getHighResolution()
        {
            if (highResolution != null && GRoot.contentScaleLevel > 0)
            {
                int i = GRoot.contentScaleLevel - 1;
                if (i >= highResolution.Length)
                    i = highResolution.Length - 1;
                string itemId = highResolution[i];
                if (itemId != null)
                    return owner.GetItem(itemId);
            }

            return this;
        }
    }
}
