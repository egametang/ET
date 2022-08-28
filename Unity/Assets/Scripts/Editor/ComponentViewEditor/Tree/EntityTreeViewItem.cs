#if ENABLE_VIEW

using System;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;
using UnityEditor.IMGUI.Controls;

namespace ET
{
    public class EntityTreeViewItem: TreeViewItem
    {
        public Entity entity;

        public EntityTreeViewItem(Entity entity, int id)
        {
            this.entity = entity;
            base.id     = id;
        }

        public override string displayName
        {
            get
            {
                if(!string.IsNullOrEmpty(_displayName))
                {
                    return _displayName;
                }

                string name = this.entity.GetType().Name;

                string debugger_name = ReadDebuggerDisplay(entity);

                _displayName = string.IsNullOrEmpty(debugger_name)? name : $"{name}{debugger_name}";

                return _displayName;
            }
        }

        private string _displayName;

        // https://stackoverflow.com/a/13650728/37055
        private static object ReadProperty(object target, string propertyName)
        {
            var args = new[] { CSharpArgumentInfo.Create(0, null) };
            var binder = Binder.GetMember(
                0,
                propertyName,
                target.GetType(),
                args
            );
            var site = CallSite<Func<CallSite, object, object>>.Create(binder);
            return site.Target(site, target);
        }

        private static string ReadDebuggerDisplay(object target, string propertyName = "ViewGoName")
        {
            string debuggerDisplay = string.Empty;
            try
            {
                var value = ReadProperty(target, propertyName) ?? string.Empty;

                debuggerDisplay = value as string ?? value.ToString();
            }
            catch(Exception)
            {
                // ignored
            }

            return debuggerDisplay;
        }
    }
}
#endif