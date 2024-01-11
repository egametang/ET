using UnityEditor;

namespace I2.Loc
{
    [CustomEditor(typeof(LanguageSourceAsset))]
    public class LanguageSourceAssetInspector : LocalizationEditor
    {
        void OnEnable()
        {
            var newSource = target as LanguageSourceAsset;
            SerializedProperty propSource = serializedObject.FindProperty("mSource");

            Custom_OnEnable(newSource.mSource, propSource);
        }
        public override LanguageSourceData GetSourceData()
        {
            return (target as LanguageSourceAsset).mSource;
        }
    }
}