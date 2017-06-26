using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace PSDUIImporter
{
    public class ScrollBarLayerImport : ILayerImport
    {
        PSDImportCtrl ctrl;
        public ScrollBarLayerImport(PSDImportCtrl ctrl)
        {
            this.ctrl = ctrl;
        }
        public void DrawLayer(Layer layer, GameObject parent)
        {
            UnityEngine.UI.Scrollbar temp = AssetDatabase.LoadAssetAtPath(PSDImporterConst.ASSET_PATH_SCROLLBAR, typeof(UnityEngine.UI.Scrollbar)) as UnityEngine.UI.Scrollbar;
            UnityEngine.UI.Scrollbar scrollBar = GameObject.Instantiate(temp) as UnityEngine.UI.Scrollbar;
            scrollBar.transform.SetParent(parent.transform, false); //parent = parent.transform;

            string type = layer.arguments[0].ToUpper();
            switch (type)
            {
                case "R":
                    scrollBar.direction = Scrollbar.Direction.RightToLeft;
                    break;
                case "L":
                    scrollBar.direction = Scrollbar.Direction.LeftToRight;
                    break;
                case "T":
                    scrollBar.direction = Scrollbar.Direction.TopToBottom;
                    break;
                case "B":
                    scrollBar.direction = Scrollbar.Direction.BottomToTop;
                    break;
                default:
                    break;
            }

            float pecent;
            if (float.TryParse(layer.arguments[1], out pecent))
            {
                scrollBar.size = pecent;
            }


            //for (int i = 0; i < layer.images.Length; i++)
            //{
                PSImage image = layer.image;
                string assetPath = PSDImportUtility.baseDirectory + image.name + PSDImporterConst.PNG_SUFFIX; Debug.Log("==  CommonImagePath  ====" + assetPath);
                Sprite sprite = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Sprite)) as Sprite;

                if (image.name.ToLower().Contains("background"))
                {
                    scrollBar.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
                    RectTransform rectTransform = scrollBar.GetComponent<RectTransform>();

                    rectTransform.sizeDelta = new Vector2(image.size.width, image.size.height);
                    rectTransform.anchoredPosition = new Vector2(image.position.x, image.position.y);
                }
                else if (image.name.ToLower().Contains("handle"))
                {
                    scrollBar.handleRect.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
                }
            //}
        }
    }
}
