namespace ET.Client
{
    public struct Wait_CreateMyUnit: IWaitType
    {
        public int Error
        {
            get;
            set;
        }

        public M2C_CreateMyUnit Message;
    }
}