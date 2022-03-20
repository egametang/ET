using System;

namespace ET
{
    public class RobotCaseAttribute: BaseAttribute
    {
        public int CaseType { get; }

        public RobotCaseAttribute(int caseType)
        {
            this.CaseType = caseType;
        }
    }
}