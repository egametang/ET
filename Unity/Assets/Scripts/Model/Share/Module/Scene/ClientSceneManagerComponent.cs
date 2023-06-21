using System;

namespace ET
{
    [ComponentOf(typeof(VProcess))]
    public class ClientSceneManagerComponent: SingletonEntity<ClientSceneManagerComponent>, IAwake
    {
    }
}