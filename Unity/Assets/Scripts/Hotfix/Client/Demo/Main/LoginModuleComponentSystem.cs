using System.Threading.Tasks;

namespace ET.Client
{
    [EntitySystemOf(typeof(LoginModuleComponent))]
    [FriendOf(typeof(LoginModuleComponent))]
    public static partial class LoginModuleComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LoginModuleComponent self)
        {

        }
        
        [EntitySystem]
        private static void Destroy(this LoginModuleComponent self)
        {
            
        }
        
        public static async ETTask<long> AddExpAsync(this LoginModuleComponent self, int addExp)
        {
            //给消息中心发送消息，使用NetClient的ActorId
            C2G_AddExp c2GAddExp = C2G_AddExp.Create();
            c2GAddExp.AddExp = addExp;
            //这样调用内部封装了发送给NetClient的消息，不需要像下面再写一遍
            G2C_AddExp g2CAddExp = await self.Root().GetComponent<ClientSenderComponent>().Call(c2GAddExp) as G2C_AddExp;
            return g2CAddExp.Exp;
        }

        public static async ETTask<long> LoginAsync(this LoginModuleComponent self, string account, string password)
        {
            //声明协议，设置数值
            Main2NetClient_Login main2NetClientLogin = Main2NetClient_Login.Create();
            main2NetClientLogin.OwnerFiberId = self.Fiber().Id;
            main2NetClientLogin.Account = account;
            main2NetClientLogin.Password = password;
            main2NetClientLogin.token = "我是token";
            //发送消息，并且等待回信
            var netClientActorId = self.Root().GetComponent<ClientSenderComponent>().GetActorId();
            //给消息中心发送消息，使用NetClient的ActorId
            var response = await self.Root().GetComponent<ProcessInnerSender>().Call(netClientActorId, main2NetClientLogin) as NetClient2Main_Login;
            return response.PlayerId;
        }
    }
}