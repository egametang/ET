using System;

namespace YooAsset.Editor
{
    public class WindowsDefine
    {
#if UNITY_2019_4_OR_NEWER
        /// <summary>
        /// 停靠窗口类型集合
        /// </summary>
        public static readonly Type[] DockedWindowTypes =
        {
            typeof(AssetBundleBuilderWindow),
            typeof(AssetBundleCollectorWindow),
            typeof(AssetBundleDebuggerWindow),
            typeof(AssetBundleReporterWindow)
        };
#endif
    }

    /// <summary>
    /// 资源搜索类型
    /// </summary>
    public enum EAssetSearchType
    {
        All,
        RuntimeAnimatorController,
        AnimationClip,
        AudioClip,
        AudioMixer,
        Font,
        Material,
        Mesh,
        Model,
        PhysicMaterial,
        Prefab,
        Scene,
        Script,
        Shader,
        Sprite,
        Texture,
        VideoClip,
    }

    /// <summary>
    /// 资源文件格式
    /// </summary>
    public enum EAssetFileExtension
    {
        prefab,
        unity,
        fbx,
        anim,
        controller,
        png,
        jpg,
        mat,
        shader,
        ttf,
        cs,
    }
}