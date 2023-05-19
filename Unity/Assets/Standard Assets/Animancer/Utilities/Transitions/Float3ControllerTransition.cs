// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// <summary>A <see cref="ScriptableObject"/> which holds a <see cref="Float3ControllerState.Transition"/>.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions/types#transition-assets">Transition Assets</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/Float3ControllerTransition
    /// 
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Controller Transition/Float 3", order = Strings.AssetMenuOrder + 7)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(Float3ControllerTransition))]
    public class Float3ControllerTransition : AnimancerTransition<Float3ControllerState.Transition> { }
}
