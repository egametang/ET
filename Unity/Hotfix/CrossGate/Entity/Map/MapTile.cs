using System.Collections.Generic;

namespace ETHotfix
{
    public class MapTile : Entity
    {
        private readonly Dictionary<string, Tile> tileDict = new Dictionary<string, Tile>();

        public void Remove(string key)
        {
            Tile tile;
            this.tileDict.TryGetValue(key, out tile);
            this.tileDict.Remove(key);
            if (tile != null) tile.Dispose();
        }

        public void Add(Tile tile)
        {
            this.tileDict.Add(tile.Dong + "|" + tile.Nan, tile);
        }

        public Tile Get(string key)
        {
            Tile tile;
            this.tileDict.TryGetValue(key, out tile);
            return tile;
        }

        public void RemoveWithDispose(string key)
        {
            Tile tile;
            this.tileDict.TryGetValue(key, out tile);
            this.tileDict.Remove(key);
            tile?.Dispose();
        }

        public void RemoveNoDispose(string key)
        {
            this.tileDict.Remove(key);
        }

        public int Count
        {
            get
            {
                return this.tileDict.Count;
            }
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            base.Dispose();

            foreach (var tile in tileDict.Values)
            {
                tile.Dispose();
            }

            this.tileDict.Clear();
        }
    }
}
