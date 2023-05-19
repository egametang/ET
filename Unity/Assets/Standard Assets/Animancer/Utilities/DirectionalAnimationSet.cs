// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Animancer
{
    /// <summary>A set of up/right/down/left animations.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/playing/directional-sets">Directional Animation Sets</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/DirectionalAnimationSet
    /// 
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Directional Animation Set/4 Directions", order = Strings.AssetMenuOrder + 10)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(DirectionalAnimationSet))]
    public class DirectionalAnimationSet : ScriptableObject, IAnimationClipSource
    {
        /************************************************************************************************************************/

        [SerializeField]
        private AnimationClip _Up;

        /// <summary>[<see cref="SerializeField"/>] The animation facing up.</summary>
        public AnimationClip Up => _Up;

        /// <summary>Sets the <see cref="Up"/> animation.</summary>
        /// <remarks>This is not simply a property setter because the animations will usually not need to be changed by scripts.</remarks>
        public void SetUp(AnimationClip clip)
        {
            _Up = clip;
            AnimancerUtilities.SetDirty(this);
        }

        /************************************************************************************************************************/

        [SerializeField]
        private AnimationClip _Right;

        /// <summary>[<see cref="SerializeField"/>] The animation facing right.</summary>
        public AnimationClip Right => _Right;

        /// <summary>Sets the <see cref="Right"/> animation.</summary>
        /// <remarks>This is not simply a property setter because the animations will usually not need to be changed by scripts.</remarks>
        public void SetRight(AnimationClip clip)
        {
            _Right = clip;
            AnimancerUtilities.SetDirty(this);
        }

        /************************************************************************************************************************/

        [SerializeField]
        private AnimationClip _Down;

        /// <summary>[<see cref="SerializeField"/>] The animation facing down.</summary>
        public AnimationClip Down => _Down;

        /// <summary>Sets the <see cref="Down"/> animation.</summary>
        /// <remarks>This is not simply a property setter because the animations will usually not need to be changed by scripts.</remarks>
        public void SetDown(AnimationClip clip)
        {
            _Down = clip;
            AnimancerUtilities.SetDirty(this);
        }

        /************************************************************************************************************************/

        [SerializeField]
        private AnimationClip _Left;

        /// <summary>[<see cref="SerializeField"/>] The animation facing left.</summary>
        public AnimationClip Left => _Left;

        /// <summary>Sets the <see cref="Left"/> animation.</summary>
        /// <remarks>This is not simply a property setter because the animations will usually not need to be changed by scripts.</remarks>
        public void SetLeft(AnimationClip clip)
        {
            _Left = clip;
            AnimancerUtilities.SetDirty(this);
        }

        /************************************************************************************************************************/

        /// <summary>Returns the animation closest to the specified `direction`.</summary>
        public virtual AnimationClip GetClip(Vector2 direction)
        {
            if (direction.x >= 0)
            {
                if (direction.y >= 0)
                    return direction.x > direction.y ? _Right : _Up;
                else
                    return direction.x > -direction.y ? _Right : _Down;
            }
            else
            {
                if (direction.y >= 0)
                    return direction.x < -direction.y ? _Left : _Up;
                else
                    return direction.x < direction.y ? _Left : _Down;
            }
        }

        /************************************************************************************************************************/
        #region Directions
        /************************************************************************************************************************/

        /// <summary>The number of animations in this set.</summary>
        public virtual int ClipCount => 4;

        /************************************************************************************************************************/

        /// <summary>Up, Right, Down, or Left.</summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/playing/directional-sets">Directional Animation Sets</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/Direction
        /// 
        public enum Direction
        {
            /// <summary><see cref="Vector2.up"/>.</summary>
            Up,

            /// <summary><see cref="Vector2.right"/>.</summary>
            Right,

            /// <summary><see cref="Vector2.down"/>.</summary>
            Down,

            /// <summary><see cref="Vector2.left"/>.</summary>
            Left,
        }

        /************************************************************************************************************************/

        /// <summary>Returns the name of the specified `direction`.</summary>
        protected virtual string GetDirectionName(int direction) => ((Direction)direction).ToString();

        /************************************************************************************************************************/

        /// <summary>Returns the animation associated with the specified `direction`.</summary>
        public AnimationClip GetClip(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return _Up;
                case Direction.Right: return _Right;
                case Direction.Down: return _Down;
                case Direction.Left: return _Left;
                default: throw new ArgumentException($"Unsupported {nameof(Direction)}: {direction}");
            }
        }

        /// <summary>Returns the animation associated with the specified `direction`.</summary>
        public virtual AnimationClip GetClip(int direction) => GetClip((Direction)direction);

        /************************************************************************************************************************/

        /// <summary>Sets the animation associated with the specified `direction`.</summary>
        public void SetClip(Direction direction, AnimationClip clip)
        {
            switch (direction)
            {
                case Direction.Up: _Up = clip; break;
                case Direction.Right: _Right = clip; break;
                case Direction.Down: _Down = clip; break;
                case Direction.Left: _Left = clip; break;
                default: throw new ArgumentException($"Unsupported {nameof(Direction)}: {direction}");
            }

            AnimancerUtilities.SetDirty(this);
        }

        /// <summary>Sets the animation associated with the specified `direction`.</summary>
        public virtual void SetClip(int direction, AnimationClip clip) => SetClip((Direction)direction, clip);

        /************************************************************************************************************************/
        #region Conversion
        /************************************************************************************************************************/

        /// <summary>Returns a vector representing the specified `direction`.</summary>
        public static Vector2 DirectionToVector(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return Vector2.up;
                case Direction.Right: return Vector2.right;
                case Direction.Down: return Vector2.down;
                case Direction.Left: return Vector2.left;
                default: throw new ArgumentException($"Unsupported {nameof(Direction)}: {direction}");
            }
        }

        /// <summary>Returns a vector representing the specified `direction`.</summary>
        public virtual Vector2 GetDirection(int direction) => DirectionToVector((Direction)direction);

        /************************************************************************************************************************/

        /// <summary>Returns the direction closest to the specified `vector`.</summary>
        public static Direction VectorToDirection(Vector2 vector)
        {
            if (vector.x >= 0)
            {
                if (vector.y >= 0)
                    return vector.x > vector.y ? Direction.Right : Direction.Up;
                else
                    return vector.x > -vector.y ? Direction.Right : Direction.Down;
            }
            else
            {
                if (vector.y >= 0)
                    return vector.x < -vector.y ? Direction.Left : Direction.Up;
                else
                    return vector.x < vector.y ? Direction.Left : Direction.Down;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Returns a copy of the `vector` pointing in the closest direction this set type has an animation for.</summary>
        public static Vector2 SnapVectorToDirection(Vector2 vector)
        {
            var magnitude = vector.magnitude;
            var direction = VectorToDirection(vector);
            vector = DirectionToVector(direction) * magnitude;
            return vector;
        }

        /// <summary>Returns a copy of the `vector` pointing in the closest direction this set has an animation for.</summary>
        public virtual Vector2 Snap(Vector2 vector) => SnapVectorToDirection(vector);

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Collections
        /************************************************************************************************************************/

        /// <summary>Adds all animations from this set to the `clips`, starting from the specified `index`.</summary>
        public void AddClips(AnimationClip[] clips, int index)
        {
            var count = ClipCount;
            for (int i = 0; i < count; i++)
                clips[index + i] = GetClip(i);
        }

        /// <summary>[<see cref="IAnimationClipSource"/>] Adds all animations from this set to the `clips`.</summary>
        public void GetAnimationClips(List<AnimationClip> clips)
        {
            var count = ClipCount;
            for (int i = 0; i < count; i++)
                clips.Add(GetClip(i));
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Adds unit vectors corresponding to each of the animations in this set to the `directions`, starting from
        /// the specified `index`.
        /// </summary>
        public void AddDirections(Vector2[] directions, int index)
        {
            var count = ClipCount;
            for (int i = 0; i < count; i++)
                directions[index + i] = GetDirection(i);
        }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="AddClips"/> and <see cref="AddDirections"/>.</summary>
        public void AddClipsAndDirections(AnimationClip[] clips, Vector2[] directions, int index)
        {
            AddClips(clips, index);
            AddDirections(directions, index);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Editor Functions
        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        [UnityEditor.CustomEditor(typeof(DirectionalAnimationSet), true)]
        private class Editor : Animancer.Editor.ScriptableObjectEditor { }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Attempts to assign the `clip` to one of this set's fields based on its name and returns the direction index
        /// of that field (or -1 if it was unable to determine the direction).
        /// </summary>
        public virtual int SetClipByName(AnimationClip clip)
        {
            var name = clip.name;

            int bestDirection = -1;
            int bestDirectionIndex = -1;

            var directionCount = ClipCount;
            for (int i = 0; i < directionCount; i++)
            {
                var index = name.LastIndexOf(GetDirectionName(i));
                if (bestDirectionIndex < index)
                {
                    bestDirectionIndex = index;
                    bestDirection = i;
                }
            }

            if (bestDirection >= 0)
                SetClip(bestDirection, clip);

            return bestDirection;
        }

        /************************************************************************************************************************/

        [UnityEditor.MenuItem("CONTEXT/" + nameof(DirectionalAnimationSet) + "/Find Animations")]
        private static void FindSimilarAnimations(UnityEditor.MenuCommand command)
        {
            var set = (DirectionalAnimationSet)command.context;

            UnityEditor.Undo.RecordObject(set, "Find Animations");

            var directory = UnityEditor.AssetDatabase.GetAssetPath(set);
            directory = Path.GetDirectoryName(directory);

            var guids = UnityEditor.AssetDatabase.FindAssets(
                $"{set.name} t:{nameof(AnimationClip)}",
                new string[] { directory });

            for (int i = 0; i < guids.Length; i++)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip == null)
                    continue;

                set.SetClipByName(clip);
            }
        }

        /************************************************************************************************************************/

        [UnityEditor.MenuItem("Assets/Create/Animancer/Directional Animation Set/From Selection",
            priority = Strings.AssetMenuOrder + 12)]
        private static void CreateDirectionalAnimationSet()
        {
            var nameToAnimations = new Dictionary<string, List<AnimationClip>>();

            var selection = UnityEditor.Selection.objects;
            for (int i = 0; i < selection.Length; i++)
            {
                var clip = selection[i] as AnimationClip;
                if (clip == null)
                    continue;

                var name = clip.name;
                for (Direction direction = 0; direction < (Direction)4; direction++)
                {
                    name = name.Replace(direction.ToString(), "");
                }

                if (!nameToAnimations.TryGetValue(name, out var clips))
                {
                    clips = new List<AnimationClip>();
                    nameToAnimations.Add(name, clips);
                }

                clips.Add(clip);
            }

            if (nameToAnimations.Count == 0)
                throw new InvalidOperationException("No clips are selected");

            foreach (var nameAndAnimations in nameToAnimations)
            {
                var set = nameAndAnimations.Value.Count <= 4 ?
                    CreateInstance<DirectionalAnimationSet>() :
                    CreateInstance<DirectionalAnimationSet8>();

                for (int i = 0; i < nameAndAnimations.Value.Count; i++)
                {
                    set.SetClipByName(nameAndAnimations.Value[i]);
                }

                var path = UnityEditor.AssetDatabase.GetAssetPath(nameAndAnimations.Value[0]);
                path = Path.GetDirectoryName(path) + "/" + nameAndAnimations.Key + ".asset";
                UnityEditor.AssetDatabase.CreateAsset(set, path);
            }
        }

        /************************************************************************************************************************/

        [UnityEditor.MenuItem("CONTEXT/" + nameof(DirectionalAnimationSet) + "/Toggle Looping")]
        private static void ToggleLooping(UnityEditor.MenuCommand command)
        {
            var set = (DirectionalAnimationSet)command.context;

            var count = set.ClipCount;
            for (int i = 0; i < count; i++)
            {
                var clip = set.GetClip(i);
                if (clip == null)
                    continue;

                var isLooping = !clip.isLooping;
                for (i = 0; i < count; i++)
                {
                    clip = set.GetClip(i);
                    if (clip == null)
                        continue;

                    Animancer.Editor.AnimancerEditorUtilities.SetLooping(clip, isLooping);
                }

                break;
            }
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}
