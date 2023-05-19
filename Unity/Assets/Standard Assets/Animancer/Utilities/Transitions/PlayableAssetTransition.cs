// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// <summary>A <see cref="ScriptableObject"/> which holds a <see cref="PlayableAssetState.Transition"/>.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions/types#transition-assets">Transition Assets</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/PlayableAssetTransition
    /// 
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Playable Asset Transition", order = Strings.AssetMenuOrder + 8)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(PlayableAssetTransition))]
    public class PlayableAssetTransition : AnimancerTransition<PlayableAssetState.Transition> { }
}
