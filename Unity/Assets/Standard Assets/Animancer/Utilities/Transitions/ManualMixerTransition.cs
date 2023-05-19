// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// <summary>A <see cref="ScriptableObject"/> which holds a <see cref="ManualMixerState.Transition"/>.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions/types#transition-assets">Transition Assets</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/ManualMixerTransition
    /// 
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Mixer Transition/Manual", order = Strings.AssetMenuOrder + 1)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(ManualMixerTransition))]
    public class ManualMixerTransition : AnimancerTransition<ManualMixerState.Transition> { }
}
