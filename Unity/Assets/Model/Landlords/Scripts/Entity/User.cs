using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETModel
{

    /// <summary>
    /// 通过ETModel.ComponentFactory.CreateWithId<User, long>(g2C_LoginGate_Ack.PlayerID, g2C_LoginGate_Ack.UserID); 
    /// 这种方式创建
    /// </summary>
    [ObjectSystem]
    public class UserAwakeSystem : AwakeSystem<User, long>
    {
        public override void Awake(User self, long id)
        {
            self.Awake(id);
        }
    }

    /// <summary>
    /// 玩家对象
    /// </summary>
    public sealed class User : Entity
    {
        //用户ID（唯一）
        public long UserID { get; private set; }

        public void Awake(long id)
        {
            this.UserID = id;
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            this.UserID = 0;
        }
    }
}
