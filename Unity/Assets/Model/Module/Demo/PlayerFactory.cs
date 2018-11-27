namespace ETModel
{
    public static class PlayerFactory
    {
        public static Player Create(long id)
        {
            Player player = ComponentFactory.CreateWithId<Player>(id);
            PlayerComponent playerComponent = Game.Scene.GetComponent<PlayerComponent>();
            playerComponent.Add(player);
            return player;
        }
    }
}