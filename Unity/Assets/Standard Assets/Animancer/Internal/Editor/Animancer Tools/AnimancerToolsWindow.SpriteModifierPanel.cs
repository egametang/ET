// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    partial class AnimancerToolsWindow
    {
        /// <summary>[Editor-Only] [Pro-Only] A base <see cref="Panel"/> for modifying <see cref="Sprite"/>s.</summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/tools">Animancer Tools</see>
        /// </remarks>
        [Serializable]
        public abstract class SpriteModifierPanel : Panel
        {
            /************************************************************************************************************************/

            private static readonly List<Sprite> SelectedSprites = new List<Sprite>();
            private static bool _HasGatheredSprites;

            /// <summary>The currently selected <see cref="Sprite"/>s.</summary>
            public static List<Sprite> Sprites
            {
                get
                {
                    if (!_HasGatheredSprites)
                    {
                        _HasGatheredSprites = true;
                        GatherSelectedSprites(SelectedSprites);
                    }

                    return SelectedSprites;
                }
            }

            /// <inheritdoc/>
            public override void OnSelectionChanged()
            {
                _HasGatheredSprites = false;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Adds all <see cref="Sprite"/>s in the <see cref="Selection.objects"/> or their sub-assets to the
            /// list of `sprites`.
            /// </summary>
            public static void GatherSelectedSprites(List<Sprite> sprites)
            {
                sprites.Clear();

                var selection = Selection.objects;
                for (int i = 0; i < selection.Length; i++)
                {
                    var selected = selection[i];
                    if (selected is Sprite sprite)
                    {
                        sprites.Add(sprite);
                    }
                    else if (selected is Texture2D texture)
                    {
                        sprites.AddRange(LoadAllSpritesInTexture(texture));
                    }
                }

                sprites.Sort(NaturalCompare);
            }

            /************************************************************************************************************************/

            /// <summary>The message to confirm that the user is certain they want to apply the changes.</summary>
            protected virtual string AreYouSure => "Are you sure you want to modify these Sprites?";

            /// <summary>Called immediately after the user confirms they want to apply changes.</summary>
            protected virtual void PrepareToApply() { }

            /// <summary>Applies the desired modifications to the `data` before it is saved.</summary>
            protected virtual void Modify(ref SpriteMetaData data, Sprite sprite) { }

            /************************************************************************************************************************/

            /// <summary>
            /// Asks the user if they want to modify the target <see cref="Sprite"/>s and calls <see cref="Modify"/>
            /// on each of them before saving any changes.
            /// </summary>
            protected void AskAndApply()
            {
                if (!EditorUtility.DisplayDialog("Are You Sure?",
                    AreYouSure + "\n\nThis operation cannot be undone.",
                    "Modify", "Cancel"))
                    return;

                PrepareToApply();

                var pathToSprites = new Dictionary<string, List<Sprite>>();
                var sprites = Sprites;
                for (int i = 0; i < sprites.Count; i++)
                {
                    var sprite = sprites[i];

                    var path = AssetDatabase.GetAssetPath(sprite);

                    if (!pathToSprites.TryGetValue(path, out var spritesAtPath))
                        pathToSprites.Add(path, spritesAtPath = new List<Sprite>());

                    spritesAtPath.Add(sprite);
                }

                foreach (var asset in pathToSprites)
                {
                    var importer = (TextureImporter)AssetImporter.GetAtPath(asset.Key);
                    var spriteSheet = importer.spritesheet;
                    var hasError = false;

                    sprites = asset.Value;
                    for (int iSprite = 0; iSprite < sprites.Count; iSprite++)
                    {
                        var sprite = sprites[iSprite];
                        for (int iSpriteData = 0; iSpriteData < spriteSheet.Length; iSpriteData++)
                        {
                            ref var spriteData = ref spriteSheet[iSpriteData];
                            if (spriteData.name == sprite.name &&
                                spriteData.rect == sprite.rect)
                            {
                                Modify(ref spriteData, sprite);
                                sprites.RemoveAt(iSprite--);

                                if (spriteData.rect.xMin < 0 ||
                                    spriteData.rect.yMin < 0 ||
                                    spriteData.rect.xMax >= sprite.texture.width ||
                                    spriteData.rect.xMax >= sprite.texture.height)
                                {
                                    hasError = true;
                                    Debug.LogError($"This modification would have put '{sprite.name}' out of bounds," +
                                        $" so '{asset.Key}' was not modified.");
                                }

                                break;
                            }
                        }
                    }

                    if (!hasError)
                    {
                        importer.spritesheet = spriteSheet;
                        EditorUtility.SetDirty(importer);
                        importer.SaveAndReimport();
                    }

                    if (sprites.Count > 0)
                    {
                        var message = ObjectPool.AcquireStringBuilder()
                            .Append("Unable to find data at '")
                            .Append(asset.Key)
                            .Append("' for ")
                            .Append(sprites.Count)
                            .Append(" Sprites:");

                        for (int i = 0; i < sprites.Count; i++)
                        {
                            message.AppendLine()
                                .Append(" - ")
                                .Append(sprites[i].name);
                        }

                        Debug.LogError(message.ReleaseToString(), importer);
                    }
                }
            }

            /************************************************************************************************************************/
        }
    }
}

#endif

