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
                foreach (Entity component in entity.Components.Values)
                {
                    Rollback(component);
                }
            }

            if (entity.ChildrenCount() > 0)
            {
                foreach (Entity child in entity.Children.Values)
                {
                    Rollback(child);
                }
            }
        }
    }
}