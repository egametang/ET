using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace HybridCLR.Editor.Settings
{
    public class SettingsPresetReceiver : PresetSelectorReceiver
    {
        private Object m_Target;
        private Preset m_InitialValue;
        private SettingsProvider m_Provider;

        internal void Init(Object target, SettingsProvider provider)
        {
            m_Target = target;
            m_InitialValue = new Preset(target);
            m_Provider = provider;
        }
        public override void OnSelectionChanged(Preset selection)
        {
            if (selection != null)
            {
                Undo.RecordObject(m_Target, "Apply Preset " + selection.name);
                selection.ApplyTo(m_Target);
            }
            else
            {
                Undo.RecordObject(m_Target, "Cancel Preset");
                m_InitialValue.ApplyTo(m_Target);
            }
            m_Provider.Repaint();
        }
        public override void OnSelectionClosed(Preset selection)
        {
            OnSelectionChanged(selection);
            Object.DestroyImmediate(this);
        }
    }
}