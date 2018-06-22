using System.Collections.Generic;
using UnityEngine;

namespace ETHotfix
{
    [ETModel.ObjectSystem]
    public class UiAwakeSystem : AwakeSystem<UI, GameObject>
    {
        public override void Awake(UI self, GameObject gameObject)
        {
            self.Awake(gameObject);
        }
    }

    public sealed class UI : Entity
    {
        public string Name
        {
            get
            {
                return this.GameObject.name;
            }
        }

        public GameObject GameObject { get; private set; }

        public Dictionary<string, UI> children = new Dictionary<string, UI>();

        //扩展
        public UIBaseComponent UiComponent { get; private set; }

        public void Awake(GameObject gameObject)
        {
            this.children.Clear();
            this.GameObject = gameObject;
        }

        public override void Dispose()
        {
            if (this.Id == 0)
            {
                return;
            }

            base.Dispose();

            //设置为空
            UiComponent = null;

            foreach (UI ui in this.children.Values)
            {
                ui.Dispose();
            }

            UnityEngine.Object.Destroy(GameObject);
            children.Clear();
        }

        public void SetAsFirstSibling()
        {
            this.GameObject.transform.SetAsFirstSibling();
        }

        public void Add(UI ui)
        {
            this.children.Add(ui.Name, ui);
            ui.Parent = this;
        }

        public void Remove(string name)
        {
            UI ui;
            if (!this.children.TryGetValue(name, out ui))
            {
                return;
            }
            this.children.Remove(name);
            ui.Dispose();
        }

        public UI Get(string name)
        {
            UI child;
            if (this.children.TryGetValue(name, out child))
            {
                return child;
            }
            GameObject childGameObject = this.GameObject.transform.Find(name)?.gameObject;
            if (childGameObject == null)
            {
                return null;
            }
            child = ComponentFactory.Create<UI, GameObject>(childGameObject);
            this.Add(child);
            return child;
        }

        /// <summary>
        /// 添加主UI组件，继承自UIBaseComponent
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <returns></returns>
        public K AddUiComponent<K>() where K : UIBaseComponent, new()
        {
            UiComponent = this.AddComponent<K>();
            return (K)UiComponent;
        }

        public K AddUiComponent<K, P1>(P1 p1) where K : UIBaseComponent, new()
        {
            UiComponent = this.AddComponent<K, P1>(p1);
            return (K)UiComponent;
        }

        public K AddUiComponent<K, P1, P2>(P1 p1, P2 p2) where K : UIBaseComponent, new()
        {
            UiComponent = this.AddComponent<K, P1, P2>(p1, p2);
            return (K)UiComponent;
        }

        public K AddUiComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3) where K : UIBaseComponent, new()
        {
            UiComponent = this.AddComponent<K, P1, P2, P3>(p1, p2, p3);
            return (K)UiComponent;
        }
    }
}