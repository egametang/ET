using System;
using System.Collections.Generic;
using ETModel;
using UnityEngine;

namespace ETHotfix
{
    public enum GameObjectType
    {
        BaseTile,
        TopTile,
        SceneObject,
        SceneText,
    }

    [ObjectSystem]
    public class GameObjectPoolComponentAwakeSystem : AwakeSystem<GameObjectPoolComponent>
    {
        public override void Awake(GameObjectPoolComponent self)
        {
            self.Awake();
        }
    }

    public class GameObjectPoolComponent : Component
    {
        public static GameObjectPoolComponent Instance { get; private set; }
        private ResourcesComponent resourcesComponent { get; set; }

        //对象池
        private readonly Stack<GameObject> BaseTilePool = new Stack<GameObject>();
        private readonly Stack<GameObject> TopTilePool = new Stack<GameObject>();
        private readonly Stack<GameObject> SceneObjectPool = new Stack<GameObject>();
        private readonly Stack<GameObject> SceneTextPool = new Stack<GameObject>();

        public void Awake()
        {
            Instance = this;
            resourcesComponent = ETModel.Game.Scene.GetComponent<ResourcesComponent>();
        }

        public GameObject Get(GameObjectType type)
        {
            GameObject go = null;
            switch (type)
            {
                case GameObjectType.BaseTile:
                    if (BaseTilePool.Count > 0) go = BaseTilePool.Pop();
                    break;
                case GameObjectType.TopTile:
                    if (BaseTilePool.Count > 0) go = BaseTilePool.Pop();
                    break;
                case GameObjectType.SceneObject:
                    if (SceneObjectPool.Count > 0) go = SceneObjectPool.Pop();
                    break;
                case GameObjectType.SceneText:
                    if (SceneTextPool.Count > 0) go = SceneTextPool.Pop();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            if (go != null)
            {
                go.SetActive(true);
                return go;
            }
            resourcesComponent.LoadBundle(type + ".unity3d");
            GameObject bundleprefab = (GameObject)resourcesComponent.GetAsset(type.ToString().ToLower() + ".unity3d", type.ToString().ToLower());
            GameObject newgo = UnityEngine.Object.Instantiate(bundleprefab);
            return newgo;
        }

        public void Release(GameObject go, GameObjectType type)
        {
            go.SetActive(false);
            GameObject parent = GameObject.Find($"/Global/Pool/" + type);
            go.transform.SetParent(parent.transform, false);
            BaseTilePool.Push(go);
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            foreach (var go in BaseTilePool)
            {
                UnityEngine.Object.Destroy(go);
            }
            BaseTilePool.Clear();

            foreach (var go in TopTilePool)
            {
                UnityEngine.Object.Destroy(go);
            }
            TopTilePool.Clear();

            foreach (var go in SceneObjectPool)
            {
                UnityEngine.Object.Destroy(go);
            }
            SceneObjectPool.Clear();

            foreach (var go in SceneTextPool)
            {
                UnityEngine.Object.Destroy(go);
            }
            SceneTextPool.Clear();
        }
    }
}