using BehaviorDesigner.Runtime;

namespace ETModel
{
    public class BehaviorManagerComponent : Component
    {
        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            base.Dispose();

            BehaviorManager.instance?.OnDestroy();
        }
    }
}
