using BehaviorDesigner.Runtime;
using ETModel;
using UnityEngine;

namespace ETHotfix
{
    public class BehaviorTreeHelper
    {
        public static void Init(GameObject go)
        {
            if (go)
            {
                var bts = go.GetComponentsInChildren<BehaviorTree>();

                if(bts != null)
                {
                    foreach(var bt in bts)
                    {
                        if (bt)
                        {
                            bt.CheckForSerialization();
                            bt.gameObject?.Ensure<BehaviorTreeTasks>()?.Init();
                        }
                    }
                }
            }
        }
    }
}
