// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using UnityEngine;

namespace Animancer
{
    /// <summary>A set of up/right/down/left animations with diagonals as well.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/playing/directional-sets">Directional Animation Sets</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/DirectionalAnimationSet8
    /// 
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Directional Animation Set/8 Directions", order = Strings.AssetMenuOrder + 11)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(DirectionalAnimationSet8))]
    public class DirectionalAnimationSet8 : DirectionalAnimationSet
    {
        /************************************************************************************************************************/

        [SerializeField]
        private AnimationClip _UpRight;

        /// <summary>[<see cref="SerializeField"/>] The animation facing diagonally up-right.</summary>
        public AnimationClip UpRight => _UpRight;

        /// <summary>Sets the <see cref="UpRight"/> animation.</summary>
        /// <remarks>This is not simply a property setter because the animations will usually not need to be changed by scripts.</remarks>
        public void SetUpRight(AnimationClip clip)
        {
            _UpRight = clip;
            AnimancerUtilities.SetDirty(this);
        }

        /************************************************************************************************************************/

        [SerializeField]
        private AnimationClip _DownRight;

        /// <summary>[<see cref="SerializeField"/>] The animation facing diagonally down-right.</summary>
        public AnimationClip DownRight => _DownRight;

        /// <summary>Sets the <see cref="DownRight"/> animation.</summary>
        /// <remarks>This is not simply a property setter because the animations will usually not need to be changed by scripts.</remarks>
        public void SetDownRight(AnimationClip clip)
        {
            _DownRight = clip;
            AnimancerUtilities.SetDirty(this);
        }

        /************************************************************************************************************************/

        [SerializeField]
        private AnimationClip _DownLeft;

        /// <summary>[<see cref="SerializeField"/>] The animation facing diagonally down-left.</summary>
        public AnimationClip DownLeft => _DownLeft;

        /// <summary>Sets the <see cref="DownLeft"/> animation.</summary>
        /// <remarks>This is not simply a property setter because the animations will usually not need to be changed by scripts.</remarks>
        public void SetDownLeft(AnimationClip clip)
        {
            _DownLeft = clip;
            AnimancerUtilities.SetDirty(this);
        }

        /************************************************************************************************************************/

        [SerializeField]
        private AnimationClip _UpLeft;

        /// <summary>[<see cref="SerializeField"/>] The animation facing diagonally up-left.</summary>
        public AnimationClip UpLeft => _UpLeft;

        /// <summary>Sets the <see cref="UpLeft"/> animation.</summary>
        /// <remarks>This is not simply a property setter because the animations will usually not need to be changed by scripts.</remarks>
        public void SetUpLeft(AnimationClip clip)
        {
            _UpLeft = clip;
            AnimancerUtilities.SetDirty(this);
        }

        /************************************************************************************************************************/

        /// <summary>Returns the animation closest to the specified `direction`.</summary>
        public override AnimationClip GetClip(Vector2 direction)
        {
            var angle = Mathf.Atan2(direction.y, direction.x);
            var octant = Mathf.RoundToInt(8 * angle / (2 * Mathf.PI) + 8) % 8;
            switch (octant)
            {
                case 0: return Right;
                case 1: return _UpRight;
                case 2: return Up;
                case 3: return _UpLeft;
                case 4: return Left;
                case 5: return _DownLeft;
                case 6: return Down;
                case 7: return _DownRight;
                default: throw new ArgumentOutOfRangeException("Invalid octant");
            }
        }

        /************************************************************************************************************************/
        #region Directions
        /************************************************************************************************************************/

        /// <summary>Constants for each of the diagonal directions.</summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/playing/directional-sets">Directional Animation Sets</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/Diagonals
        /// 
        public static class Diagonals
        {
            /************************************************************************************************************************/

            /// <summary>1 / (Square Root of 2).</summary>
            public const float OneOverSqrt2 = 0.70710678118f;

            /// <summary>
            /// A vector with a magnitude of 1 pointing up to the right.
            /// <para></para>
            /// The value is approximately (0.707, 0.707).
            /// </summary>
            public static Vector2 UpRight => new Vector2(OneOverSqrt2, OneOverSqrt2);

            /// <summary>
            /// A vector with a magnitude of 1 pointing down to the right.
            /// <para></para>
            /// The value is approximately (0.707, -0.707).
            /// </summary>
            public static Vector2 DownRight => new Vector2(OneOverSqrt2, -OneOverSqrt2);

            /// <summary>
            /// A vector with a magnitude of 1 pointing down to the left.
            /// <para></para>
            /// The value is approximately (-0.707, -0.707).
            /// </summary>
            public static Vector2 DownLeft => new Vector2(-OneOverSqrt2, -OneOverSqrt2);

            /// <summary>
            /// A vector with a magnitude of 1 pointing up to the left.
            /// <para></para>
            /// The value is approximately (-0.707, 0.707).
            /// </summary>
            public static Vector2 UpLeft => new Vector2(-OneOverSqrt2, OneOverSqrt2);

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/

        public override int ClipCount => 8;

        /************************************************************************************************************************/

        /// <summary>Up, Right, Down, Left, or their diagonals.</summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/playing/directional-sets">Directional Animation Sets</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/Direction
        /// 
        public new enum Direction
        {
            /// <summary><see cref="Vector2.up"/>.</summary>
            Up,

            /// <summary><see cref="Vector2.right"/>.</summary>
            Right,

            /// <summary><see cref="Vector2.down"/>.</summary>
            Down,

            /// <summary><see cref="Vector2.left"/>.</summary>
            Left,

            /// <summary><see cref="Vector2"/>(0.707..., 0.707...).</summary>
            UpRight,

            /// <summary><see cref="Vector2"/>(0.707..., -0.707...).</summary>
            DownRight,

            /// <summary><see cref="Vector2"/>(-0.707..., -0.707...).</summary>
            DownLeft,

            /// <summary><see cref="Vector2"/>(-0.707..., 0.707...).</summary>
            UpLeft,
        }

        /************************************************************************************************************************/

        protected override string GetDirectionName(int direction) => ((Direction)direction).ToString();

        /************************************************************************************************************************/

        /// <summary>Returns the animation associated with the specified `direction`.</summary>
        public AnimationClip GetClip(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return Up;
                case Direction.Right: return Right;
                case Direction.Down: return Down;
                case Direction.Left: return Left;
                case Direction.UpRight: return _UpRight;
                case Direction.DownRight: return _DownRight;
                case Direction.DownLeft: return _DownLeft;
                case Direction.UpLeft: return _UpLeft;
                default: throw new ArgumentException($"Unsupported {nameof(Direction)}: {direction}");
            }
        }

        public override AnimationClip GetClip(int direction) => GetClip((Direction)direction);

        /************************************************************************************************************************/

        /// <summary>Sets the animation associated with the specified `direction`.</summary>
        public void SetClip(Direction direction, AnimationClip clip)
        {
            switch (direction)
            {
                case Direction.Up: SetUp(clip); break;
                case Direction.Right: SetRight(clip); break;
                case Direction.Down: SetDown(clip); break;
                case Direction.Left: SetLeft(clip); break;
                case Direction.UpRight: _UpRight = clip; break;
                case Direction.DownRight: _DownRight = clip; break;
                case Direction.DownLeft: _DownLeft = clip; break;
                case Direction.UpLeft: _UpLeft = clip; break;
                default: throw new ArgumentException($"Unsupported {nameof(Direction)}: {direction}");
            }

            AnimancerUtilities.SetDirty(this);
        }

        public override void SetClip(int direction, AnimationClip clip) => SetClip((Direction)direction, clip);

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
                case Direction.UpRight: return Diagonals.UpRight;
                case Direction.DownRight: return Diagonals.DownRight;
                case Direction.DownLeft: return Diagonals.DownLeft;
                case Direction.UpLeft: return Diagonals.UpLeft;
                default: throw new ArgumentException($"Unsupported {nameof(Direction)}: {direction}");
            }
        }

        public override Vector2 GetDirection(int direction) => DirectionToVector((Direction)direction);

        /************************************************************************************************************************/

        /// <summary>Returns the direction closest to the specified `vector`.</summary>
        public new static Direction VectorToDirection(Vector2 vector)
        {
            var angle = Mathf.Atan2(vector.y, vector.x);
            var octant = Mathf.RoundToInt(8 * angle / (2 * Mathf.PI) + 8) % 8;
            switch (octant)
            {
                case 0: return Direction.Right;
                case 1: return Direction.UpRight;
                case 2: return Direction.Up;
                case 3: return Direction.UpLeft;
                case 4: return Direction.Left;
                case 5: return Direction.DownLeft;
                case 6: return Direction.Down;
                case 7: return Direction.DownRight;
                default: throw new ArgumentOutOfRangeException("Invalid octant");
            }
        }

        /************************************************************************************************************************/

        /// <summary>Returns a copy of the `vector` pointing in the closest direction this set type has an animation for.</summary>
        public new static Vector2 SnapVectorToDirection(Vector2 vector)
        {
            var magnitude = vector.magnitude;
            var direction = VectorToDirection(vector);
            vector = DirectionToVector(direction) * magnitude;
            return vector;
        }

        public override Vector2 Snap(Vector2 vector) => SnapVectorToDirection(vector);

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Name Based Operations
        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        public override int SetClipByName(AnimationClip clip)
        {
            var name = clip.name;

            var directionCount = ClipCount;
            for (int i = directionCount - 1; i >= 0; i--)
            {
                if (name.Contains(GetDirectionName(i)))
                {
                    SetClip(i, clip);
                    return i;
                }
            }

            return -1;
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}
