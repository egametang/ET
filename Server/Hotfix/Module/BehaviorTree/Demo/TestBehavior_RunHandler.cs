using ETModel;

namespace ETHotfix
{
    // 测试行为树Demo
    [Event(EventIdType.TestBehavior)]
    public class TestBehavior_RunHandler : AEvent<string>
    {
        public override void Run(string name)
        {
            BehaviorTreeFactory.Create(name);
        }
    }
}
