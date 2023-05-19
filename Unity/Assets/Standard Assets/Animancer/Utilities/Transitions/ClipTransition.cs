// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// <summary>A <see cref="ScriptableObject"/> which holds a <see cref="ClipState.Transition"/>.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions/types#transition-assets">Transition Assets</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/ClipTransition
    /// 
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Clip Transition", order = Strings.AssetMenuOrder + 0)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(ClipTransition))]
    public class ClipTransition : AnimancerTransition<ClipState.Transition> { }
}
