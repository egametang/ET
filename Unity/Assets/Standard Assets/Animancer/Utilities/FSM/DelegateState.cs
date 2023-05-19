// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;

namespace Animancer.FSM
{
    /// <summary>An <see cref="IState"/> that uses delegates to define its behaviour.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/fsm">Finite State Machines</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.FSM/DelegateState
    /// 
    public class DelegateState : IState
    {
        /************************************************************************************************************************/

        /// <summary>Determines whether this state can be entered. Null is treated as returning true.</summary>
        public Func<bool> canEnter;

        /// <summary>Calls <see cref="canEnter"/> to determine whether this state can be entered.</summary>
        bool IState.CanEnterState => canEnter == null || canEnter();

        /************************************************************************************************************************/

        /// <summary>Determines whether this state can be exited. Null is treated as returning true.</summary>
        public Func<bool> canExit;

        /// <summary>Calls <see cref="canExit"/> to determine whether this state can be exited.</summary>
        bool IState.CanExitState => canExit == null || canExit();

        /************************************************************************************************************************/

        /// <summary>Called when this state is entered.</summary>
        public Action onEnter;

        /// <summary>Calls <see cref="onEnter"/> when this state is entered.</summary>
        void IState.OnEnterState() => onEnter?.Invoke();

        /************************************************************************************************************************/

        /// <summary>Called when this state is exited.</summary>
        public Action onExit;

        /// <summary>Calls <see cref="onExit"/> when this state is exited.</summary>
        void IState.OnExitState() => onExit?.Invoke();

        /************************************************************************************************************************/
    }
}
