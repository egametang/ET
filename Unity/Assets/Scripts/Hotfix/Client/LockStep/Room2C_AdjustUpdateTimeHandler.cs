namespace ET.Client
{
    [MessageHandler(SceneType.Client)]
    public class Room2C_AdjustUpdateTimeHandler: AMHandler<Room2C_AdjustUpdateTime>
    {
        protected override async ETTask Run(Session session, Room2C_AdjustUpdateTime message)
        {
            Room room = session.DomainScene().GetComponent<Room>();
            int newInterval = (1000 + (message.DiffTime - LSConstValue.UpdateInterval * 2)) * LSConstValue.UpdateInterval / 1000;

            if (newInterval < 40)
            {
                newInterval = 40;
            }

            if (newInterval > 66)
            {
                newInterval = 66;
            }
            
            room.FixedTimeCounter.ChangeInterval(newInterval, room.FrameBuffer.PredictionFrame - 1);
            await ETTask.CompletedTask;
        }
    }
}