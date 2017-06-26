using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace PSDUIImporter
{
    public class DefultLayerImport : ILayerImport
    {
        PSDImportCtrl ctrl;
        public DefultLayerImport(PSDImportCtrl ctrl)
        {
            this.ctrl = ctrl;
        }
        public void DrawLayer(Layer layer, GameObject parent)
        {
            RectTransform obj = PSDImportUtility.LoadAndInstant<RectTransform>(PSDImporterConst.ASSET_PATH_EMPTY, layer.name, parent);
            obj.offsetMin = Vector2.zero;
            obj.offsetMax = Vector2.zero;
            obj.anchorMin = Vector2.zero;
            obj.anchorMax = Vector2.one;

            RectTransform rectTransform = parent.GetComponent<RectTransform>();
            obj.sizeDelta = rectTransform.sizeDelta;
            obj.anchoredPosition = rectTransform.anchoredPosition;

            if (layer.image != null)
            {
                //for (int imageIndex = 0; imageIndex < layer.images.Length; imageIndex++)
                //{
                    PSImage image = layer.image;
                    ctrl.DrawImage(image, obj.gameObject);
                //}
            }

            ctrl.DrawLayers(layer.layers, obj.gameObject);
            //obj.transform.SetParent(parent.transform, false); //parent.transform;
        }
    }
}