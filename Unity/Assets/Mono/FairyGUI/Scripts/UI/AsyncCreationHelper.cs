using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
    public class AsyncCreationHelper
    {
        public static void CreateObject(PackageItem item, UIPackage.CreateObjectCallback callback)
        {
            Timers.inst.StartCoroutine(_CreateObject(item, callback));
        }

        static IEnumerator _CreateObject(PackageItem item, UIPackage.CreateObjectCallback callback)
        {
            Stats.LatestObjectCreation = 0;
            Stats.LatestGraphicsCreation = 0;

            float frameTime = UIConfig.frameTimeForAsyncUIConstruction;

            List<DisplayListItem> itemList = new List<DisplayListItem>();
            DisplayListItem di = new DisplayListItem(item, ObjectType.Component);
            di.childCount = CollectComponentChildren(item, itemList);
            itemList.Add(di);

            int cnt = itemList.Count;
            List<GObject> objectPool = new List<GObject>(cnt);
            GObject obj;
            float t = Time.realtimeSinceStartup;
            bool alreadyNextFrame = false;

            for (int i = 0; i < cnt; i++)
            {
                di = itemList[i];
                if (di.packageItem != null)
                {
                    obj = UIObjectFactory.NewObject(di.packageItem);
                    objectPool.Add(obj);

                    UIPackage._constructing++;
                    if (di.packageItem.type == PackageItemType.Component)
                    {
                        int poolStart = objectPool.Count - di.childCount - 1;

                        ((GComponent)obj).ConstructFromResource(objectPool, poolStart);

                        objectPool.RemoveRange(poolStart, di.childCount);
                    }
                    else
                    {
                        obj.ConstructFromResource();
                    }
                    UIPackage._constructing--;
                }
                else
                {
                    obj = UIObjectFactory.NewObject(di.type);
                    objectPool.Add(obj);

                    if (di.type == ObjectType.List && di.listItemCount > 0)
                    {
                        int poolStart = objectPool.Count - di.listItemCount - 1;
                        for (int k = 0; k < di.listItemCount; k++) //把他们都放到pool里，这样GList在创建时就不需要创建对象了
                            ((GList)obj).itemPool.ReturnObject(objectPool[k + poolStart]);
                        objectPool.RemoveRange(poolStart, di.listItemCount);
                    }
                }

                if ((i % 5 == 0) && Time.realtimeSinceStartup - t >= frameTime)
                {
                    yield return null;
                    t = Time.realtimeSinceStartup;
                    alreadyNextFrame = true;
                }
            }

            if (!alreadyNextFrame) //强制至至少下一帧才调用callback，避免调用者逻辑出错
                yield return null;

            callback(objectPool[0]);
        }

        /// <summary>
        /// 收集创建目标对象所需的所有类型信息
        /// </summary>
        /// <param name="item"></param>
        /// <param name="list"></param>
        static int CollectComponentChildren(PackageItem item, List<DisplayListItem> list)
        {
            ByteBuffer buffer = item.rawData;
            buffer.Seek(0, 2);

            int dcnt = buffer.ReadShort();
            DisplayListItem di;
            PackageItem pi;
            for (int i = 0; i < dcnt; i++)
            {
                int dataLen = buffer.ReadShort();
                int curPos = buffer.position;

                buffer.Seek(curPos, 0);

                ObjectType type = (ObjectType)buffer.ReadByte();
                string src = buffer.ReadS();
                string pkgId = buffer.ReadS();

                buffer.position = curPos;

                if (src != null)
                {
                    UIPackage pkg;
                    if (pkgId != null)
                        pkg = UIPackage.GetById(pkgId);
                    else
                        pkg = item.owner;

                    pi = pkg != null ? pkg.GetItem(src) : null;
                    di = new DisplayListItem(pi, type);

                    if (pi != null && pi.type == PackageItemType.Component)
                        di.childCount = CollectComponentChildren(pi, list);
                }
                else
                {
                    di = new DisplayListItem(null, type);
                    if (type == ObjectType.List) //list
                        di.listItemCount = CollectListChildren(buffer, list);
                }

                list.Add(di);
                buffer.position = curPos + dataLen;
            }

            return dcnt;
        }

        static int CollectListChildren(ByteBuffer buffer, List<DisplayListItem> list)
        {
            buffer.Seek(buffer.position, 8);

            string defaultItem = buffer.ReadS();
            int listItemCount = 0;
            int itemCount = buffer.ReadShort();
            for (int i = 0; i < itemCount; i++)
            {
                int nextPos = buffer.ReadShort();
                nextPos += buffer.position;

                string url = buffer.ReadS();
                if (url == null)
                    url = defaultItem;
                if (!string.IsNullOrEmpty(url))
                {
                    PackageItem pi = UIPackage.GetItemByURL(url);
                    if (pi != null)
                    {
                        DisplayListItem di = new DisplayListItem(pi, pi.objectType);
                        if (pi.type == PackageItemType.Component)
                            di.childCount = CollectComponentChildren(pi, list);

                        list.Add(di);
                        listItemCount++;
                    }
                }
                buffer.position = nextPos;
            }

            return listItemCount;
        }

        /// <summary>
        /// 
        /// </summary>
        class DisplayListItem
        {
            public PackageItem packageItem;
            public ObjectType type;
            public int childCount;
            public int listItemCount;

            public DisplayListItem(PackageItem pi, ObjectType type)
            {
                this.packageItem = pi;
                this.type = type;
            }
        }
    }
}
