using System;

namespace ETModel
{
    public interface IFilter
    {
        Type[] GetFilter();
    }
    
    public abstract class AFilter<A>: IFilter
    {
        public Type[] GetFilter()
        {
            return new[] { typeof (A) };
        }
    }
    
    public abstract class AFilter<A, B>: IFilter
    {
        public Type[] GetFilter()
        {
            return new[] { typeof (A), typeof(B) };
        }
    }
    
    public abstract class AFilter<A, B, C>: IFilter
    {
        public Type[] GetFilter()
        {
            return new[] { typeof (A), typeof(B), typeof(C) };
        }
    }
    
    public abstract class AFilter<A, B, C, D>: IFilter
    {
        public Type[] GetFilter()
        {
            return new[] { typeof (A), typeof(B), typeof(C), typeof(D) };
        }
    }
    
    public abstract class AFilter<A, B, C, D, E>: IFilter
    {
        public Type[] GetFilter()
        {
            return new[] { typeof (A), typeof(B), typeof(C), typeof(D), typeof(E) };
        }
    }
}