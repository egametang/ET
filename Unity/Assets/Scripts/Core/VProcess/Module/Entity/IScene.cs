namespace ET
{
    public interface IScene
    {
        IScene Root { get; set; }
        SceneType SceneType { get; set; }
    }
}