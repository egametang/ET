//-----------------------------------------------------------------------
// <copyright file="ValidationOverview.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinValidator.Editor
{
    using Sirenix.OdinInspector.Editor;
    using Sirenix.OdinInspector.Editor.Validation;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public class ValidationOverview
    {
        private static readonly DisplayOptions[] AllDisplayOptions = Enum.GetValues(typeof(DisplayOptions)).Cast<DisplayOptions>().Where(n => n != DisplayOptions.None).ToArray();

        public string Title;
        public List<ValidationProfileResult> ProfileResults = new List<ValidationProfileResult>();
        public event Action<ValidationProfileResult> OnProfileResultSelected;
        private DisplayOptions? _display;
        public DisplayOptions Display
        {
            get
            {
                if (!_display.HasValue)
                {
                    if (EditorPrefs.HasKey("OdinValidatorDisplayColumns_v2"))
                    {
                        _display = (DisplayOptions)EditorPrefs.GetInt("OdinValidatorDisplayColumns_v2");
                    }
                    else
                    {
                        _display = DisplayOptions.Message | DisplayOptions.Object | DisplayOptions.Category | DisplayOptions.PropertyPath | DisplayOptions.Path | DisplayOptions.Validator;
                        EditorPrefs.SetInt("OdinValidatorDisplayColumns_v2", (int)_display);
                    }
                }

                return _display.Value;
            }
            set
            {
                _display = value;
                EditorPrefs.SetInt("OdinValidatorDisplayColumns_v2", (int)_display);
            }
        }
        public OdinMenuTree Tree;
        public DisplayOptions SortBy;
        public bool SortAscending;

        [Flags]
        public enum DisplayOptions
        {
            [HideInInspector]
            None = 0,
            Category = 1 << 0,
            Message = 1 << 1,
            Path = 1 << 2,
            PropertyPath = 1 << 3,
            Validator = 1 << 4,
            Object = 1 << 5,
            Scene = 1 << 6,
        }

        private Dictionary<DisplayOptions, ResizableColumn> columnLookup = new Dictionary<DisplayOptions, ResizableColumn>();
        private ResizableColumn[] columns;
        private static GUIStyle EnumBtnStyle;
        private DisplayOptions? nextDisplay;
        private bool shouldSort;

        public ValidationOverview()
        {
            this.Update();
        }

        public void ResetSortingSettings()
        {
            this.SortBy = DisplayOptions.None;
            this.SortAscending = false;
        }

        public void Update()
        {
            var displayOptions = AllDisplayOptions.Where(option => (this.Display & option) == option).ToList();

            // Create/adjust columns
            if (this.columns == null)
            {
                this.columns = new ResizableColumn[displayOptions.Count];
            }
            else
            {
                Array.Resize(ref this.columns, displayOptions.Count);
            }

            for (int i = 0; i < displayOptions.Count; i++)
            {
                this.columns[i] = this.GetColumn(displayOptions[i]);
            }

            // Create tree
            {
                this.Tree = new OdinMenuTree();
                this.Tree.Config.AutoHandleKeyboardNavigation = true;
                this.Tree.Config.UseCachedExpandedStates = false;
                this.Tree.Config.EXPERIMENTAL_INTERNAL_DrawFlatTreeFastNoLayout = true;
                this.Tree.DefaultMenuStyle = OdinMenuStyle.TreeViewStyle;

                int itemCount = 0;

                foreach (var profileResult in this.ProfileResults)
                {
                    foreach (var validationResult in profileResult.Results)
                    {
                        if (validationResult.ResultType != ValidationResultType.Error && validationResult.ResultType != ValidationResultType.Warning) continue;

                        var menuItem = new ValidationInfoMenuItem(this.Tree, validationResult, profileResult, this.columns, this.Display, itemCount++);
                        this.Tree.MenuItems.Add(menuItem);
                    }
                }

                this.Tree.Selection.SelectionChanged += changeEvent =>
                {
                    if (changeEvent != SelectionChangedType.ItemAdded) return;

                    if (this.OnProfileResultSelected != null)
                    {
                        var menuItem = this.Tree.Selection.Last() as ValidationInfoMenuItem;
                        var profileResult = menuItem.ProfileResult;

                        this.OnProfileResultSelected(profileResult);
                    }
                };
            }

            if (this.SortBy != DisplayOptions.None)
                this.Sort();
        }

        public void DrawOverview()
        {
            if (Event.current.type == EventType.Layout && this.nextDisplay != null)
            {
                this.Display = this.nextDisplay.Value;
                this.nextDisplay = null;
                this.Update();
            }

            if (Event.current.type == EventType.Layout && this.shouldSort)
            {
                this.shouldSort = false;
                this.Sort();
            }

            EnumBtnStyle = EnumBtnStyle ?? new GUIStyle(EditorStyles.toolbarDropDown);
            EnumBtnStyle.stretchHeight = true;
            EnumBtnStyle.fixedHeight = this.Tree.Config.SearchToolbarHeight;

            GUILayout.BeginHorizontal(GUILayoutOptions.ExpandHeight());
            {
                var rect = GUIHelper.GetCurrentLayoutRect();
                var columnRect = rect.AddYMin(this.Tree.Config.SearchToolbarHeight);

                GUILayout.BeginVertical(GUILayoutOptions.Width(rect.width).ExpandHeight());
                {
                    EditorGUI.DrawRect(columnRect.AddYMin(this.Tree.Config.DefaultMenuStyle.Height), SirenixGUIStyles.EditorWindowBackgroundColor);

                    GUILayout.BeginHorizontal();
                    this.Tree.DrawSearchToolbar();
                    var displayRect = GUILayoutUtility.GetRect(95, this.Tree.Config.SearchToolbarHeight, GUILayoutOptions.Width(95));
                    displayRect.height = this.Tree.Config.SearchToolbarHeight;
                    displayRect.width -= 1;

                    var newDisplay = EnumSelector<DisplayOptions>.DrawEnumField(displayRect, null, GUIHelper.TempContent("Data Columns"), this.Display, EnumBtnStyle);
                    if (newDisplay != this.Display)
                    {
                        this.nextDisplay = newDisplay;
                    }

                    GUILayout.EndHorizontal();

                    GUITableUtilities.ResizeColumns(columnRect, this.columns);
                    this.DrawColumnHeaders();
                    this.Tree.DrawMenuTree();
                }

                GUILayout.EndVertical();
                GUITableUtilities.DrawColumnHeaderSeperators(columnRect, this.columns, SirenixGUIStyles.BorderColor);

                GUILayout.Space(-5);
            }
            GUILayout.EndHorizontal();
        }

        private void Sort()
        {
            // We can't just use list.Sort(), because we want a stable sort - LINQ's OrderBy provides that
            var items = this.Tree.MenuItems.OrderBy(a => (ValidationInfoMenuItem)a, new ItemComparer(this.SortBy, this.SortAscending)).ToList();
            this.Tree.MenuItems.Clear();
            this.Tree.MenuItems.AddRange(items);
            this.Tree.MarkDirty();
        }

        private class ItemComparer : IComparer<ValidationInfoMenuItem>
        {
            public ItemComparer(DisplayOptions option, bool ascending)
            {
                this.Option = option;
                this.Ascending = ascending;
            }

            public DisplayOptions Option;
            public bool Ascending;

            public int Compare(ValidationInfoMenuItem a, ValidationInfoMenuItem b)
            {
                var result = this.DoCompare(a, b);
                return this.Ascending ? -result : result;
            }

            private int DoCompare(ValidationInfoMenuItem a, ValidationInfoMenuItem b)
            {
                switch (this.Option)
                {
                    case DisplayOptions.Message:
                    case DisplayOptions.Path:
                    case DisplayOptions.PropertyPath:
                    case DisplayOptions.Object:
                    case DisplayOptions.Scene:
                        return NullSafeCompare(a.GetDisplayString(this.Option), b.GetDisplayString(this.Option));
                    case DisplayOptions.Validator:
                        {
                            if (a.ValidationResult.Setup.Validator != null && b.ValidationResult.Setup.Validator == null) return 1;
                            if (a.ValidationResult.Setup.Validator == null && b.ValidationResult.Setup.Validator != null) return -1;
                            if (a.ValidationResult.Setup.Validator == null && b.ValidationResult.Setup.Validator == null) return 0;

                            return a.ValidationResult.Setup.Validator.GetType().GetNiceName().CompareTo(b.ValidationResult.Setup.Validator.GetType().GetNiceName());
                        }
                    case DisplayOptions.Category:
                        return a.ValidationResult.ResultType.CompareTo(b.ValidationResult.ResultType);
                    default:
                        return a.OriginalItemIndex.CompareTo(b.OriginalItemIndex);
                }
            }

            private static int NullSafeCompare<T>(T a, T b) where T : class, IComparable<T>
            {
                if (a != null && b == null) return 1;
                if (a == null && b != null) return -1;
                if (a == null && b == null) return 0;

                return a.CompareTo(b);
            }
        }

        private void DrawColumnHeaders()
        {
            var columnsRect = GUILayoutUtility.GetRect(0, this.Tree.Config.DefaultMenuStyle.Height, GUILayoutOptions.ExpandWidth(true));

            EditorGUI.DrawRect(columnsRect, SirenixGUIStyles.DarkEditorBackground);

            //SirenixGUIStyles.Temporary.Draw(columnsRect, GUIContent.none, 0);

            int columnIndex = 0;
            float currentX = columnsRect.xMin;

            for (int i = 0; i < AllDisplayOptions.Length; i++)
            {
                var option = AllDisplayOptions[i];

                if ((this.Display & option) == option)
                {
                    var width = this.columns[columnIndex].ColWidth;
                    var rect = new Rect(currentX, columnsRect.yMin + 3, width - 0.5f, columnsRect.height);

                    rect.xMax = Math.Min(rect.xMax, columnsRect.xMax);

                    if (rect.width <= 0) break;

                    string labelText;
                    
                    if (option == DisplayOptions.Category)
                    {
                        labelText = "";
                    }
                    else if (option == DisplayOptions.PropertyPath)
                    {
                        labelText = "Property Path";
                    }
                    else
                    {
                        labelText = option.ToString();
                    }

                    if (GUI.Button(rect, GUIHelper.TempContent(labelText), SirenixGUIStyles.BoldLabel))
                    {
                        if (this.SortBy == option)
                        {
                            this.SortAscending = !this.SortAscending;
                        }
                        else
                        {
                            this.SortBy = option;
                            this.SortAscending = false;
                        }

                        this.shouldSort = true;
                    }

                    var iconRect = rect.AlignRight(rect.height).Padding(3).SubY(3);
                    EditorIcon icon;

                    if (this.SortBy != option)
                    {
                        icon = EditorIcons.TriangleRight;
                        GUIHelper.PushColor(GUI.color * 0.7f);
                    }
                    else
                    {
                        icon = this.SortAscending ? EditorIcons.TriangleUp : EditorIcons.TriangleDown;
                    }

                    icon.Draw(iconRect);

                    if (this.SortBy != option)
                    {
                        GUIHelper.PopColor();
                    }

                    currentX += width;
                    columnIndex++;
                }
            }

            SirenixEditorGUI.DrawHorizontalLineSeperator(columnsRect.xMin, columnsRect.yMax, columnsRect.width, 0.5f);
        }

        private ResizableColumn GetColumn(DisplayOptions option)
        {
            ResizableColumn result;
            if (!this.columnLookup.TryGetValue(option, out result))
            {
                switch (option)
                {
                    case DisplayOptions.Category:
                        result = ResizableColumn.FixedColumn(23);
                        break;
                    case DisplayOptions.Message:
                        result = ResizableColumn.DynamicColumn(250, 50);
                        break;
                    case DisplayOptions.Path:
                        result = ResizableColumn.FlexibleColumn(150, 50);
                        break;
                    case DisplayOptions.PropertyPath:
                        result = ResizableColumn.FlexibleColumn(150, 50);
                        break;
                    case DisplayOptions.Validator:
                        result = ResizableColumn.FlexibleColumn(150, 50);
                        break;
                    case DisplayOptions.Object:
                        result = ResizableColumn.FlexibleColumn(150, 50);
                        break;
                    case DisplayOptions.Scene:
                        result = ResizableColumn.FlexibleColumn(150, 50);
                        break;
                    default:
                        result = ResizableColumn.FlexibleColumn(150, 50);
                        break;
                }

                this.columnLookup[option] = result;
            }

            return result;
        }

        private class ValidationInfoMenuItem : OdinMenuItem
        {
            public ValidationResult ValidationResult;
            public ValidationProfileResult ProfileResult;
            public ResizableColumn[] Columns;
            public DisplayOptions DisplayOptions;
            public int OriginalItemIndex;
            public string SceneName;

            private string lastUpdatedMessage;
            private string displayMessage;

            public ValidationInfoMenuItem(OdinMenuTree tree, ValidationResult validationResult, ValidationProfileResult profileResult, ResizableColumn[] columns, DisplayOptions displayOptions, int originalItemIndex) : base(tree, "", validationResult)
            {
                this.ValidationResult = validationResult;
                this.ProfileResult = profileResult;
                this.Columns = columns;
                this.DisplayOptions = displayOptions;
                this.OriginalItemIndex = originalItemIndex;

                if (this.ProfileResult.SourceRecoveryData is SceneValidationProfile.SceneAddress)
                {
                    var address = (SceneValidationProfile.SceneAddress)this.ProfileResult.SourceRecoveryData;
                    var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(address.ScenePath);
                    this.SceneName = sceneAsset != null ? sceneAsset.name : "";
                }
                else this.SceneName = "";

                this.SearchString = string.Join(" ", AllDisplayOptions.Select(x => this.GetDisplayString(x)).ToArray());
            }

            public override void DrawMenuItem(int indentLevel)
            {
                base.DrawMenuItem(indentLevel);

                if (!this.MenuItemIsBeingRendered || Event.current.type != EventType.Repaint) return;

                var totalRect = this.Rect;

                int columnIndex = 0;
                float currentX = totalRect.xMin;

                for (int i = 0; i < AllDisplayOptions.Length; i++)
                {
                    var option = AllDisplayOptions[i];

                    if ((this.DisplayOptions & option) == option)
                    {
                        var width = this.Columns[columnIndex].ColWidth;
                        var rect = new Rect(currentX, totalRect.yMin, width, totalRect.height);

                        if (option == DisplayOptions.Category)
                        {
                            rect = rect.AlignCenter(16, 16);

                            switch (this.ValidationResult.ResultType)
                            {
                                case ValidationResultType.Valid:
                                    GUIHelper.PushColor(Color.green);
                                    GUI.DrawTexture(rect, EditorIcons.Checkmark.Highlighted, ScaleMode.ScaleToFit);
                                    GUIHelper.PopColor();
                                    break;
                                case ValidationResultType.Error:
                                    GUI.DrawTexture(rect, EditorIcons.UnityErrorIcon, ScaleMode.ScaleToFit);
                                    break;
                                case ValidationResultType.Warning:
                                    GUI.DrawTexture(rect, EditorIcons.UnityWarningIcon, ScaleMode.ScaleToFit);
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            rect.y = this.LabelRect.y;
                            rect.yMax = this.LabelRect.yMax;
                            rect.x += 5;
                            rect.width -= 10;

                            var labelStyle = this.IsSelected ? this.Style.SelectedLabelStyle : this.Style.DefaultLabelStyle;
                            var text = this.GetDisplayString(option);
                            GUI.Label(rect, GUIHelper.TempContent(text), labelStyle);
                        }

                        currentX += width;
                        columnIndex++;
                    }
                }
            }

            public string GetDisplayString(DisplayOptions option)
            {
                switch (option)
                {
                    case DisplayOptions.Message:
                        if (!object.ReferenceEquals(this.lastUpdatedMessage, this.ValidationResult.Message))
                        {
                            this.displayMessage = this.ValidationResult.Message.Replace("\n", " ").Replace("\r", "");
                            this.lastUpdatedMessage = this.ValidationResult.Message;
                        }

                        return this.displayMessage;
                    case DisplayOptions.Path:
                        {
                            var path = this.ProfileResult.Path;

                            if (string.IsNullOrEmpty(path) && this.ProfileResult.Source is UnityEngine.Object)
                            {
                                var uObj = this.ProfileResult.Source as UnityEngine.Object;
                                if (AssetDatabase.Contains(uObj.GetInstanceID()))
                                {
                                    path = AssetDatabase.GetAssetPath(uObj.GetInstanceID());
                                }
                            }

                            return path;
                        }
                    case DisplayOptions.PropertyPath:
                        {
                            return this.ValidationResult.Path;
                        }
                    case DisplayOptions.Validator:
                        return this.ValidationResult.Setup.Validator == null ? "None" : this.ValidationResult.Setup.Validator.GetType().GetNiceName();
                    case DisplayOptions.Object:
                        return this.ProfileResult.Name;
                    case DisplayOptions.Scene:
                        return this.SceneName;
                    case DisplayOptions.Category:
                        switch (this.ValidationResult.ResultType)
                        {
                            case ValidationResultType.Error:
                                return "Error";
                            case ValidationResultType.Warning:
                                return "Warning";
                            case ValidationResultType.Valid:
                            case ValidationResultType.IgnoreResult:
                            default:
                                return "";
                        }
                    default:
                        return "";
                }
            }
        }
    }
}
