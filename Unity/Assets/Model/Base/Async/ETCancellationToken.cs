using System;
using System.Collections.Generic;

namespace ETModel
{
    [ObjectSystem]
    public class ETCancellationTokenDestroySystem: DestroySystem<ETCancellationToken>
    {
        public override void Destroy(ETCancellationToken self)
        {
            self.actions.Clear();
        }
    }
    
    public class ETCancellationToken: Entity
    {
        public readonly List<Action> actions = new List<Action>();

        public void Register(Action callback)
        {
            this.actions.Add(callback);
        }

        public void Cancel()
        {
            foreach (Action action in this.actions)
            {
                action.Invoke();
            }
        }
    }
}