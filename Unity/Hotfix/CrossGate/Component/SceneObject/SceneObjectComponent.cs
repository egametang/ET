using System.Collections.Generic;
using System.Linq;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class SceneObjectComponentAwakeSystem : AwakeSystem<SceneObjectComponent>
    {
        public override void Awake(SceneObjectComponent self)
        {
            self.Awake();
        }
    }

    public class SceneObjectComponent : Component
    {
        public static SceneObjectComponent Instance { get; private set; }

        public SceneObject MyPlayer;

        private readonly Dictionary<long, SceneObject> idSceneObjects = new Dictionary<long, SceneObject>();

        public void Awake()
        {
            Instance = this;
        }

        public void Add(SceneObject SceneObject)
        {
            this.idSceneObjects.Add(SceneObject.UserID, SceneObject);
        }

        public SceneObject Get(long userid)
        {
            SceneObject SceneObject;
            this.idSceneObjects.TryGetValue(userid, out SceneObject);
            return SceneObject;
        }

        public void Remove(long userid)
        {
            this.idSceneObjects.Remove(userid);
        }

        public int Count
        {
            get
            {
                return this.idSceneObjects.Count;
            }
        }

        public SceneObject[] GetAll()
        {
            return this.idSceneObjects.Values.ToArray();
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            foreach (SceneObject SceneObject in this.idSceneObjects.Values)
            {
                SceneObject.Dispose();
            }

            Instance = null;
        }
    }
}