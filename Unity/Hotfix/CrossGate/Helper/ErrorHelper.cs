using ETModel;

namespace ETHotfix
{
    public static class ErrorHelper
    {
        public static void ShowErrorMessage(int error)
        {
            switch (error)
            {
                case ErrorCode.ERR_SignError:
                    GameTool.ShowPopMessage("网络连接信息已失效!");
                    break;
                case ErrorCode.ERR_AccountAlreadyRegister:
                    GameTool.ShowPopMessage("注册失败, 帐号已被注册!");
                    break;
                case ErrorCode.ERR_ConnectGateKeyError:
                    GameTool.ShowPopMessage("连接失败, 连接网关超时!");
                    break;
                case ErrorCode.ERR_CharacterNameAlreadyExist:
                    GameTool.ShowPopMessage("创建失败, 该名字已被其他玩家注册!");
                    break;
                default:
                    GameTool.ShowPopMessage("遇到了未处理的异常");
                    Log.Debug("遇到未注册的ErrorCode: " + error);
                    break;
            }
        }
    }
}
