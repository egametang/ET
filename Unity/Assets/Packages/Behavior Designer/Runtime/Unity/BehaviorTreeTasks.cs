using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
    [RequireComponent(typeof(BehaviorTree))]
    public class BehaviorTreeTasks : MonoBehaviour
    {
        public readonly List<HotfixAction> hotfixActions = new List<HotfixAction>();
        public readonly List<HotfixComposite> hotfixComposites = new List<HotfixComposite>();
        public readonly List<HotfixConditional> hotfixConditionals = new List<HotfixConditional>();
        public readonly List<HotfixDecorator> hotfixDecorators = new List<HotfixDecorator>();

        private bool isInit = false;

        void Awake()
        {
            Init();
        }

        public void Init()
        {
            if (!isInit)
            {
                var behaviorTree = GetComponent<BehaviorTree>();

                if (behaviorTree)
                {
                    var hotfixActionList = behaviorTree.FindTasks<HotfixAction>();

                    if (hotfixActionList != null)
                    {
                        hotfixActions.AddRange(hotfixActionList);
                    }

                    var hotfixCompositeList = behaviorTree.FindTasks<HotfixComposite>();

                    if (hotfixCompositeList != null)
                    {
                        hotfixComposites.AddRange(hotfixCompositeList);
                    }

                    var hotfixConditionalList = behaviorTree.FindTasks<HotfixConditional>();

                    if (hotfixConditionalList != null)
                    {
                        hotfixConditionals.AddRange(hotfixConditionalList);
                    }

                    var hotfixDecoratorList = behaviorTree.FindTasks<HotfixDecorator>();

                    if (hotfixDecoratorList != null)
                    {
                        hotfixDecorators.AddRange(hotfixDecoratorList);
                    }

                }                

                isInit = true;
            }
        }
    }
}

