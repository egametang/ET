// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// <summary>Various string constants used throughout Animancer.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/Strings
    /// 
    public static class Strings
    {
        /************************************************************************************************************************/

        /// <summary>The standard prefix for <see cref="CreateAssetMenuAttribute.menuName"/>.</summary>
        public const string MenuPrefix = "Animancer/";

        /// <summary>The standard prefix for <see cref="CreateAssetMenuAttribute.menuName"/>.</summary>
        public const string ExamplesMenuPrefix = "Animancer/Examples/";

        /// <summary>
        /// The base value for <see cref="CreateAssetMenuAttribute.order"/> to group
        /// "Assets/Create/Animancer/..." menu items just under "Avatar Mask".
        /// </summary>
        public const int AssetMenuOrder = 410;

        /************************************************************************************************************************/

        /// <summary>The conditional compilation symbol for Editor-Only code.</summary>
        public const string UnityEditor = "UNITY_EDITOR";

        /// <summary>The conditional compilation symbol for assertions.</summary>
        public const string Assertions = "UNITY_ASSERTIONS";

        /************************************************************************************************************************/

        /// <summary>4 spaces for indentation.</summary>
        public const string Indent = "    ";

        /// <summary>[Internal]
        /// A prefix for tooltips on Pro-Only features.
        /// <para></para>
        /// "[Pro-Only] " in Animancer Lite or "" in Animancer Pro.
        /// </summary>
        internal const string ProOnlyTag = "";

        /************************************************************************************************************************/

        /// <summary>URLs of various documentation pages.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/DocsURLs
        /// 
        public static class DocsURLs
        {
            /************************************************************************************************************************/

            /// <summary>The URL of the website where the Animancer documentation is hosted.</summary>
            public const string Documentation = "https://kybernetik.com.au/animancer";

            /// <summary>The URL of the website where the Animancer API documentation is hosted.</summary>
            public const string APIDocumentation = Documentation + "/api/" + nameof(Animancer);

            /// <summary>The URL of the website where the Animancer API documentation is hosted.</summary>
            public const string ExampleAPIDocumentation = APIDocumentation + ".Examples.";

            /// <summary>The email address which handles support for Animancer.</summary>
            public const string DeveloperEmail = "animancer@kybernetik.com.au";

            /************************************************************************************************************************/

            public const string OptionalWarning = APIDocumentation + "/" + nameof(Animancer.OptionalWarning);

            /************************************************************************************************************************/
#if UNITY_ASSERTIONS
            /************************************************************************************************************************/

            public const string Docs = Documentation + "/docs/";

            public const string SharedEventSequences = Docs + "manual/events/animancer#shared-event-sequences";

            /************************************************************************************************************************/
#endif
            /************************************************************************************************************************/
#if UNITY_EDITOR
            /************************************************************************************************************************/

            public const string Examples = Docs + "examples";

            public const string UnevenGround = Docs + "examples/ik/uneven-ground";

            public const string AnimancerTools = Docs + "manual/tools";
            public const string ModifySprites = AnimancerTools + "/modify-sprites";
            public const string RenameSprites = AnimancerTools + "/rename-sprites";
            public const string GenerateSpriteAnimations = AnimancerTools + "/generate-sprite-animations";
            public const string RemapSpriteAnimation = AnimancerTools + "/remap-sprite-animation";
            public const string RemapAnimationBindings = AnimancerTools + "/remap-animation-bindings";

            public const string Fading = Docs + "manual/blending/fading";

            public const string Inspector = Docs + "manual/playing/inspector";

            public const string Layers = Docs + "manual/blending/layers";

            public const string EndEvents = Docs + "manual/events/end";

            public const string States = Docs + "manual/playing/states";

            public const string UpdateModes = Docs + "bugs/update-modes";

            public const string ChangeLogPrefix = Docs + "changes/animancer-";

            /************************************************************************************************************************/
#endif
            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
    }
}

