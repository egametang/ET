using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class RoleAwakeSystem: AwakeSystem<Role, LoginBasicRoleInfo>
    {
        public override void Awake(Role self, LoginBasicRoleInfo basicinfo)
        {
            self.Awake(basicinfo);
        }
    }

    /// <summary>
    /// 本地角色
    /// </summary>
    public class Role: Entity
    {
        //角色全部信息
        public LoginBasicRoleInfo BasicInfo { get; set; }

        public void Awake(LoginBasicRoleInfo basicinfo)
        {
            this.BasicInfo = basicinfo;
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            this.BasicInfo = null;
        }
    }
}
