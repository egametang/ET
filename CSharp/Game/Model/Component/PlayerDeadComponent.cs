using System;

namespace Model
{
    public class PlayerDeadComponent: DeadComponent
    {
        public override void Dead()
        {
            Console.WriteLine("Player {0} Dead!", this.Owner.Id);
        }
    }
}
