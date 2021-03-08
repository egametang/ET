namespace ET
{
    public class LocationProxyComponent: Entity
    {
        public static LocationProxyComponent Instance;

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            Instance = null;
        }
    }
}