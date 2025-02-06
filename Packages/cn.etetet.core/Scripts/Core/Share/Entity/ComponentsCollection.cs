using System.Diagnostics;

namespace ET
{
    internal sealed class ComponentsCollectionDebugView    
    {
        private readonly ComponentsCollection _collection;

        public ComponentsCollectionDebugView(ComponentsCollection collection)
        {
            ArgumentNullException.ThrowIfNull(collection);

            _collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public Entity[] Items
        {
            get
            {
                Entity[] items = new Entity[_collection.Count];
                _collection.Values.CopyTo(items, 0);
                return items;
            }
        }
    }
    
    [DebuggerTypeProxy(typeof(ComponentsCollectionDebugView))]
    [DebuggerDisplay("Count = {Count}")]
    public class ComponentsCollection : SortedDictionary<long, Entity>, IPool
    {
        public static ComponentsCollection Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ComponentsCollection>(isFromPool);
        }
        
        public bool IsFromPool { get; set; }

        public void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }
            
            this.IsFromPool = false;
            this.Clear();
            
            ObjectPool.Recycle(this);
        }
    }
}