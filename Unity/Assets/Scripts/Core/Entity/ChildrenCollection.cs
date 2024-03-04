using System.Collections.Generic;

namespace ET
{
    public class ChildrenCollection : SortedDictionary<long, Entity>
    {
        public ChildrenCollection()
        {
        }

        public ChildrenCollection(IComparer<long> comparer) : base(comparer)
        {
        }

        public ChildrenCollection(IDictionary<long, Entity> dictionary) : base(dictionary)
        {
        }

        public ChildrenCollection(IDictionary<long, Entity> dictionary, IComparer<long> comparer) : base(dictionary, comparer)
        {
        }
    }
}