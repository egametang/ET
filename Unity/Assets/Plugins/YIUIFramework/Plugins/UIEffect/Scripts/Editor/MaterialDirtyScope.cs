using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Coffee.UIEffects.Editors
{
    /// <summary>
    /// Changes in this scope cause the graphic's material to be dirty.
    /// When you change a property, it marks the material as dirty.
    /// </summary>
    internal class MaterialDirtyScope : EditorGUI.ChangeCheckScope
    {
        readonly Object[] targets;

        public MaterialDirtyScope(Object[] targets)
        {
            this.targets = targets;
        }

        protected override void CloseScope()
        {
            if (changed)
            {
                foreach (var effect in targets.OfType<BaseMaterialEffect>())
                {
                    effect.SetMaterialDirty();
                }
            }

            base.CloseScope();
        }
    }
}
