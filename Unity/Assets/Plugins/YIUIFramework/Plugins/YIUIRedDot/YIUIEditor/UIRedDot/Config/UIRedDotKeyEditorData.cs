#if UNITY_EDITOR

using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace YIUIFramework.Editor
{
    [Serializable]
    internal class UIRedDotKeyEditorData
    {
        [HideLabel]
        [OnValueChanged("OnValueChangedKeyType")]
        [OdinSerialize]
        public ERedDotKeyType KeyType;

        [HideLabel]
        [ReadOnly]
        [TableColumnWidth(100, resizable: false)]
        [OdinSerialize]
        public int Id;

        private UIRedDotConfigEditorData m_UIRedDotConfigEditorData;

        public UIRedDotKeyEditorData(UIRedDotConfigEditorData editorData)
        {
            m_UIRedDotConfigEditorData = editorData;
        }

        private void OnValueChangedKeyType()
        {
            Id = (int)KeyType;
            m_UIRedDotConfigEditorData?.CheckParentList();
        }
    }
}
#endif