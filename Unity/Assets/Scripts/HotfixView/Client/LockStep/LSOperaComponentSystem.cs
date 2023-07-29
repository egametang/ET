using TrueSync;
using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(LSOperaComponent))]
    [FriendOf(typeof(LSClientUpdater))]
    public static partial class LSOperaComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.LSOperaComponent self)
        {

        }
        
        [EntitySystem]
        private static void Update(this LSOperaComponent self)
        {
            TSVector2 v = new();
            if (Input.GetKey(KeyCode.W))
            {
                v.y += 1;
            }

            if (Input.GetKey(KeyCode.A))
            {
                v.x -= 1;
            }

            if (Input.GetKey(KeyCode.S))
            {
                v.y -= 1;
            }

            if (Input.GetKey(KeyCode.D))
            {
                v.x += 1;
            }

            LSClientUpdater lsClientUpdater = self.GetParent<Room>().GetComponent<LSClientUpdater>();
            lsClientUpdater.Input.V = v.normalized;
        }

    }
}