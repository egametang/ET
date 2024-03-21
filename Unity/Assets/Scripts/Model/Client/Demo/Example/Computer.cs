using System.Collections;
using System.Collections.Generic;

namespace ET.Client
{
    //该实体类属于指定的实体类，添加typeof指定，任意父实体就不需要。
    [ChildOf(typeof(ComputersComponent))]
    //无论是实体还是组件，都必须继承Entity
    //而IAwake IUpdate IDestroy是生命周期函数，按需添加即可
    public class Computer : Entity, IAwake, IUpdate, IDestroy
    {
        
    }
}