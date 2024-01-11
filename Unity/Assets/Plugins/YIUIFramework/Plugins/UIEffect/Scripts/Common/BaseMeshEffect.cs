using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Coffee.UIEffects
{
    /// <summary>
    /// Base class for effects that modify the generated Mesh.
    /// It works well not only for standard Graphic components (Image, RawImage, Text, etc.) but also for TextMeshPro and TextMeshProUGUI.
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    [RequireComponent(typeof(RectTransform))]
    [ExecuteInEditMode]
    public abstract class BaseMeshEffect : UIBehaviour, IMeshModifier
    {
        RectTransform _rectTransform;
        Graphic _graphic;
        GraphicConnector _connector;

        /// <summary>
        /// The Graphic attached to this GameObject.
        /// </summary>
        protected GraphicConnector connector
        {
            get { return _connector ?? (_connector = GraphicConnector.FindConnector(graphic)); }
        }

        /// <summary>
        /// The Graphic attached to this GameObject.
        /// </summary>
        public Graphic graphic
        {
            get { return _graphic ? _graphic : _graphic = GetComponent<Graphic>(); }
        }

        /// <summary>
        /// The RectTransform attached to this GameObject.
        /// </summary>
        protected RectTransform rectTransform
        {
            get { return _rectTransform ? _rectTransform : _rectTransform = GetComponent<RectTransform>(); }
        }

        internal readonly List<UISyncEffect> syncEffects = new List<UISyncEffect>(0);

        /// <summary>
        /// Call used to modify mesh. (legacy)
        /// </summary>
        /// <param name="mesh">Mesh.</param>
        public virtual void ModifyMesh(Mesh mesh)
        {
        }

        /// <summary>
        /// Call used to modify mesh.
        /// </summary>
        /// <param name="vh">VertexHelper.</param>
        public virtual void ModifyMesh(VertexHelper vh)
        {
            ModifyMesh(vh, graphic);
        }

        public virtual void ModifyMesh(VertexHelper vh, Graphic graphic)
        {
        }

        /// <summary>
        /// Mark the vertices as dirty.
        /// </summary>
        protected virtual void SetVerticesDirty()
        {
            connector.SetVerticesDirty(graphic);

            foreach (var effect in syncEffects)
            {
                effect.SetVerticesDirty();
            }

// #if TMP_PRESENT
//             if (textMeshPro)
//             {
//                 foreach (var info in textMeshPro.textInfo.meshInfo)
//                 {
//                     var mesh = info.mesh;
//                     if (mesh)
//                     {
//                         mesh.Clear();
//                         mesh.vertices = info.vertices;
//                         mesh.uv = info.uvs0;
//                         mesh.uv2 = info.uvs2;
//                         mesh.colors32 = info.colors32;
//                         mesh.normals = info.normals;
//                         mesh.tangents = info.tangents;
//                         mesh.triangles = info.triangles;
//                     }
//                 }
//
//                 if (canvasRenderer)
//                 {
//                     canvasRenderer.SetMesh(textMeshPro.mesh);
//
//                     GetComponentsInChildren(false, s_SubMeshUIs);
//                     foreach (var sm in s_SubMeshUIs)
//                     {
//                         sm.canvasRenderer.SetMesh(sm.mesh);
//                     }
//
//                     s_SubMeshUIs.Clear();
//                 }
//
//                 textMeshPro.havePropertiesChanged = true;
//             }
//             else
// #endif
//             if (graphic)
//             {
//                 graphic.SetVerticesDirty();
//             }
        }


        //################################
        // Protected Members.
        //################################
        /// <summary>
        /// Should the effect modify the mesh directly for TMPro?
        /// </summary>
        // protected virtual bool isLegacyMeshModifier
        // {
        //     get { return false; }
        // }
//         protected virtual void Initialize()
//         {
//             if (_initialized) return;
//
//             _initialized = true;
//             _graphic = _graphic ? _graphic : GetComponent<Graphic>();
//
//             _connector = GraphicConnector.FindConnector(_graphic);
//
//             // _canvasRenderer = _canvasRenderer ?? GetComponent<CanvasRenderer> ();
//             _rectTransform = _rectTransform ? _rectTransform : GetComponent<RectTransform>();
// // #if TMP_PRESENT
// // 			_textMeshPro = _textMeshPro ?? GetComponent<TMP_Text> ();
// // #endif
//         }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        protected override void OnEnable()
        {
            connector.OnEnable(graphic);
            SetVerticesDirty();

            // SetVerticesDirty();
// #if TMP_PRESENT
            // 			if (textMeshPro)
            // 			{
            // 				TMPro_EventManager.TEXT_CHANGED_EVENT.Add (OnTextChanged);
            // 			}
            // #endif
            //
            // #if UNITY_EDITOR && TMP_PRESENT
            // 			if (graphic && textMeshPro)
            // 			{
            // 				GraphicRebuildTracker.TrackGraphic (graphic);
            // 			}
            // #endif
            //
            // #if UNITY_5_6_OR_NEWER
            // 			if (graphic)
            // 			{
            // 				AdditionalCanvasShaderChannels channels = requiredChannels;
            // 				var canvas = graphic.canvas;
            // 				if (canvas && (canvas.additionalShaderChannels & channels) != channels)
            // 				{
            // 					Debug.LogWarningFormat (this, "Enable {1} of Canvas.additionalShaderChannels to use {0}.", GetType ().Name, channels);
            // 				}
            // 			}
            // #endif
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled () or inactive.
        /// </summary>
        protected override void OnDisable()
        {
            connector.OnDisable(graphic);
            SetVerticesDirty();
        }

        /// <summary>
        /// Mark the effect parameters as dirty.
        /// </summary>
        protected virtual void SetEffectParamsDirty()
        {
            if (!isActiveAndEnabled) return;
            SetVerticesDirty();
        }

        /// <summary>
        /// Callback for when properties have been changed by animation.
        /// </summary>
        protected override void OnDidApplyAnimationProperties()
        {
            if (!isActiveAndEnabled) return;
            SetEffectParamsDirty();
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            if (!isActiveAndEnabled) return;
            SetVerticesDirty();
        }

        /// <summary>
        /// This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only).
        /// </summary>
        protected override void OnValidate()
        {
            if (!isActiveAndEnabled) return;
            SetEffectParamsDirty();
        }
#endif
    }
}
