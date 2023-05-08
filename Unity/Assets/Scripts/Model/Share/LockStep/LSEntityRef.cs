namespace ET
{
    public readonly struct LSEntityRef<T> where T: LSEntity
    {
        private readonly long id;
        private readonly T entity;

        private LSEntityRef(T t)
        {
            this.id = t.Id;
            this.entity = t;
        }
        
        private T UnWrap
        {
            get
            {
                if (this.entity == null)
                {
                    return null;
                }
                if (this.entity.Id != this.id)
                {
                    return null;
                }
                return this.entity;
            }
        }
        
        public static implicit operator LSEntityRef<T>(T t)
        {
            return new LSEntityRef<T>(t);
        }

        public static implicit operator T(LSEntityRef<T> v)
        {
            return v.UnWrap;
        }
    }
}