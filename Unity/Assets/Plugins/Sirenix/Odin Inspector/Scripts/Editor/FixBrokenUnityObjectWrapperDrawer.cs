//-----------------------------------------------------------------------
// <copyright file="FixBrokenUnityObjectWrapperDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY_EDITOR && UNITY_2018_3_OR_NEWER
#pragma warning disable

namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [DrawerPriority(0.001, 0, 0)]
    public class FixBrokenUnityObjectWrapperDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems
        where T : UnityEngine.Component
    {
        private const string AUTO_FIX_PREFS_KEY = "TemporarilyBrokenUnityObjectWrapperDrawer.autoFix";

        private bool isBroken = false;
        private T realWrapperInstance;
        private bool allowSceneViewObjects;
        private static bool autoFix;

        protected override void Initialize()
        {
            this.allowSceneViewObjects = this.ValueEntry.Property.GetAttribute<AssetsOnlyAttribute>() == null;
            autoFix = EditorPrefs.HasKey(AUTO_FIX_PREFS_KEY);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (!(this.ValueEntry.ValueState == PropertyValueState.NullReference || this.ValueEntry.ValueState == PropertyValueState.ReferenceValueConflict))
            {
                this.CallNextDrawer(label);
                return;
            }

            if (Event.current.type == EventType.Layout)
            {
                this.isBroken = false;
                var count = this.ValueEntry.ValueCount;
                for (int i = 0; i < count; i++)
                {
                    var component = this.ValueEntry.Values[i];

                    if (ComponentIsBroken(component, ref this.realWrapperInstance))
                    {
                        this.isBroken = true;
                        break;
                    }
                }

                if (this.isBroken && autoFix)
                {
                    this.isBroken = false;

                    for (int i = 0; i < this.ValueEntry.ValueCount; i++)
                    {
                        T fixedComponent = null;
                        if (ComponentIsBroken(this.ValueEntry.Values[i], ref fixedComponent) && fixedComponent)
                        {
                            (this.ValueEntry as IValueEntryActualValueSetter<T>).SetActualValue(i, fixedComponent);
                        }
                    }

                    this.ValueEntry.Update();
                }
            }

            if (!this.isBroken)
            {
                this.CallNextDrawer(label);
                return;
            }

            var rect = EditorGUILayout.GetControlRect(label != null);
            var btnRect = rect.AlignRight(20);
            var controlRect = rect.SetXMax(btnRect.xMin - 5);

            object newInstance = null;

            EditorGUI.BeginChangeCheck();
            {
                if (this.ValueEntry.BaseValueType.IsInterface)
                {
                    newInstance = SirenixEditorFields.PolymorphicObjectField(controlRect,
                        label,
                        this.realWrapperInstance,
                        this.ValueEntry.BaseValueType,
                        this.allowSceneViewObjects);
                }
                else
                {
                    newInstance = SirenixEditorFields.UnityObjectField(
                        controlRect,
                        label,
                        this.realWrapperInstance,
                        this.ValueEntry.BaseValueType,
                        this.allowSceneViewObjects) as Component;
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                this.ValueEntry.WeakSmartValue = newInstance;
            }

            if (GUI.Button(btnRect, " ", EditorStyles.miniButton))
            {
                var popup = new FixBrokenUnityObjectWrapperPopup(this.ValueEntry);
                OdinEditorWindow.InspectObjectInDropDown(popup, 300);
            }

            if (Event.current.type == EventType.Repaint)
            {
                GUI.DrawTexture(btnRect, EditorIcons.ConsoleWarnicon, ScaleMode.ScaleToFit);
            }
        }

        private static bool ComponentIsBroken(T component, ref T realInstance)
        {
            var uObj = component;
            var oObj = (object)uObj;

            if (oObj != null && uObj == null)
            {
                var instanceId = uObj.GetInstanceID();
                if (AssetDatabase.Contains(instanceId))
                {
                    var path = AssetDatabase.GetAssetPath(instanceId);
                    var realWrapper = AssetDatabase.LoadAllAssetsAtPath(path).FirstOrDefault(n => n.GetInstanceID() == instanceId) as T;
                    if (realWrapper)
                    {
                        realInstance = realWrapper;
                        return true;
                    }
                }
            }

            return false;
        }

        public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            if (EditorPrefs.HasKey(AUTO_FIX_PREFS_KEY))
            {
                genericMenu.AddItem(new GUIContent("Disable auto-fix of broken prefab instance references"), false, (x) =>
                {
                    EditorPrefs.DeleteKey(AUTO_FIX_PREFS_KEY);
                    autoFix = false;
                }, null);
            }
        }

        [TypeInfoBox("This asset reference is temporarily broken until the next reload, because of an error in Unity where the C# wrapper object of a prefab asset is destroyed when changes are made to that prefab asset. This error has been reported to Unity.\n\nMeanwhile, Odin can fix this for you by getting a new, valid wrapper object from the asset database and replacing the broken wrapper instance with the new one.")]
        private class FixBrokenUnityObjectWrapperPopup
        {
            private IPropertyValueEntry<T> valueEntry;

            public FixBrokenUnityObjectWrapperPopup(IPropertyValueEntry<T> valueEntry)
            {
                this.valueEntry = valueEntry;
            }

            [HorizontalGroup, Button(ButtonSizes.Large)]
            public void FixItThisTime()
            {
                for (int i = 0; i < this.valueEntry.ValueCount; i++)
                {
                    var localI = i;
                    T fixedComponent = null;
                    if (ComponentIsBroken(this.valueEntry.Values[i], ref fixedComponent) && fixedComponent)
                    {
                        this.valueEntry.Property.Tree.DelayActionUntilRepaint(() =>
                        {
                            (this.valueEntry as IValueEntryActualValueSetter<T>).SetActualValue(localI, fixedComponent);
                        });
                    }
                }

                if (GUIHelper.CurrentWindow) 
                {
                    EditorApplication.delayCall += GUIHelper.CurrentWindow.Close;
                }
            }

            [HorizontalGroup, Button(ButtonSizes.Large)]
            public void FixItAlways()
            {
                EditorPrefs.SetBool(AUTO_FIX_PREFS_KEY, true);
                autoFix = true;

                if (GUIHelper.CurrentWindow) 
                {
                    EditorApplication.delayCall += GUIHelper.CurrentWindow.Close;
                }
            }
        }
    }
}

#endif