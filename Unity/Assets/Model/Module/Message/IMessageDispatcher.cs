using System.IO;

namespace ET
{
    public interface IMessageDispatcher
    {
        void Dispatch(Session session, MemoryStream message);
    }
}