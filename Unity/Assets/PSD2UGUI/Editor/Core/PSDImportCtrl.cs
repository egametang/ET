using UnityEditor;
using UnityEngine;
using System.IO;
#if UNITY_5_3
using UnityEditor.SceneManagement;
#endif

namespace PSDUIImporter
{
    public class PSDImportCtrl
    {
        private PSDUI psdUI;

        private IImageImport spriteImport;
        private IImageImport textImport;
        private IImageImport textureImport;
        private IImageImport slicedSpriteImport;
        private IImageImport halfSpriteImport;

        private ILayerImport buttonImport;
        private ILayerImport toggleImport;
        private ILayerImport panelImport;
        private ILayerImport scrollViewImport;
        private ILayerImport scrollBarImport;
        private ILayerImport sliderImport;
        private ILayerImport gridImport;
        private ILayerImport emptyImport;
        private ILayerImport groupImport;
        private ILayerImport inputFiledImport;
        private ILayerImport layoutElemLayerImport;

        public PSDImportCtrl(string xmlFilePath)
        {
            InitDataAndPath(xmlFilePath);
            InitCanvas();
            LoadLayers();
            MoveLayers();
            InitDrawers();
            PSDImportUtility.ParentDic.Clear();
        }

        public void DrawLayer(Layer layer, GameObject parent)
        {
            switch (layer.type)
            {
                case LayerType.Panel:
                    panelImport.DrawLayer(layer, parent);
                    break;
                case LayerType.Normal:
                    emptyImport.DrawLayer(layer, parent);
                    break;
                case LayerType.Button:
                    buttonImport.DrawLayer(layer, parent);
                    break;
                case LayerType.Toggle:
                    toggleImport.DrawLayer(layer, parent);
                    break;
                case LayerType.Grid:
                    gridImport.DrawLayer(layer, parent);
                    break;
                case LayerType.ScrollView:
                    scrollViewImport.DrawLayer(layer, parent);
                    break;
                case LayerType.Slider:
                    sliderImport.DrawLayer(layer, parent);
                    break;
                case LayerType.Group:
                    groupImport.DrawLayer(layer, parent);
                    break;
                case LayerType.InputField:
                    inputFiledImport.DrawLayer(layer, parent);
                    break;
                case LayerType.ScrollBar:
                    scrollBarImport.DrawLayer(layer, parent);
                    break;
                case LayerType.LayoutElement:
                    layoutElemLayerImport.DrawLayer(layer, parent);
                    break;
                default:
                    break;

            }
        }

        public void DrawLayers(Layer[] layers, GameObject parent)
        {
            if (layers != null)
            {
                for (int layerIndex = 0; layerIndex < layers.Length; layerIndex++)
                {
                    DrawLayer(layers[layerIndex], parent);
                }
            }
        }

        public void DrawImage(PSImage image, GameObject parent, GameObject ownObj = null)
        {
            switch (image.imageType)
            {
                case ImageType.Image:
                    spriteImport.DrawImage(image, parent, parent);
                    break;
                case ImageType.Texture:
                    textureImport.DrawImage(image, parent, parent);
                    break;
                case ImageType.Label:
                    textImport.DrawImage(image, parent, parent);
                    break;
                case ImageType.SliceImage:
                    slicedSpriteImport.DrawImage(image, parent, parent);
                    break;
                case ImageType.LeftHalfImage:
                    halfSpriteImport.DrawImage(image, parent);
                    break;
                case ImageType.BottomHalfImage:
                    halfSpriteImport.DrawImage(image, parent);
                    break;
                case ImageType.QuarterImage:
                    halfSpriteImport.DrawImage(image, parent);
                    break;
                default:
                    break;
            }
        }

        private void InitDataAndPath(string xmlFilePath)
        {
            psdUI = (PSDUI)PSDImportUtility.DeserializeXml(xmlFilePath, typeof(PSDUI));
            Debug.Log(psdUI.psdSize.width + "=====psdSize======" + psdUI.psdSize.height);
            if (psdUI == null)
            {
                Debug.Log("The file " + xmlFilePath + " wasn't able to generate a PSDUI.");
                return;
            }
#if UNITY_5_2
            if (EditorApplication.SaveCurrentSceneIfUserWantsTo() == false) { return; }
#elif UNITY_5_3
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() == false) { return; }
#endif
            PSDImportUtility.baseFilename = Path.GetFileNameWithoutExtension(xmlFilePath);
            PSDImportUtility.baseDirectory = "Assets/" + Path.GetDirectoryName(xmlFilePath.Remove(0, Application.dataPath.Length + 1)) + "/";
        }

        private void InitCanvas()
        {
#if UNITY_5_2
            EditorApplication.NewScene();
#elif UNITY_5_3
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
#endif
            Canvas temp = AssetDatabase.LoadAssetAtPath(PSDImporterConst.ASSET_PATH_CANVAS, typeof(Canvas)) as Canvas;
            PSDImportUtility.canvas = GameObject.Instantiate(temp) as Canvas;
            PSDImportUtility.canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            UnityEngine.UI.CanvasScaler scaler = PSDImportUtility.canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            scaler.referenceResolution = new Vector2(psdUI.psdSize.width, psdUI.psdSize.height);

            GameObject go = AssetDatabase.LoadAssetAtPath(PSDImporterConst.ASSET_PATH_EVENTSYSTEM, typeof(GameObject)) as GameObject;
            PSDImportUtility.eventSys = GameObject.Instantiate(go) as GameObject;
        }

        private void LoadLayers()
        {
            for (int layerIndex = 0; layerIndex < psdUI.layers.Length; layerIndex++)
            {
                ImportLayer(psdUI.layers[layerIndex], PSDImportUtility.baseDirectory);
            }
        }

        private void InitDrawers()
        {
            spriteImport = new SpriteImport();
            textImport = new TextImport();
            textureImport = new TextureImport();
            slicedSpriteImport = new SliceSpriteImport();
            halfSpriteImport = new HalfSpriteImport();


            buttonImport = new ButtonLayerImport(this);
            toggleImport = new ToggleLayerImport(this);
            panelImport = new PanelLayerImport(this);
            scrollViewImport = new ScrollViewLayerImport(this);
            scrollBarImport = new ScrollBarLayerImport(this);
            sliderImport = new SliderLayerImport(this);
            gridImport = new GridLayerImport(this);
            emptyImport = new DefultLayerImport(this);
            groupImport = new GroupLayerImport(this);
            inputFiledImport = new InputFieldLayerImport(this);
            layoutElemLayerImport = new LayoutElementLayerImport(this);
        }

        public void BeginDrawUILayers()
        {
            RectTransform obj = PSDImportUtility.LoadAndInstant<RectTransform>(PSDImporterConst.ASSET_PATH_EMPTY, PSDImportUtility.baseFilename, PSDImportUtility.canvas.gameObject);
            obj.offsetMin = Vector2.zero;
            obj.offsetMax = Vector2.zero;
            obj.anchorMin = Vector2.zero;
            obj.anchorMax = Vector2.one;

            for (int layerIndex = 0; layerIndex < psdUI.layers.Length; layerIndex++)
            {
                DrawLayer(psdUI.layers[layerIndex], obj.gameObject);
            }
            AssetDatabase.Refresh();
        }

        public void BeginSetUIParents()
        {
            foreach (var item in PSDImportUtility.ParentDic)
            {
                item.Key.SetParent(item.Value);
            }
        }


        private void MoveLayers()
        {
            for (int layerIndex = 0; layerIndex < psdUI.layers.Length; layerIndex++)
            {
                MoveAsset(psdUI.layers[layerIndex], PSDImportUtility.baseDirectory);
            }

            AssetDatabase.Refresh();
        }

        //--------------------------------------------------------------------------
        // private methods,按texture或image的要求导入图片到unity可加载的状态
        //-------------------------------------------------------------------------

        private void ImportLayer(Layer layer, string baseDirectory)
        {
            if (layer.image != null)
            {
                //for (int imageIndex = 0; imageIndex < layer.images.Length; imageIndex++)
                //{
                    // we need to fixup all images that were exported from PS
                    //PSImage image = layer.images[imageIndex];
                    PSImage image = layer.image;

                    if (image.imageType != ImageType.Label)
                    {
                        string texturePathName = PSDImportUtility.baseDirectory + image.name + PSDImporterConst.PNG_SUFFIX;

                        Debug.Log(texturePathName);
                        // modify the importer settings
                        TextureImporter textureImporter = AssetImporter.GetAtPath(texturePathName) as TextureImporter;

                        if (textureImporter != null && image.imageType != ImageType.Texture)           //Texture类型不设置属性
                        {
                            textureImporter.textureType = TextureImporterType.Sprite;
                            textureImporter.spriteImportMode = SpriteImportMode.Single;
                            textureImporter.mipmapEnabled = false;          //默认关闭mipmap
                            if(image.imageSource == ImageSource.Global)
                            {
                                textureImporter.spritePackingTag = PSDImporterConst.Globle_FOLDER_NAME;
                            }
                            else
                            {
                                textureImporter.spritePackingTag = PSDImportUtility.baseFilename;
                            }
                            
                            textureImporter.maxTextureSize = 2048;

                            if (image.imageType == ImageType.SliceImage)  //slice才需要设置border,可能需要根据实际修改border值
                            {
                                setSpriteBorder(textureImporter, image.arguments);
                                //textureImporter.spriteBorder = new Vector4(3, 3, 3, 3);   // Set Default Slice type  UnityEngine.UI.Image's border to Vector4 (3, 3, 3, 3)
                            }

                            AssetDatabase.WriteImportSettingsIfDirty(texturePathName);
                            AssetDatabase.ImportAsset(texturePathName);
                        }              
                    }
                //}
            }

            if (layer.layers != null)
            {
                //LoadLayers();
                for (int layerIndex = 0; layerIndex < layer.layers.Length; layerIndex++)
                {
                    ImportLayer(layer.layers[layerIndex], PSDImportUtility.baseDirectory);
                }
            }
        }
        //设置九宫格
        void setSpriteBorder(TextureImporter textureImporter,string[] args)
        {
            textureImporter.spriteBorder = new Vector4(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
        }

        //------------------------------------------------------------------
        //when it's a common psd, then move the asset to special folder
        //------------------------------------------------------------------
        private void MoveAsset(Layer layer, string baseDirectory)
        {
            if (layer.image != null)
            {
                string newPath = PSDImporterConst.Globle_BASE_FOLDER;
                if (layer.name == PSDImporterConst.IMAGE)
                {
                    newPath += PSDImporterConst.IMAGE + "/";
                }
                else if (layer.name == PSDImporterConst.NINE_SLICE)
                {
                    newPath += PSDImporterConst.NINE_SLICE + "/";
                }

                if (!System.IO.Directory.Exists(newPath))
                {
                    System.IO.Directory.CreateDirectory(newPath);
                    Debug.Log("creating new folder : " + newPath);
                }
                
                AssetDatabase.Refresh();

                //for (int imageIndex = 0; imageIndex < layer.images.Length; imageIndex++)
                //{
                    // we need to fixup all images that were exported from PS
                    PSImage image = layer.image;
                    if(image.imageSource == ImageSource.Global)
                    {
                        string texturePathName = PSDImportUtility.baseDirectory + image.name + PSDImporterConst.PNG_SUFFIX;
                        string targetPathName = newPath + image.name + PSDImporterConst.PNG_SUFFIX;

                        if (File.Exists(texturePathName))
                        {
                            AssetDatabase.MoveAsset(texturePathName, targetPathName);
                            Debug.Log(texturePathName);
                            Debug.Log(targetPathName);
                        }                           
                    }                 
                //}
            }

            if (layer.layers != null)
            {
                for (int layerIndex = 0; layerIndex < layer.layers.Length; layerIndex++)
                {
                    MoveAsset(layer.layers[layerIndex], PSDImportUtility.baseDirectory);
                }
            }
        }
    }
}