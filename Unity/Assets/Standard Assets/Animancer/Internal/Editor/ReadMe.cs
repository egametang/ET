// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] A welcome screen for Animancer.</summary>
    /// <remarks>Automatic selection is handled by <c>ShowReadMeOnStartup</c>.</remarks>
    //[CreateAssetMenu(menuName = Strings.MenuItemPrefix + "Read Me", order = Strings.AssetMenuOrder + 4)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "." + nameof(Editor) + "/" + nameof(ReadMe))]
    public sealed class ReadMe : ScriptableObject
    {
        /************************************************************************************************************************/

        /// <summary>The release ID of this Animancer version.</summary>
        /// <remarks><list type="bullet">
        ///   <item>[ 1] = v1.0: 2018-05-02.</item>
        ///   <item>[ 2] = v1.1: 2018-05-29.</item>
        ///   <item>[ 3] = v1.2: 2018-08-14.</item>
        ///   <item>[ 4] = v1.3: 2018-09-12.</item>
        ///   <item>[ 5] = v2.0: 2018-10-08.</item>
        ///   <item>[ 6] = v3.0: 2019-05-27.</item>
        ///   <item>[ 7] = v3.1: 2019-08-12.</item>
        ///   <item>[ 8] = v4.0: 2020-01-28.</item>
        ///   <item>[ 9] = v4.1: 2020-02-21.</item>
        ///   <item>[10] = v4.2: 2020-03-02.</item>
        ///   <item>[11] = v4.3: 2020-03-13.</item>
        ///   <item>[12] = v4.4: 2020-03-27.</item>
        ///   <item>[13] = v5.0: 2020-07-17.</item>
        ///   <item>[14] = v5.1: 2020-07-27.</item>
        ///   <item>[15] = v5.2: 2020-09-16.</item>
        ///   <item>[16] = v5.3: 2020-10-06.</item>
        ///   <item>[17] = v6.0: 2020-??-??.</item>
        /// </list></remarks>
        private const int ReleaseNumber = 17;

        /// <summary>The display name of this Animancer version.</summary>
        private const string VersionName = "v6.0";

        /// <summary>The end of the URL for the change log of this Animancer version.</summary>
        private const string ChangeLogSuffix = "v6-0";

        /// <summary>The key used to save the release number.</summary>
        private const string ReleaseNumberPrefKey = "Animancer.ReleaseNumber";

        /************************************************************************************************************************/

        [SerializeField] private Texture2D _Icon;
        [SerializeField] private DefaultAsset _ExamplesFolder;
        [SerializeField] private bool _DontShowOnStartup;

        /************************************************************************************************************************/

        /// <summary>If true, <c>ShowReadMeOnStartup</c> will not automatically select this asset.</summary>
        public bool DontShowOnStartup => _DontShowOnStartup && HasCorrectName;

        /************************************************************************************************************************/

        /// <summary>
        /// The <see cref="ReadMe"/> file name ends with the <see cref="VersionName"/> to detect if the user imported
        /// this version without deleting a previous version.
        /// <para></para>
        /// When Unity's package importer sees an existing file with the same GUID as one in the package, it will
        /// overwrite that file but not move or rename it if the name has changed. So it will leave the file there with
        /// the old version name.
        /// </summary>
        private bool HasCorrectName => name.EndsWith(VersionName);

        /************************************************************************************************************************/

        [CustomEditor(typeof(ReadMe))]
        private sealed class Editor : UnityEditor.Editor
        {
            /************************************************************************************************************************/

            /// <summary>The <see cref="Editor.target"/> cast to <see cref="ReadMe"/>.</summary>
            public ReadMe Target { get; private set; }

            /// <summary>The <see cref="ReleaseNumber"/> from when "Don't show this again" was last ticked.</summary>
            [NonSerialized]
            private int _PreviousVersion;

            /// <summary>The file path of the Examples folder.</summary>
            [NonSerialized]
            private string _ExamplesDirectory;

            /// <summary>The details of all example scenes.</summary>
            [NonSerialized]
            private List<ExampleGroup> _Examples;

            /************************************************************************************************************************/

            private void OnEnable()
            {
                Target = (ReadMe)target;

                _PreviousVersion = PlayerPrefs.GetInt(ReleaseNumberPrefKey, -1);
                if (_PreviousVersion < 0)
                    _PreviousVersion = EditorPrefs.GetInt(ReleaseNumberPrefKey, -1);// Animancer v2.0 used EditorPrefs.

                _Examples = ExampleGroup.Gather(Target._ExamplesFolder, out _ExamplesDirectory);
            }

            /************************************************************************************************************************/

            protected override void OnHeaderGUI()
            {
                GUILayout.BeginHorizontal("In BigTitle");
                {
                    const string Title = "Animancer Pro\n" + VersionName;
                    var title = AnimancerGUI.TempContent(Title, null, false);

                    var style = ObjectPool.GetCachedResult(() => new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 26,
                    });

                    var iconWidth = style.CalcHeight(title, EditorGUIUtility.currentViewWidth);
                    GUILayout.Label(Target._Icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
                    GUILayout.Label(title, style);
                }
                GUILayout.EndHorizontal();
            }

            /************************************************************************************************************************/

            public override void OnInspectorGUI()
            {
                DoWarnings();

                DoShowOnStartup();

                DoSpace();

                GUILayout.BeginVertical(GUI.skin.box);

                DoHeadingLink("Documentation", null, Strings.DocsURLs.Documentation);

                DoSpace();

                DoHeadingLink("Change Log", null, Strings.DocsURLs.ChangeLogPrefix + ChangeLogSuffix);

                GUILayout.EndVertical();

                DoSpace();

                GUILayout.BeginVertical(GUI.skin.box);

                DoHeadingLink("Examples", null, Strings.DocsURLs.Examples);
                if (Target._ExamplesFolder != null)
                {
                    EditorGUILayout.ObjectField(_ExamplesDirectory, Target._ExamplesFolder, typeof(SceneAsset), false);

                    ExampleGroup.DoExampleGUI(_Examples);
                }

                GUILayout.EndVertical();

                DoSpace();

                GUILayout.BeginVertical(GUI.skin.box);

                DoHeadingLink("Forum",
                    "for general discussion, feedback, and news",
                    "https://forum.unity.com/threads/566452");

                DoSpace();

                DoHeadingLink("Issues",
                    "for questions, suggestions, and bug reports",
                    "https://github.com/KybernetikGames/animancer/issues");

                DoSpace();

                DoHeadingLink("Email",
                    "for anything private",
                    "mailto:" + Strings.DocsURLs.DeveloperEmail + "?subject=Animancer", Strings.DocsURLs.DeveloperEmail);

                GUILayout.EndVertical();

                DoSpace();

                DoShowOnStartup();
            }

            /************************************************************************************************************************/

            private void DoShowOnStartup()
            {
                EditorGUI.BeginChangeCheck();
                Target._DontShowOnStartup = GUILayout.Toggle(Target._DontShowOnStartup, "Don't show this Read Me on startup");
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(Target);
                    if (Target._DontShowOnStartup)
                        PlayerPrefs.SetInt(ReleaseNumberPrefKey, ReleaseNumber);
                }
            }

            /************************************************************************************************************************/

            private void DoSpace() => GUILayout.Space(AnimancerGUI.LineHeight * 0.2f);

            /************************************************************************************************************************/

            private void DoWarnings()
            {
                MessageType messageType;

                if (!Target.HasCorrectName)
                {
                    messageType = MessageType.Error;
                }
                else if (_PreviousVersion >= 0 && _PreviousVersion < ReleaseNumber)
                {
                    messageType = MessageType.Warning;
                }
                else return;

                // Upgraded from any older version.

                DoSpace();

                var directory = AssetDatabase.GetAssetPath(Target);
                directory = Path.GetDirectoryName(directory);

                string versionWarning;
                if (messageType == MessageType.Error)
                {
                    versionWarning = "You must fully delete any old version of Animancer before importing a new version." +
                        "\n\nClick here to delete '" + directory + "' then you will need to import Animancer again.";
                }
                else
                {
                    versionWarning = "You must fully delete any old version of Animancer before importing a new version." +
                        "\n\nYou can ignore this message if you have already done so." +
                        " Otherwise click here to delete '" + directory + "' then you will need to import Animancer again.";
                }

                EditorGUILayout.HelpBox(versionWarning, messageType);
                CheckDeleteAnimancer(directory);

                // Upgraded from before v2.0.
                if (_PreviousVersion < 4)
                {
                    DoSpace();

                    EditorGUILayout.HelpBox("It seems you have just upgraded from an earlier version of Animancer" +
                        " (before v2.0) so you will need to restart Unity before you can use it.",
                        MessageType.Warning);
                }

                DoSpace();
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Asks if the user wants to delete the root Animancer folder and does so if they confirm.
            /// </summary>
            private void CheckDeleteAnimancer(string directory)
            {
                if (!AnimancerGUI.TryUseClickEventInLastRect())
                    return;

                if (!AssetDatabase.IsValidFolder(directory))
                {
                    Debug.Log($"{directory} doesn't exist." +
                        " You must have moved Animancer somewhere else so you will need to delete it manually.", this);
                    return;
                }

                if (!EditorUtility.DisplayDialog("Delete Animancer?",
                    "Would you like to delete " + directory + "?" +
                    "\n\nYou will then need to reimport Animancer manually.",
                    "Delete", "Cancel"))
                    return;

                AssetDatabase.DeleteAsset(directory);
            }

            /************************************************************************************************************************/

            private bool DoHeadingLink(string heading, string description, string url, string displayURL = null)
            {
                if (DoLinkLabel(heading, description))
                    Application.OpenURL(url);

                bool clicked;

                if (displayURL == null)
                    displayURL = url;

                var area = AnimancerGUI.LayoutSingleLineRect();

                var content = AnimancerGUI.TempContent(displayURL,
                    "Click to copy this link to the clipboard", false);

                var style = ObjectPool.GetCachedResult(() =>
                {
                    var newStyle = new GUIStyle(GUI.skin.label);
                    newStyle.fontSize = Mathf.CeilToInt(newStyle.fontSize * 0.8f);
                    newStyle.normal.textColor = Color.Lerp(newStyle.normal.textColor, Color.grey, 0.75f);
                    return newStyle;
                });

                if (GUI.Button(area, content, style))
                {
                    GUIUtility.systemCopyBuffer = displayURL;
                    Debug.Log($"Copied '{displayURL}' to the clipboard.", this);
                    clicked = true;
                }
                else clicked = false;

                EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Text);

                return clicked;
            }

            /************************************************************************************************************************/

            private bool DoLinkLabel(string label, string description)
            {
                var headerStyle = ObjectPool.GetCachedResult(() =>
                {
                    var newStyle = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 18,
                        stretchWidth = false,
                    };
                    newStyle.normal.textColor = newStyle.hover.textColor = new Color32(0x00, 0x78, 0xDA, 0xFF);
                    return newStyle;
                });

                var content = AnimancerGUI.TempContent(label, null, false);
                var size = headerStyle.CalcSize(content);
                var labelArea = GUILayoutUtility.GetRect(0, size.y);
                var buttonArea = AnimancerGUI.StealFromLeft(ref labelArea, size.x);

                if (description != null)
                {
                    var descriptionStyle = ObjectPool.GetCachedResult(() => new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.LowerLeft,
                    });

                    GUI.Label(labelArea, description, descriptionStyle);
                }

                Handles.BeginGUI();
                Handles.color = headerStyle.normal.textColor;
                Handles.DrawLine(new Vector3(buttonArea.xMin, buttonArea.yMax), new Vector3(buttonArea.xMax, buttonArea.yMax));
                Handles.color = Color.white;
                Handles.EndGUI();

                EditorGUIUtility.AddCursorRect(buttonArea, MouseCursor.Link);

                return GUI.Button(buttonArea, content, headerStyle);
            }

            /************************************************************************************************************************/

            private sealed class ExampleGroup
            {
                /************************************************************************************************************************/

                /// <summary>The name of this group.</summary>
                public readonly string Name;

                /// <summary>The scenes in the <see cref="Name"/>.</summary>
                public readonly List<SceneAsset> Scenes = new List<SceneAsset>();

                /// <summary>The folder paths of each of the <see cref="Scenes"/>.</summary>
                public readonly List<string> Directories = new List<string>();

                /// <summary>Indicates whether this group should show its contents in the GUI.</summary>
                private bool _IsExpanded;

                /************************************************************************************************************************/

                public static List<ExampleGroup> Gather(DefaultAsset rootDirectoryAsset, out string examplesDirectory)
                {
                    if (rootDirectoryAsset == null)
                    {
                        examplesDirectory = null;
                        return null;
                    }

                    examplesDirectory = AssetDatabase.GetAssetPath(rootDirectoryAsset);
                    if (string.IsNullOrEmpty(examplesDirectory))
                        return null;

                    var directories = Directory.GetDirectories(examplesDirectory);
                    var examples = new List<ExampleGroup>();

                    List<SceneAsset> scenes = null;

                    for (int i = 0; i < directories.Length; i++)
                    {
                        var directory = directories[i];
                        var files = Directory.GetFiles(directory, "*.unity", SearchOption.AllDirectories);

                        for (int j = 0; j < files.Length; j++)
                        {
                            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(files[j]);
                            if (scene != null)
                            {
                                AnimancerUtilities.NewIfNull(ref scenes);
                                scenes.Add(scene);
                            }
                        }

                        if (scenes != null)
                        {
                            examples.Add(new ExampleGroup(examplesDirectory, directory, scenes));
                            scenes = null;
                        }
                    }

                    examplesDirectory = Path.GetDirectoryName(examplesDirectory);

                    return examples;
                }

                /************************************************************************************************************************/

                public ExampleGroup(string rootDirectory, string directory, List<SceneAsset> scenes)
                {
                    var start = rootDirectory.Length + 1;
                    Name = directory.Substring(start, directory.Length - start);
                    Scenes = scenes;

                    start = directory.Length + 1;

                    for (int i = 0; i < scenes.Count; i++)
                    {
                        directory = AssetDatabase.GetAssetPath(scenes[i]);

                        directory = directory.Substring(start, directory.Length - start);
                        directory = Path.GetDirectoryName(directory);
                        Directories.Add(directory);
                    }
                }

                /************************************************************************************************************************/

                public static void DoExampleGUI(List<ExampleGroup> examples)
                {
                    if (examples == null)
                        return;

                    for (int i = 0; i < examples.Count; i++)
                        examples[i].DoExampleGUI();
                }

                public void DoExampleGUI()
                {
                    EditorGUI.indentLevel++;
                    _IsExpanded = EditorGUILayout.Foldout(_IsExpanded, AnimancerGUI.TempContent(Name, null, false), true);
                    if (_IsExpanded)
                    {
                        for (int i = 0; i < Scenes.Count; i++)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.ObjectField(Directories[i], Scenes[i], typeof(SceneAsset), false);
                            EditorGUI.indentLevel--;
                        }
                    }
                    EditorGUI.indentLevel--;
                }

                /************************************************************************************************************************/
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
    }
}

#endif

