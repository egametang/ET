namespace ET.Server
{
    public class HttpHandlerAttribute: BaseAttribute
    {
        public int SceneType { get; }

        public string Path { get; }

        public HttpHandlerAttribute(int sceneType, string path)
        {
            this.SceneType = sceneType;
            this.Path = path;
        }
    }
}