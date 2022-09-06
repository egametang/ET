namespace ET
{
    public abstract class AEntityMenuHandler
    {
        internal string menuName;

        public abstract void OnClick(Entity entity);
    }
}