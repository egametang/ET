// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// <summary>A <see cref="ScriptableObject"/> which holds a <see cref="Float2ControllerState.Transition"/>.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions/types#transition-assets">Transition Assets</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/Float2ControllerTransition
    /// 
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Controller Transition/Float 2", order = Strings.AssetMenuOrder + 6)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(Float2ControllerTransition))]
    public class Float2ControllerTransition : AnimancerTransition<Float2ControllerState.Transition> { }
}
