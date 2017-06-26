using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace PSDUIImporter
{
    //arguments:行数,列数,render width, render height,spacingx ,spacingy
    public class GridLayerImport : ILayerImport
    {
        PSDImportCtrl ctrl;
        public GridLayerImport(PSDImportCtrl ctrl)
        {
            this.ctrl = ctrl;
        }
        public void DrawLayer(Layer layer, GameObject parent)
        {
            UnityEngine.UI.GridLayoutGroup gridLayoutGroup = PSDImportUtility.LoadAndInstant<UnityEngine.UI.GridLayoutGroup>(PSDImporterConst.ASSET_PATH_GRID, layer.name, parent);            

            RectTransform rectTransform = gridLayoutGroup.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(layer.size.width, layer.size.height);
            rectTransform.anchoredPosition = new Vector2(layer.position.x, layer.position.y);

            gridLayoutGroup.padding = new RectOffset();

            float width, height;
            if (float.TryParse(layer.arguments[2], out width) && float.TryParse(layer.arguments[3], out height))
            {
                gridLayoutGroup.cellSize = new Vector2(width, height);
            }         
            gridLayoutGroup.spacing = new Vector2(System.Convert.ToInt32(layer.arguments[4]), System.Convert.ToInt32(layer.arguments[5]));

            ctrl.DrawLayers(layer.layers, gridLayoutGroup.gameObject);
        }
    }
}
