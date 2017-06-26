using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace PSDUIImporter
{
    public class SliceSpriteImport : IImageImport
    {
        public void DrawImage(PSImage image, GameObject parent, GameObject ownObj)
        {
            //UnityEngine.UI.Image pic = AssetDatabase.LoadAssetAtPath(PSDImporterConst.PREFAB_PATH_IMAGE, typeof(UnityEngine.UI.Image)) as UnityEngine.UI.Image;
            //string commonImagePath = PSDImporterConst.Globle_BASE_FOLDER + image.name.Replace(".", "/") + PSDImporterConst.PNG_SUFFIX;
            //Debug.Log("==  CommonImagePath  ====" + commonImagePath);
            //Sprite sprite = AssetDatabase.LoadAssetAtPath(commonImagePath, typeof(Sprite)) as Sprite;

            UnityEngine.UI.Image pic;
            if (ownObj != null)
                pic = ownObj.AddComponent<UnityEngine.UI.Image>();
            else
                pic = PSDImportUtility.LoadAndInstant<UnityEngine.UI.Image>(PSDImporterConst.ASSET_PATH_IMAGE, image.name, parent);

            RectTransform rectTransform = pic.GetComponent<RectTransform>();
            PSDImportUtility.SetAnchorMiddleCenter(rectTransform);
            string assetPath = PSDImportUtility.baseDirectory + image.name + PSDImporterConst.PNG_SUFFIX;
            Sprite sprite = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Sprite)) as Sprite;
            if (sprite == null)
            {
                Debug.Log("loading asset at path: " + PSDImportUtility.baseDirectory + image.name);
            }

            //string assetPath = PSDImportUtility.baseDirectory + image.name + PSDImporterConst.PNG_SUFFIX;
            //Sprite sprite = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Sprite)) as Sprite;
            //UnityEngine.UI.Image myImage = GameObject.Instantiate(pic) as UnityEngine.UI.Image;

            pic.sprite = sprite;
            pic.type = UnityEngine.UI.Image.Type.Sliced;

            //RectTransform rectTransform = pic.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(image.size.width, image.size.height);
            rectTransform.anchoredPosition = new Vector2(image.position.x, image.position.y);
        }
    }
}
