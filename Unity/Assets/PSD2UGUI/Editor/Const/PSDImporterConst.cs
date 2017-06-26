using UnityEngine;
using System.Collections;
namespace PSDUIImporter
{
    public class PSDImporterConst
    {
        public const string BASE_FOLDER = "UI/";
        public const string PNG_SUFFIX = ".png";
        public const string Globle_BASE_FOLDER = "Assets/Textures/HomeCommon/";        //公用ui路径，按需修改
        public const string Globle_FOLDER_NAME = "HomeCommon";

        public const string RENDER = "Render";
        public const string NINE_SLICE = "9Slice";
        public const string IMAGE = "Image";

        //字体路径，按需修改
        public const string FONT_FOLDER = "Assets/PSD2UGUI/Font/";
        public const string FONT_STATIC_FOLDER = "Assets/PSD2UGUI/Font/StaticFont/";
        public const string FONT_SUFIX = ".ttf";

        //修改资源模板加载路径，不能放在resources目录
        public const string PSDUI_PATH = "Assets/PSD2UGUI/Template/UI/";
        public const string PSDUI_SUFFIX = ".prefab";

        public static string ASSET_PATH_EMPTY = PSDUI_PATH + "Empty" + PSDUI_SUFFIX;
        public static string ASSET_PATH_BUTTON = PSDUI_PATH + "Button" + PSDUI_SUFFIX;
        public static string ASSET_PATH_TOGGLE = PSDUI_PATH + "Toggle" + PSDUI_SUFFIX;
        public static string ASSET_PATH_CANVAS = PSDUI_PATH + "Canvas" + PSDUI_SUFFIX;
        public static string ASSET_PATH_EVENTSYSTEM = PSDUI_PATH + "EventSystem" + PSDUI_SUFFIX;
        public static string ASSET_PATH_GRID = PSDUI_PATH + "Grid" + PSDUI_SUFFIX;
        public static string ASSET_PATH_IMAGE = PSDUI_PATH + "Image" + PSDUI_SUFFIX;
        public static string ASSET_PATH_RAWIMAGE = PSDUI_PATH + "RawImage" + PSDUI_SUFFIX;
        public static string ASSET_PATH_HALFIMAGE = PSDUI_PATH + "HalfImage" + PSDUI_SUFFIX;
        public static string ASSET_PATH_SCROLLVIEW = PSDUI_PATH + "ScrollView" + PSDUI_SUFFIX;
        public static string ASSET_PATH_SLIDER = PSDUI_PATH + "Slider" + PSDUI_SUFFIX;
        public static string ASSET_PATH_TEXT = PSDUI_PATH + "Text" + PSDUI_SUFFIX;
        public static string ASSET_PATH_SCROLLBAR = PSDUI_PATH + "Scrollbar" + PSDUI_SUFFIX;
        public static string ASSET_PATH_GROUP_V = PSDUI_PATH + "VerticalGroup" + PSDUI_SUFFIX;
        public static string ASSET_PATH_GROUP_H = PSDUI_PATH + "HorizontalGroup" + PSDUI_SUFFIX;
        public static string ASSET_PATH_INPUTFIELD = PSDUI_PATH + "InputField" + PSDUI_SUFFIX;
        public static string ASSET_PATH_LAYOUTELEMENT = PSDUI_PATH + "LayoutElement" + PSDUI_SUFFIX;
    }
}