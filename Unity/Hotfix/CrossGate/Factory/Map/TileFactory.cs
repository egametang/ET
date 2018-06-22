using ETHotfix;
using UnityEngine;

namespace ETHotFix
{
    public static class TileFactory
    {
        public static Tile CreateBase()
        {
            Tile tile = ComponentFactory.Create<Tile>();
            GameObject go = GameObjectPoolComponent.Instance.Get(GameObjectType.BaseTile);
            GameObject parent = GameObject.Find($"/Global/Map");
            go.transform.SetParent(parent.transform, false);
            tile.BaseTileGameObject = go;
            tile.BaseTileComponent = ComponentFactory.Create<StaticSpriteComponent, SpriteRenderer>(go.GetComponent<SpriteRenderer>());
            return tile;
        }

        public static void CreateTop(ref Tile tile)
        {
            GameObject go = GameObjectPoolComponent.Instance.Get(GameObjectType.TopTile);
            tile.TopTileComponent = ComponentFactory.Create<StaticSpriteComponent, SpriteRenderer>(go.GetComponent<SpriteRenderer>());
            tile.TopTileGameObject = go;
            go.transform.SetParent(tile.BaseTileGameObject.transform, false);
        }
    }
}