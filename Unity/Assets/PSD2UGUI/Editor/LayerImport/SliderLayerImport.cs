using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
namespace PSDUIImporter
{
    public class SliderLayerImport : ILayerImport
    {
        PSDImportCtrl ctrl;
        public SliderLayerImport(PSDImportCtrl ctrl)
        {
            this.ctrl = ctrl;
        }
        public void DrawLayer(Layer layer, GameObject parent)
        {
            //UnityEngine.UI.Slider temp = AssetDatabase.LoadAssetAtPath(PSDImporterConst.PREFAB_PATH_SLIDER, typeof(UnityEngine.UI.Slider)) as UnityEngine.UI.Slider;
            UnityEngine.UI.Slider slider = PSDImportUtility.LoadAndInstant<UnityEngine.UI.Slider>(PSDImporterConst.ASSET_PATH_SLIDER, layer.name, parent); //GameObject.Instantiate(temp) as UnityEngine.UI.Slider;

            RectTransform rectTransform = slider.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(layer.size.width, layer.size.height);
            rectTransform.anchoredPosition = new Vector2(layer.position.x, layer.position.y);

            //slider.transform.SetParent(parent.transform, true); //parent = parent.transform;

            PosLoader posloader = slider.gameObject.AddComponent<PosLoader>();
            posloader.worldPos = rectTransform.position;

            string type = layer.arguments[0].ToUpper();
            switch (type)
            {
                case "R":
                    slider.direction = Slider.Direction.RightToLeft;
                    break;
                case "L":
                    slider.direction = Slider.Direction.LeftToRight;
                    break;
                case "T":
                    slider.direction = Slider.Direction.TopToBottom;
                    break;
                case "B":
                    slider.direction = Slider.Direction.BottomToTop;
                    break;
                default:
                    break;
            }

            for (int i = 0; i < layer.layers.Length; i++)
            {
                PSImage image = layer.layers[i].image;
                string assetPath = PSDImportUtility.baseDirectory + image.name + PSDImporterConst.PNG_SUFFIX;
                Sprite sprite = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Sprite)) as Sprite;

                if (image.name.ToLower().Contains("bg"))
                {
                    var bgRect = slider.transform.Find("Background").GetComponent<RectTransform>();
                    var bgImage = bgRect.GetComponent<UnityEngine.UI.Image>();
                    if (image.imageType != ImageType.SliceImage)
                    {
                        bgImage.type = Image.Type.Simple;
                    }
                    bgImage.sprite = sprite;
                    bgRect.sizeDelta = new Vector2(image.size.width, image.size.height);
                }
                else if (image.name.ToLower().Contains("fill"))
                {
                    var fillImage = slider.fillRect.GetComponent<UnityEngine.UI.Image>();
                    if (image.imageType != ImageType.SliceImage)
                    {
                        fillImage.type = Image.Type.Simple;
                    }
                    fillImage.sprite = sprite;

                    var fillArea = slider.transform.Find("Fill Area").GetComponent<RectTransform>();
                    fillArea.sizeDelta = new Vector2(image.size.width, image.size.height);
                }
                else if (image.name.ToLower().Contains("handle"))       //默认没有handle
                {
                    var handleRectTrans = slider.transform.Find("Handle Slide Area/Handle").GetComponent<RectTransform>();
                    var handleSprite = handleRectTrans.GetComponent<UnityEngine.UI.Image>();
                    slider.handleRect = handleRectTrans;
                    handleSprite.sprite = sprite;

                    handleRectTrans.gameObject.SetActive(true);
                }
            }
        }
    }
}