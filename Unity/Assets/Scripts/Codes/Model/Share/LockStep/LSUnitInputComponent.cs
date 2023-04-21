namespace ET
{
    public class LSUnitInputComponent: LSEntity, ILSUpdate, IAwake
    {
        public LSInputInfo LSInputInfo { get; set; } = new LSInputInfo();

    }
}