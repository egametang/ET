using System;
using UnityEngine;
using UnityEditor;

namespace PSDUIImporter
{
    internal class InputFieldLayerImport : ILayerImport
    {
        private PSDImportCtrl pSDImportCtrl;

        public InputFieldLayerImport(PSDImportCtrl pSDImportCtrl)
        {
            this.pSDImportCtrl = pSDImportCtrl;
        }

        public void DrawLayer(Layer layer, GameObject parent)
        {
            UnityEngine.UI.InputField temp = AssetDatabase.LoadAssetAtPath(PSDImporterConst.ASSET_PATH_INPUTFIELD, typeof(UnityEngine.UI.InputField)) as UnityEngine.UI.InputField;
            UnityEngine.UI.InputField inputfield = GameObject.Instantiate(temp) as UnityEngine.UI.InputField;
            inputfield.transform.SetParent(parent.transform, false);//.parent = parent.transform;
            inputfield.name = layer.name;

            if (layer.image != null)
            {
                //for (int imageIndex = 0; imageIndex < layer.images.Length; imageIndex++)
                //{
                    PSImage image = layer.image;

                    if (image.imageType == ImageType.Label)
                    {
                        if (image.name.ToLower().Contains("text"))
                        {
                            UnityEngine.UI.Text text = (UnityEngine.UI.Text)inputfield.textComponent;//inputfield.transform.Find("Text").GetComponent<UnityEngine.UI.Text>();
                            Color color;
                            if (UnityEngine.ColorUtility.TryParseHtmlString(("#" + image.arguments[0]), out color))
                            {
                                text.color = color;
                            }

                            int size;
                            if (int.TryParse(image.arguments[2], out size))
                            {
                                text.fontSize = size;
                            }
                        }
                        else if (image.name.ToLower().Contains("holder"))
                        {
                            UnityEngine.UI.Text text = (UnityEngine.UI.Text)inputfield.placeholder;//.transform.Find("Placeholder").GetComponent<UnityEngine.UI.Text>();
                            Color color;
                            if (UnityEngine.ColorUtility.TryParseHtmlString(("#" + image.arguments[0]), out color))
                            {
                                text.color = color;
                            }

                            int size;
                            if (int.TryParse(image.arguments[2], out size))
                            {
                                text.fontSize = size;
                            }
                        }
                    }
                    else
                    {
                        if (image.name.ToLower().Contains("background"))
                        {
                            if (image.imageSource == ImageSource.Common || image.imageSource == ImageSource.Custom)
                            {
                                string assetPath = PSDImportUtility.baseDirectory + image.name + PSDImporterConst.PNG_SUFFIX;
                                Sprite sprite = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Sprite)) as Sprite;
                                inputfield.image.sprite = sprite;

                                RectTransform rectTransform = inputfield.GetComponent<RectTransform>();
                                rectTransform.sizeDelta = new Vector2(image.size.width, image.size.height);
                                rectTransform.anchoredPosition = new Vector2(image.position.x, image.position.y);
                            }
                        }
                    }
                //}
            }
        }
    }
}