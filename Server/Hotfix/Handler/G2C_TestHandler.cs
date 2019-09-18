using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Benchmark)]
    public class G2C_TestHandler: AMHandler<G2C_Test>
    {
        public static int count = 0;
        protected override async ETTask Run(Session session, G2C_Test message)
        {
            count++;
            await ETTask.CompletedTask;
        }
    }
}