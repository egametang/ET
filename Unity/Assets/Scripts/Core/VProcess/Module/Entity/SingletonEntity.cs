namespace ET
{
    public class SingletonEntity<T>: Entity where T: SingletonEntity<T>
    {
        public static T Instance
        {
            get
            {
                return VProcess.Instance.GetComponent<T>();
            }
        }
    }
}