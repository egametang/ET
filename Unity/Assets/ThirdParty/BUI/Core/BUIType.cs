using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BUI
{
    public enum BUIType
    {
        Auto,
        Image,
        Text,
        RawImage,
        Button,
        ScrollRect,
        BUIRoot,
        Transform,
        GameObject,
    }

    public class BUITypeCatagory
    {
        //从上到下有自动搜索的优先级，所以要注意顺序
        public static Dictionary<BUIType, Type> Types = new Dictionary<BUIType, Type>()
        {
            { BUIType.Image ,typeof(Image)},
            { BUIType.Text ,typeof(Text)},
            { BUIType.RawImage ,typeof(RawImage)},
            { BUIType.Button ,typeof(Button)},
            { BUIType.ScrollRect ,typeof(ScrollRect)},
            { BUIType.BUIRoot ,typeof(BUIRoot)},
            { BUIType.Transform ,typeof(Transform)},
            { BUIType.GameObject ,typeof(GameObject)},
        };
    }
}
