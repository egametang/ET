namespace ET
{
    public interface IScene
    {
        IScene Root { get; set; }
        VProcess VProcess { get; set; }
        SceneType SceneType { get; set; }
    }
}