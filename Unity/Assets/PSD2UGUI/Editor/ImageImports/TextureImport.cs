using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace PSDUIImporter
{
    public class TextureImport : IImageImport
    {
        public void DrawImage(PSImage image, GameObject parent, GameObject ownObj = null)
        {
            UnityEngine.UI.RawImage pic;
            if (ownObj != null)
                pic = ownObj.AddComponent<UnityEngine.UI.RawImage>();
            else
                pic = PSDImportUtility.LoadAndInstant<UnityEngine.UI.RawImage>(PSDImporterConst.ASSET_PATH_RAWIMAGE, image.name, parent);
            
            string assetPath = PSDImportUtility.baseDirectory + image.name + PSDImporterConst.PNG_SUFFIX;
            Texture2D texture = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;
            if (texture == null)
            {
                Debug.Log("loading asset at path: " + PSDImportUtility.baseDirectory + image.name);
            }

            pic.texture = texture as Texture;
            RectTransform rectTransform = pic.GetComponent<RectTransform>();
            PSDImportUtility.SetAnchorMiddleCenter(rectTransform);
            rectTransform.sizeDelta = new Vector2(image.size.width, image.size.height);
            rectTransform.anchoredPosition = new Vector2(image.position.x, image.position.y);
        }
    }
}
