using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace Coffee.UIEffects
{
    public class GraphicConnector
    {
        private static readonly List<GraphicConnector> s_Connectors = new List<GraphicConnector>();

        private static readonly Dictionary<Type, GraphicConnector> s_ConnectorMap =
            new Dictionary<Type, GraphicConnector>();

        private static readonly GraphicConnector s_EmptyConnector = new GraphicConnector();

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            AddConnector(new GraphicConnector());
        }

        protected static void AddConnector(GraphicConnector connector)
        {
            s_Connectors.Add(connector);
            s_Connectors.Sort((x, y) => y.priority - x.priority);
        }

        public static GraphicConnector FindConnector(Graphic graphic)
        {
            if (!graphic) return s_EmptyConnector;

            var type = graphic.GetType();
            GraphicConnector connector = null;
            if (s_ConnectorMap.TryGetValue(type, out connector)) return connector;

            foreach (var c in s_Connectors)
            {
                if (!c.IsValid(graphic)) continue;

                s_ConnectorMap.Add(type, c);
                return c;
            }

            return s_EmptyConnector;
        }

        /// <summary>
        /// Connector priority.
        /// </summary>
        protected virtual int priority
        {
            get { return -1; }
        }

        /// <summary>
        /// Extra channel.
        /// </summary>
        public virtual AdditionalCanvasShaderChannels extraChannel
        {
            get { return AdditionalCanvasShaderChannels.TexCoord1; }
        }

        /// <summary>
        /// The connector is valid for the component.
        /// </summary>
        protected virtual bool IsValid(Graphic graphic)
        {
            return true;
        }

        /// <summary>
        /// Find effect shader.
        /// </summary>
        public virtual Shader FindShader(string shaderName)
        {
            return Shader.Find("Hidden/" + shaderName);
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        public virtual void OnEnable(Graphic graphic)
        {
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled () or inactive.
        /// </summary>
        public virtual void OnDisable(Graphic graphic)
        {
        }

        /// <summary>
        /// Mark the vertices as dirty.
        /// </summary>
        public virtual void SetVerticesDirty(Graphic graphic)
        {
            if (graphic)
                graphic.SetVerticesDirty();
        }

        /// <summary>
        /// Mark the material as dirty.
        /// </summary>
        public virtual void SetMaterialDirty(Graphic graphic)
        {
            if (graphic)
                graphic.SetMaterialDirty();
        }

        /// <summary>
        /// Gets position factor for area.
        /// </summary>
        public virtual void GetPositionFactor(EffectArea area, int index, Rect rect, Vector2 position, out float x, out float y)
        {
            if (area == EffectArea.Fit)
            {
                x = Mathf.Clamp01((position.x - rect.xMin) / rect.width);
                y = Mathf.Clamp01((position.y - rect.yMin) / rect.height);
            }
            else
            {
                x = Mathf.Clamp01(position.x / rect.width + 0.5f);
                y = Mathf.Clamp01(position.y / rect.height + 0.5f);
            }
        }

        public virtual bool IsText(Graphic graphic)
        {
            return graphic && graphic is Text;
        }

        public virtual void SetExtraChannel(ref UIVertex vertex, Vector2 value)
        {
            vertex.uv1 = value;
        }

        /// <summary>
        /// Normalize vertex position by local matrix.
        /// </summary>
        public virtual void GetNormalizedFactor(EffectArea area, int index, Matrix2x3 matrix, Vector2 position,
            out Vector2 normalizedPos)
        {
            normalizedPos = matrix * position;
        }
    }
}
