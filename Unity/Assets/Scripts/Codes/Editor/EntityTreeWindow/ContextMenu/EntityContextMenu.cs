using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class EntityContextMenu
    {
        private static MultiMap<string, AEntityMenuHandler> ACTIONS = new();

        private static GenericMenu menu;

        static EntityContextMenu()
        {
            var types = TypeCache.GetTypesWithAttribute<EntityMenuAttribute>();

            foreach(var type in types)
            {
                var menu = type.GetCustomAttribute<EntityMenuAttribute>();

                if(menu is null)
                {
                    continue;
                }

                if(Activator.CreateInstance(type) is not AEntityMenuHandler action)
                {
                    continue;
                }

                action.menuName = menu.menu_name;
                ACTIONS.Add(menu.bind_to.Name, action);
            }
        }

        public static void Show(object entity)
        {
            if(entity is null)
            {
                return;
            }

            string name = entity.GetType().Name;

            ACTIONS.TryGetValue(name, out var actions);

            if(actions is null)
            {
                return;
            }

            menu = new GenericMenu();

            foreach(var action in actions)
            {
                menu.AddItem(
                    new GUIContent(action.menuName),
                    false,
                    delegate(object data)
                    {
                        if(data is not Entity callback_data)
                        {
                            return;
                        }

                        action.OnClick(callback_data);
                    },
                    entity
                );
            }

            menu.ShowAsContext();
        }
    }
}