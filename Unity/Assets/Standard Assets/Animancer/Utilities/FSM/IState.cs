// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using UnityEngine;

namespace Animancer.FSM
{
    /// <summary>A state that can be used in a <see cref="StateMachine{TState}"/>.</summary>
    /// <remarks>
    /// The <see cref="StateExtensions"/> class contains various extension methods for this interface.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/fsm">Finite State Machines</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.FSM/IState
    /// 
    public interface IState
    {
        /// <summary>Can this state be entered?</summary>
        /// <remarks>
        /// Called by <see cref="StateMachine{TState}.CanSetState"/>, <see cref="StateMachine{TState}.TrySetState"/>
        /// and <see cref="StateMachine{TState}.TryResetState"/>.
        /// <para></para>
        /// Not called by <see cref="StateMachine{TState}.ForceSetState"/>.
        /// </remarks>
        bool CanEnterState { get; }

        /// <summary>Can this state be exited?</summary>
        /// <remarks>
        /// Called by <see cref="StateMachine{TState}.CanSetState"/>, <see cref="StateMachine{TState}.TrySetState"/>
        /// and <see cref="StateMachine{TState}.TryResetState"/>.
        /// <para></para>
        /// Not called by <see cref="StateMachine{TState}.ForceSetState"/>.
        /// </remarks>
        bool CanExitState { get; }

        /// <summary>Called when this state is entered.</summary>
        /// <remarks>
        /// Called by <see cref="StateMachine{TState}.TrySetState"/>, <see cref="StateMachine{TState}.TryResetState"/>
        /// and <see cref="StateMachine{TState}.ForceSetState"/>.
        /// </remarks>
        void OnEnterState();

        /// <summary>Called when this state is exited.</summary>
        /// <remarks>
        /// Called by <see cref="StateMachine{TState}.TrySetState"/>, <see cref="StateMachine{TState}.TryResetState"/>
        /// and <see cref="StateMachine{TState}.ForceSetState"/>.
        /// </remarks>
        void OnExitState();
    }

    /************************************************************************************************************************/

    /// <summary>An <see cref="IState"/> that knows which <see cref="StateMachine{TState}"/> it is used in.</summary>
    /// <remarks>
    /// The <see cref="StateExtensions"/> class contains various extension methods for this interface.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/fsm">Finite State Machines</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.FSM/IOwnedState_1
    public interface IOwnedState<TState> : IState where TState : class, IState
    {
        /// <summary>The <see cref="StateMachine{TState}"/> that this state is used in.</summary>
        StateMachine<TState> OwnerStateMachine { get; }
    }

    /************************************************************************************************************************/

    /// <summary>An empty <see cref="IState"/> that implements all the required methods as <c>virtual</c>.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/fsm">Finite State Machines</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.FSM/State
    /// 
    public abstract class State : IState
    {
        /************************************************************************************************************************/

        /// <summary><see cref="IState.CanEnterState"/></summary>
        /// <remarks>Returns true unless overridden.</remarks>
        public virtual bool CanEnterState => true;

        /// <summary><see cref="IState.CanExitState"/></summary>
        /// <remarks>Returns true unless overridden.</remarks>
        public virtual bool CanExitState => true;

        /// <summary><see cref="IState.OnEnterState"/></summary>
        public virtual void OnEnterState() { }

        /// <summary><see cref="IState.OnExitState"/></summary>
        public virtual void OnExitState() { }

        /************************************************************************************************************************/
    }

    /************************************************************************************************************************/

    /// <summary>Various extension methods for <see cref="IState"/> and <see cref="IOwnedState{TState}"/>.</summary>
    /// 
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/fsm">Finite State Machines</see>
    /// </remarks>
    /// 
    /// <example>
    /// <code>
    /// public class Creature : MonoBehaviour
    /// {
    ///     public StateMachine&lt;CreatureState&gt; StateMachine { get; private set; }
    /// }
    /// 
    /// public class CreatureState : StateBehaviour, IOwnedState&lt;CreatureState&gt;
    /// {
    ///     [SerializeField]
    ///     private Creature _Creature;
    ///     public Creature Creature =&gt; _Creature;
    ///     
    ///     public StateMachine&lt;CreatureState&gt; OwnerStateMachine =&gt; _Creature.StateMachine;
    /// }
    /// 
    /// public class CreatureBrain : MonoBehaviour
    /// {
    ///     [SerializeField] private Creature _Creature;
    ///     [SerializeField] private CreatureState _Jump;
    ///     
    ///     private void Update()
    ///     {
    ///         if (Input.GetKeyDown(KeyCode.Space))
    ///         {
    ///             // Normally you would need to refer to both the state machine and the state:
    ///             _Creature.StateMachine.TrySetState(_Jump);
    ///             
    ///             // But since CreatureState implements IOwnedState you can use these extension methods:
    ///             _Jump.TryEnterState();
    ///         }
    ///     }
    /// }
    /// </code>
    /// <h2>Inherited Types</h2>
    /// Unfortunately, if the field type is not the same as the <c>T</c> in the <c>IOwnedState&lt;T&gt;</c>
    /// implementation then attempting to use these extension methods without specifying the generic argument will
    /// give the following error:
    /// <para></para>
    /// <em>The type 'StateType' cannot be used as type parameter 'TState' in the generic type or method
    /// 'StateExtensions.TryEnterState&lt;TState&gt;(TState)'. There is no implicit reference conversion from
    /// 'StateType' to 'Animancer.FSM.IOwnedState&lt;StateType&gt;'.</em>
    /// <para></para>
    /// For example, you might want to access members of a derived state class like this <c>SetTarget</c> method:
    /// <para></para><code>
    /// public class AttackState : CreatureState
    /// {
    ///     public void SetTarget(Transform target) { }
    /// }
    /// 
    /// public class CreatureBrain : MonoBehaviour
    /// {
    ///     [SerializeField] private AttackState _Attack;
    ///     
    ///     private void Update()
    ///     {
    ///         if (Input.GetMouseButtonDown(0))
    ///         {
    ///             _Attack.SetTarget(...)
    ///             // Can't do _Attack.TryEnterState();
    ///             _Attack.TryEnterState&lt;CreatureState&gt;();
    ///         }
    ///     }
    /// }
    /// </code>
    /// Unlike the <c>_Jump</c> example, the <c>_Attack</c> field is an <c>AttackState</c> rather than the base
    /// <c>CreatureState</c> so we can call <c>_Attack.SetTarget(...)</c> but that causes problems with these extension
    /// methods.
    /// <para></para>
    /// Calling the method without specifying its generic argument automatically uses the variable's type as the
    /// argument so both of the following calls do the same thing:
    /// <para></para><code>
    /// _Attack.TryEnterState();
    /// _Attack.TryEnterState&lt;AttackState&gt;();
    /// </code>
    /// <para></para>
    /// The problem is that <c>AttackState</c> inherits the implementation of <c>IOwnedState</c> from the base
    /// <c>CreatureState</c> class. But since that implementation is <c>IOwnedState&lt;CreatureState&gt;</c>, rather
    /// than <c>IOwnedState&lt;AttackState&gt;</c> that means <c>TryEnterState&lt;AttackState&gt;</c> does not satisfy
    /// that method's generic constraints: <c>where TState : class, IOwnedState&lt;TState&gt;</c>
    /// <para></para>
    /// That is why you simply need to specify the base class which implements <c>IOwnedState</c> as the generic
    /// argument to prevent it from inferring the wrong type:
    /// <para></para><code>
    /// _Attack.TryEnterState&lt;CreatureState&gt;();
    /// </code>  
    /// </example>
    /// https://kybernetik.com.au/animancer/api/Animancer.FSM/StateExtensions
    [HelpURL(APIDocumentationURL + nameof(StateExtensions))]
    public static class StateExtensions
    {
        /************************************************************************************************************************/

        /// <summary>The URL of the API documentation for the <see cref="FSM"/> system.</summary>
        public const string APIDocumentationURL = "https://kybernetik.com.au/animancer/api/Animancer.FSM/";

        /************************************************************************************************************************/

        /// <summary>Returns the <see cref="StateChange{TState}.PreviousState"/>.</summary>
        public static TState GetPreviousState<TState>(this TState state)
            where TState : class, IState
            => StateChange<TState>.PreviousState;

        /// <summary>Returns the <see cref="StateChange{TState}.NextState"/>.</summary>
        public static TState GetNextState<TState>(this TState state)
            where TState : class, IState
            => StateChange<TState>.NextState;

        /************************************************************************************************************************/

        /// <summary>
        /// Checks if the specified `state` is the <see cref="StateMachine{TState}.CurrentState"/> in its
        /// <see cref="IOwnedState{TState}.OwnerStateMachine"/>.
        /// </summary>
        public static bool IsCurrentState<TState>(this TState state)
            where TState : class, IOwnedState<TState>
            => state.OwnerStateMachine.CurrentState == state;

        /************************************************************************************************************************/

        /// <summary>
        /// Attempts to enter the specified `state` and returns true if successful.
        /// <para></para>
        /// This method returns true immediately if the specified `state` is already the
        /// <see cref="StateMachine{TState}.CurrentState"/>. To allow directly re-entering the same state, use
        /// <see cref="TryReEnterState"/> instead.
        /// </summary>
        public static bool TryEnterState<TState>(this TState state)
            where TState : class, IOwnedState<TState>
            => state.OwnerStateMachine.TrySetState(state);

        /************************************************************************************************************************/

        /// <summary>
        /// Attempts to enter the specified `state` and returns true if successful.
        /// <para></para>
        /// This method does not check if the `state` is already the <see cref="StateMachine{TState}.CurrentState"/>.
        /// To do so, use <see cref="TryEnterState"/> instead.
        /// </summary>
        public static bool TryReEnterState<TState>(this TState state)
            where TState : class, IOwnedState<TState>
            => state.OwnerStateMachine.TryResetState(state);

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="IState.OnExitState"/> on the <see cref="StateMachine{TState}.CurrentState"/> then
        /// changes to the specified `state` and calls <see cref="IState.OnEnterState"/> on it.
        /// <para></para>
        /// This method does not check <see cref="IState.CanExitState"/> or
        /// <see cref="IState.CanEnterState"/>. To do that, you should use <see cref="TrySetState"/> instead.
        /// </summary>
        public static void ForceEnterState<TState>(this TState state)
            where TState : class, IOwnedState<TState>
            => state.OwnerStateMachine.ForceSetState(state);

        /************************************************************************************************************************/
#pragma warning disable IDE0079 // Remove unnecessary suppression.
#pragma warning disable CS1587 // XML comment is not placed on a valid language element.
#pragma warning restore IDE0079 // Remove unnecessary suppression.
        // Copy this #region into a class which implements IOwnedState to give it the state extension methods as regular members.
        // This will avoid any issues with the compiler inferring the wrong generic argument in the extension methods.
        ///************************************************************************************************************************/
        //#region State Extensions
        ///************************************************************************************************************************/

        ///// <summary>
        ///// Checks if this state is the <see cref="StateMachine{TState}.CurrentState"/> in its
        ///// <see cref="IOwnedState{TState}.OwnerStateMachine"/>.
        ///// </summary>
        //public bool IsCurrentState() => OwnerStateMachine.CurrentState == this;

        ///************************************************************************************************************************/

        ///// <summary>
        ///// Calls <see cref="StateMachine{TState}.TrySetState(TState)"/> on the
        ///// <see cref="IOwnedState{TState}.OwnerStateMachine"/>.
        ///// </summary>
        //public bool TryEnterState() => OwnerStateMachine.TrySetState(this);

        ///************************************************************************************************************************/

        ///// <summary>
        ///// Calls <see cref="StateMachine{TState}.TryResetState(TState)"/> on the
        ///// <see cref="IOwnedState{TState}.OwnerStateMachine"/>.
        ///// </summary>
        //public bool TryReEnterState() => OwnerStateMachine.TryResetState(this);

        ///************************************************************************************************************************/

        ///// <summary>
        ///// Calls <see cref="StateMachine{TState}.ForceSetState(TState)"/> on the
        ///// <see cref="IOwnedState{TState}.OwnerStateMachine"/>.
        ///// </summary>
        //public void ForceEnterState() => OwnerStateMachine.ForceSetState(this);

        ///************************************************************************************************************************/
        //#endregion
        ///************************************************************************************************************************/
    }
}
