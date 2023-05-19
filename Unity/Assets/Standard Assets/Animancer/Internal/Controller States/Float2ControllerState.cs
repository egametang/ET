// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using UnityEngine;

namespace Animancer
{
    /// <summary>[Pro-Only] A <see cref="ControllerState"/> which manages two float parameters.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/animator-controllers">Animator Controllers</see>
    /// </remarks>
    /// <seealso cref="Float1ControllerState"/>
    /// <seealso cref="Float3ControllerState"/>
    /// https://kybernetik.com.au/animancer/api/Animancer/Float2ControllerState
    /// 
    public sealed class Float2ControllerState : ControllerState
    {
        /************************************************************************************************************************/

        private ParameterID _ParameterXID;

        /// <summary>The identifier of the parameter which <see cref="ParameterX"/> will get and set.</summary>
        public ParameterID ParameterXID
        {
            get => _ParameterXID;
            set
            {
                _ParameterXID = value;
                _ParameterXID.ValidateHasParameter(Controller, AnimatorControllerParameterType.Float);
            }
        }

        /// <summary>
        /// Gets and sets a float parameter in the <see cref="ControllerState.Controller"/> using the
        /// <see cref="ParameterXID"/>.
        /// </summary>
        public float ParameterX
        {
            get => Playable.GetFloat(_ParameterXID.Hash);
            set => Playable.SetFloat(_ParameterXID.Hash, value);
        }

        /************************************************************************************************************************/

        private ParameterID _ParameterYID;

        /// <summary>The identifier of the parameter which <see cref="ParameterY"/> will get and set.</summary>
        public ParameterID ParameterYID
        {
            get => _ParameterYID;
            set
            {
                _ParameterYID = value;
                _ParameterYID.ValidateHasParameter(Controller, AnimatorControllerParameterType.Float);
            }
        }

        /// <summary>
        /// Gets and sets a float parameter in the <see cref="ControllerState.Controller"/> using the
        /// <see cref="ParameterYID"/>.
        /// </summary>
        public float ParameterY
        {
            get => Playable.GetFloat(_ParameterYID.Hash);
            set => Playable.SetFloat(_ParameterYID.Hash, value);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Gets and sets <see cref="ParameterX"/> and <see cref="ParameterY"/>.
        /// </summary>
        public Vector2 Parameter
        {
            get => new Vector2(ParameterX, ParameterY);
            set
            {
                ParameterX = value.x;
                ParameterY = value.y;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="Float2ControllerState"/> to play the `controller`.</summary>
        public Float2ControllerState(RuntimeAnimatorController controller,
            ParameterID parameterX, ParameterID parameterY, bool keepStateOnStop = false)
            : base(controller, keepStateOnStop)
        {
            _ParameterXID = parameterX;
            _ParameterXID.ValidateHasParameter(Controller, AnimatorControllerParameterType.Float);

            _ParameterYID = parameterY;
            _ParameterYID.ValidateHasParameter(Controller, AnimatorControllerParameterType.Float);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override int ParameterCount => 2;

        /// <inheritdoc/>
        public override int GetParameterHash(int index)
        {
            switch (index)
            {
                case 0: return _ParameterXID;
                case 1: return _ParameterYID;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            };
        }

        /************************************************************************************************************************/
        #region Transition
        /************************************************************************************************************************/

        /// <summary>
        /// A serializable <see cref="ITransition"/> which can create a <see cref="Float2ControllerState"/>
        /// when passed into <see cref="AnimancerPlayable.Play(ITransition)"/>.
        /// </summary>
        /// <remarks>
        /// Unfortunately the tool used to generate this documentation does not currently support nested types with
        /// identical names, so only one <c>Transition</c> class will actually have a documentation page.
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/Transition
        /// 
        [Serializable]
        public new class Transition : Transition<Float2ControllerState>
        {
            /************************************************************************************************************************/

            [SerializeField]
            private string _ParameterNameX;

            /// <summary>[<see cref="SerializeField"/>] The name that will be used to access <see cref="ParameterX"/>.</summary>
            public ref string ParameterNameX => ref _ParameterNameX;

            /************************************************************************************************************************/

            [SerializeField]
            private string _ParameterNameY;

            /// <summary>[<see cref="SerializeField"/>] The name that will be used to access <see cref="ParameterY"/>.</summary>
            public ref string ParameterNameY => ref _ParameterNameY;

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="Transition"/>.</summary>
            public Transition() { }

            /// <summary>Creates a new <see cref="Transition"/> with the specified Animator Controller and parameters.</summary>
            public Transition(RuntimeAnimatorController controller, string parameterNameX, string parameterNameY)
            {
                Controller = controller;
                _ParameterNameX = parameterNameX;
                _ParameterNameY = parameterNameY;
            }

            /************************************************************************************************************************/

            /// <summary>Creates and returns a new <see cref="Float2ControllerState"/>.</summary>
            /// <remarks>
            /// Note that using methods like <see cref="AnimancerPlayable.Play(ITransition)"/> will also call
            /// <see cref="ITransition.Apply"/>, so if you call this method manually you may want to call that method
            /// as well. Or you can just use <see cref="AnimancerUtilities.CreateStateAndApply"/>.
            /// <para></para>
            /// This method also assigns it as the <see cref="AnimancerState.Transition{TState}.State"/>.
            /// </remarks>
            public override Float2ControllerState CreateState()
                => State = new Float2ControllerState(Controller, _ParameterNameX, _ParameterNameY, KeepStateOnStop);

            /************************************************************************************************************************/
            #region Drawer
#if UNITY_EDITOR
            /************************************************************************************************************************/

            /// <summary>[Editor-Only] Draws the Inspector GUI for a <see cref="Transition"/>.</summary>
            /// <remarks>
            /// Unfortunately the tool used to generate this documentation does not currently support nested types with
            /// identical names, so only one <c>Drawer</c> class will actually have a documentation page.
            /// <para></para>
            /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
            /// </remarks>
            [UnityEditor.CustomPropertyDrawer(typeof(Transition), true)]
            public class Drawer : ControllerState.Transition.Drawer
            {
                /************************************************************************************************************************/

                /// <summary>
                /// Creates a new <see cref="Drawer"/> and sets the
                /// <see cref="ControllerState.Transition.Drawer.Parameters"/>.
                /// </summary>
                public Drawer() : base(nameof(_ParameterNameX), nameof(_ParameterNameY)) { }

                /************************************************************************************************************************/
            }

            /************************************************************************************************************************/
#endif
            #endregion
            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

