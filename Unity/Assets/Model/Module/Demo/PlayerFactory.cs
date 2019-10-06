namespace ETModel
{
    public static class PlayerFactory
    {
        public static Player Create(Entity domain, long id)
        {
            Player player = EntityFactory.CreateWithId<Player>(domain, id);
            PlayerComponent playerComponent = Game.Scene.GetComponent<PlayerComponent>();
            playerComponent.Add(player);
            return player;
        }
    }
}