namespace ET.Server
{
    public class HttpHandlerAttribute: BaseAttribute
    {
        public SceneType SceneType { get; }

        public string Path { get; }

        public HttpHandlerAttribute(SceneType sceneType, string path)
        {
            this.SceneType = sceneType;
            this.Path = path;
        }
    }
}