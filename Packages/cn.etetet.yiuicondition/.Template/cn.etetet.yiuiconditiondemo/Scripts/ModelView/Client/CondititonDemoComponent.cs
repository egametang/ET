using System;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Desc    条件测试
    /// </summary>
    [ComponentOf]
    public class CondititonDemoComponent : Entity, IAwake, IDestroy
    {
        public int  DemoValue { get; set; }
        public long m_TimerId;
    }
}
