using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace PSDUIImporter
{
    public class ScrollViewLayerImport : ILayerImport
    {
        PSDImportCtrl ctrl;
        public ScrollViewLayerImport(PSDImportCtrl ctrl)
        {
            this.ctrl = ctrl;
        }
        public void DrawLayer(Layer layer, GameObject parent)
        {
            //UnityEngine.UI.ScrollRect temp = AssetDatabase.LoadAssetAtPath(PSDImporterConst.PREFAB_PATH_SCROLLVIEW, typeof(UnityEngine.UI.ScrollRect)) as UnityEngine.UI.ScrollRect;
            UnityEngine.UI.ScrollRect scrollRect = PSDImportUtility.LoadAndInstant<UnityEngine.UI.ScrollRect>(PSDImporterConst.ASSET_PATH_SCROLLVIEW, layer.name, parent);
            //scrollRect.transform.SetParent(parent.transform, false); //parent = parent.transform;


            RectTransform rectTransform = scrollRect.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(layer.size.width, layer.size.height);
            rectTransform.anchoredPosition = new Vector2(layer.position.x, layer.position.y);

            if (layer.layers != null)
            {
                string type = layer.arguments[0].ToUpper();
                switch (type)
                {
                    case "V":
                        //scrollRect.vertical = true;
                        //scrollRect.horizontal = false;
                        BuildVerticalScrollView(scrollRect, layer);
                        break;
                    case "H":
                        //scrollRect.vertical = false;
                        //scrollRect.horizontal = true;
                        BuildHorizonScrollView(scrollRect, layer);
                        break;
                    default:
                        break;
                }

                ctrl.DrawLayers(layer.layers, scrollRect.content.gameObject);
            }
        }

        /// <summary>
        /// 构建水平滑动，主要是添加自动布局
        /// </summary>
        /// <param name="scrollRect"></param>
        /// <param name="layer"></param>
        public void BuildHorizonScrollView(UnityEngine.UI.ScrollRect scrollRect,Layer layer)
        {
            scrollRect.vertical = false;
            scrollRect.horizontal = true;

            //水平默认从左向右滑动
            scrollRect.content.anchorMin = Vector2.zero;
            scrollRect.content.anchorMax = new Vector2(0,1);
            scrollRect.content.pivot = new Vector2(0,0.5f);         //锚定左侧，否则动态增长时会从两边添加cell
           
            var contentSizeFilter = scrollRect.content.GetComponent<ContentSizeFitter>();
            contentSizeFilter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFilter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var hLayout = scrollRect.content.gameObject.AddComponent<HorizontalLayoutGroup>();  //添加水平布局组件
            if (layer.arguments.Length < 4)
            {
                Debug.LogWarning("ScrollView arguments error !");
            }
            else
            {
                float spacing;
                if (float.TryParse(layer.arguments[1], out spacing))
                {
                    hLayout.spacing = spacing;
                }
                int left, top;
                if (int.TryParse(layer.arguments[2], out left) && int.TryParse(layer.arguments[3], out top))
                {
                    hLayout.padding = new RectOffset(left, left, top, top);
                }
            }                      
        }

        /// <summary>
        /// 构建垂直滑动
        /// </summary>
        /// <param name="scrollRect"></param>
        /// <param name="layer"></param>
        public void BuildVerticalScrollView(UnityEngine.UI.ScrollRect scrollRect, Layer layer)
        {
            scrollRect.vertical = true;
            scrollRect.horizontal = false;

            //垂直默认从上向下滑动
            scrollRect.content.anchorMin = new Vector2(0,1);
            scrollRect.content.anchorMax = Vector2.one;
            scrollRect.content.pivot = new Vector2(0.5f, 1);         //锚定上侧，否则动态增长时会从两边添加cell

            var contentSizeFilter = scrollRect.content.GetComponent<ContentSizeFitter>();
            contentSizeFilter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFilter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var vLayout = scrollRect.content.gameObject.AddComponent<VerticalLayoutGroup>();  //添加水平布局组件
            if (layer.arguments.Length < 4)
            {
                Debug.LogWarning("ScrollView arguments error !");
            }
            else
            {
                float spacing;
                if (float.TryParse(layer.arguments[1], out spacing))
                {
                    vLayout.spacing = spacing;
                }
                int left, top;
                if (int.TryParse(layer.arguments[2], out left) && int.TryParse(layer.arguments[3], out top))
                {
                    vLayout.padding = new RectOffset(left, left, top, top);
                }
            }
        }

    }
}