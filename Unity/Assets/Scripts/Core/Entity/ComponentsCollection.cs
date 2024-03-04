using System.Collections.Generic;

namespace ET
{
    public class ComponentsCollection : SortedDictionary<long, Entity>
    {
        public ComponentsCollection()
        {
        }

        public ComponentsCollection(IComparer<long> comparer) : base(comparer)
        {
        }

        public ComponentsCollection(IDictionary<long, Entity> dictionary) : base(dictionary)
        {
        }

        public ComponentsCollection(IDictionary<long, Entity> dictionary, IComparer<long> comparer) : base(dictionary, comparer)
        {
        }
    }
}