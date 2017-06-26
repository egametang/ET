using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;



namespace PSDUIImporter
{
    public class SpriteImport : IImageImport
    {
        public void DrawImage(PSImage image, GameObject parent, GameObject ownObj = null)
        {
            if (image.imageSource == ImageSource.Common || image.imageSource == ImageSource.Custom)
            {
                UnityEngine.UI.Image pic;
                if (ownObj != null)
                    pic = ownObj.AddComponent<UnityEngine.UI.Image>();
                else
                    pic = PSDImportUtility.LoadAndInstant<UnityEngine.UI.Image>(PSDImporterConst.ASSET_PATH_IMAGE, image.name, parent);

                RectTransform rectTransform = pic.GetComponent<RectTransform>();
                rectTransform.offsetMin = new Vector2(0.5f, 0.5f);
                rectTransform.offsetMax = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

                string assetPath = PSDImportUtility.baseDirectory + image.name + PSDImporterConst.PNG_SUFFIX;
                Sprite sprite = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Sprite)) as Sprite;

                if (sprite == null)
                {
                    Debug.Log("loading asset at path: " + PSDImportUtility.baseDirectory + image.name);
                }

                pic.sprite = sprite;

                rectTransform.sizeDelta = new Vector2(image.size.width, image.size.height);
                rectTransform.anchoredPosition = new Vector2(image.position.x, image.position.y);
            }
            else if (image.imageSource == ImageSource.Global)
            {
                UnityEngine.UI.Image pic;
                if (ownObj != null)
                    pic = ownObj.AddComponent<UnityEngine.UI.Image>();
                else
                    pic = PSDImportUtility.LoadAndInstant<UnityEngine.UI.Image>(PSDImporterConst.ASSET_PATH_IMAGE, image.name, parent); 

                RectTransform rectTransform = pic.GetComponent<RectTransform>();
                rectTransform.offsetMin = new Vector2(0.5f, 0.5f);
                rectTransform.offsetMax = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

                string commonImagePath = PSDImporterConst.Globle_BASE_FOLDER + image.name.Replace(".", "/") + PSDImporterConst.PNG_SUFFIX;
                Debug.Log("==  CommonImagePath  ====" + commonImagePath);
                Sprite sprite = AssetDatabase.LoadAssetAtPath(commonImagePath, typeof(Sprite)) as Sprite;
                pic.sprite = sprite;

                pic.name = image.name;

                if (image.imageType == ImageType.SliceImage)
                {
                    pic.type = UnityEngine.UI.Image.Type.Sliced;
                }

                //RectTransform rectTransform = pic.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(image.size.width, image.size.height);
                rectTransform.anchoredPosition = new Vector2(image.position.x, image.position.y);
            }
        }
    }
}
