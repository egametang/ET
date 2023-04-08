using System;

namespace ET
{
    public class LSEntity: Entity
    {
        public new K AddComponent<K>(bool isFromPool = false) where K : LSEntity, IAwake, new()
        {
            return base.AddComponentWithId<K>(this.DomainScene().GetId(), isFromPool);
        }

        public new K AddComponent<K, P1>(P1 p1, bool isFromPool = false) where K : LSEntity, IAwake<P1>, new()
        {
            return base.AddComponentWithId<K, P1>(this.DomainScene().GetId(), p1, isFromPool);
        }

        public new K AddComponent<K, P1, P2>(P1 p1, P2 p2, bool isFromPool = false) where K : LSEntity, IAwake<P1, P2>, new()
        {
            return base.AddComponentWithId<K, P1, P2>(this.DomainScene().GetId(), p1, p2, isFromPool);
        }

        public new K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3, bool isFromPool = false) where K : LSEntity, IAwake<P1, P2, P3>, new()
        {
            return base.AddComponentWithId<K, P1, P2, P3>(this.DomainScene().GetId(), p1, p2, p3, isFromPool);
        }

        public new K AddComponentWithId<K>(long id, bool isFromPool = false) where K : Entity, IAwake, new()
        {
            throw new Exception("dont use this");
        }
        
        public new K AddComponentWithId<K, P1>(long id, P1 p1, bool isFromPool = false) where K : Entity, IAwake<P1>, new()
        {
            throw new Exception("dont use this");
        }

        public new K AddComponentWithId<K, P1, P2>(long id, P1 p1, P2 p2, bool isFromPool = false) where K : Entity, IAwake<P1, P2>, new()
        {
            throw new Exception("dont use this");
        }

        public new K AddComponentWithId<K, P1, P2, P3>(long id, P1 p1, P2 p2, P3 p3, bool isFromPool = false) where K : Entity, IAwake<P1, P2, P3>, new()
        {
            throw new Exception("dont use this");
        }
        
        public Entity AddChild(LSEntity entity)
        {
            return entity.AddChild(entity);
        }

        public new T AddChild<T>(bool isFromPool = false) where T : LSEntity, IAwake
        {
            return base.AddChildWithId<T>(this.DomainScene().GetId(), isFromPool);
        }

        public new T AddChild<T, A>(A a, bool isFromPool = false) where T : LSEntity, IAwake<A>
        {
            return base.AddChildWithId<T, A>(this.DomainScene().GetId(), a, isFromPool);
        }

        public new T AddChild<T, A, B>(A a, B b, bool isFromPool = false) where T : LSEntity, IAwake<A, B>
        {
            return base.AddChildWithId<T, A, B>(this.DomainScene().GetId(), a, b, isFromPool);
        }

        public new T AddChild<T, A, B, C>(A a, B b, C c, bool isFromPool = false) where T : LSEntity, IAwake<A, B, C>
        {
            return base.AddChildWithId<T, A, B, C>(this.DomainScene().GetId(), a, b, c, isFromPool);
        }

        public new T AddChildWithId<T>(long id, bool isFromPool = false) where T : LSEntity, IAwake, new()
        {
            throw new Exception("dont use this");
        }

        public new T AddChildWithId<T, A>(long id, A a, bool isFromPool = false) where T : LSEntity, IAwake<A>
        {
            throw new Exception("dont use this");
        }

        public new T AddChildWithId<T, A, B>(long id, A a, B b, bool isFromPool = false) where T : LSEntity, IAwake<A, B>
        {
            throw new Exception("dont use this");
        }

        public new T AddChildWithId<T, A, B, C>(long id, A a, B b, C c, bool isFromPool = false) where T : LSEntity, IAwake<A, B, C>
        {
            throw new Exception("dont use this");
        }
    }
}