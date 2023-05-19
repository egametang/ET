// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// <summary>A <see cref="ScriptableObject"/> which holds a <see cref="LinearMixerState.Transition"/>.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions/types#transition-assets">Transition Assets</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/LinearMixerTransition
    /// 
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Mixer Transition/Linear", order = Strings.AssetMenuOrder + 2)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(LinearMixerTransition))]
    public class LinearMixerTransition : AnimancerTransition<LinearMixerState.Transition> { }
}
