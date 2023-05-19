// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// <summary>A <see cref="ScriptableObject"/> which holds a <see cref="Float1ControllerState.Transition"/>.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions/types#transition-assets">Transition Assets</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/Float1ControllerTransition
    /// 
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Controller Transition/Float 1", order = Strings.AssetMenuOrder + 5)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(Float1ControllerTransition))]
    public class Float1ControllerTransition : AnimancerTransition<Float1ControllerState.Transition> { }
}
