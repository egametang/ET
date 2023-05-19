// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// <summary>A <see cref="ScriptableObject"/> which holds a <see cref="ControllerState.Transition"/>.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions/types#transition-assets">Transition Assets</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/ControllerTransition
    /// 
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Controller Transition/Base", order = Strings.AssetMenuOrder + 4)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(ControllerTransition))]
    public class ControllerTransition : AnimancerTransition<ControllerState.Transition> { }
}
