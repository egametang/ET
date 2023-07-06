namespace ET.Client
{
    [ActorMessageHandler(SceneType.NetClient)]
    public class A2NetClient_RequestHandler: ActorMessageHandler<Scene, A2NetClient_Request, A2NetClient_Response>
    {
        protected override async ETTask Run(Scene root, A2NetClient_Request request, A2NetClient_Response response)
        {
            response.MessageObject = await root.GetComponent<SessionComponent>().Session.Call(request.MessageObject);
        }
    }
}