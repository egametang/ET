using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(UI))]
    public static partial class UISystem
    {
        [EntitySystem]
        private static void Awake(this UI self, string name, GameObject gameObject)
        {
            self.nameChildren.Clear();
            gameObject.layer = LayerMask.NameToLayer(LayerNames.UI);
            self.Name = name;
            self.GameObject = gameObject;
        }
		
        [EntitySystem]
        private static void Destroy(this UI self)
        {
            foreach (UI ui in self.nameChildren.Values)
            {
                ui.Dispose();
            }
		
            UnityEngine.Object.Destroy(self.GameObject);
            self.nameChildren.Clear();
        }

        public static void SetAsFirstSibling(this UI self)
        {
            self.GameObject.transform.SetAsFirstSibling();
        }

        public static void Add(this UI self, UI ui)
        {
            self.nameChildren.Add(ui.Name, ui);
        }

        public static void Remove(this UI self, string name)
        {
            EntityRef<UI> uiRef;
            if (!self.nameChildren.Remove(name, out uiRef))
            {
                return;
            }

            UI ui = uiRef;
            ui?.Dispose();
        }

        public static UI Get(this UI self, string name)
        {
            EntityRef<UI> uiRef;
            if (self.nameChildren.TryGetValue(name, out uiRef))
            {
                return uiRef;
            }
            GameObject childGameObject = self.GameObject.transform.Find(name)?.gameObject;
            if (childGameObject == null)
            {
                return null;
            }
            UI child = self.AddChild<UI, string, GameObject>(name, childGameObject);
            self.Add(child);
            return child;
        }
    }
    
    [ChildOf()]
    public sealed class UI: Entity, IAwake<string, GameObject>, IDestroy
    {
        public GameObject GameObject { get; set; }
		
        public string Name { get; set; }

        public Dictionary<string, EntityRef<UI>> nameChildren = new();
    }
}