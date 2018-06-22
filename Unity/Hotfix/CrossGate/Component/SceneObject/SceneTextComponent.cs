using System.Collections.Generic;
using UnityEngine;

namespace ETHotfix
{
    public class SceneTextComponent: Component
    {
        private readonly List<GameObject> Texts = new List<GameObject>();
        
        public void SetName(GameObject go, Color color, string playername, int level)
        {
            go.GetComponent<SceneText>().Show("Lv." + level + " " + playername, color);
            Texts.Add(go);
        }

        public void SetTitle(GameObject go, Color color, string title)
        {
            go.GetComponent<SceneText>().Show(title, color);
            Texts.Add(go);
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            for (int i = 0; i < Texts.Count; i++)
            {
                GameObjectPoolComponent.Instance.Release(Texts[i], GameObjectType.SceneText);
            }

            Texts.Clear();
        }
    }
}
