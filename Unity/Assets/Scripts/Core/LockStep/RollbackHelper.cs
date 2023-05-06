namespace ET
{
    public static class RollbackHelper
    {
        public static void Rollback(Entity entity)
        {
            if (entity is LSEntity)
            {
                return;
            }
            
            LSSington.Instance.Rollback(entity);
            
            if (entity.ComponentsCount() > 0)
            {
                foreach (var kv in entity.Components)
                {
                    Rollback(kv.Value);
                }
            }

            if (entity.ChildrenCount() > 0)
            {
                foreach (var kv in entity.Children)
                {
                    Rollback(kv.Value);
                }
            }
        }
    }
}