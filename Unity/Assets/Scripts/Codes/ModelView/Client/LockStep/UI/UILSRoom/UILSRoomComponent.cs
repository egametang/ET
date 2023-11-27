using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [ComponentOf(typeof(UI))]
    public class UILSRoomComponent: Entity, IAwake, IUpdate
    {
        public Button saveReplay;
        public InputField saveName;
        public Text totalFrame;
        public Text frameText;
        public Text predictFrameText;
        public InputField jumpToField;
        public Button jump;
        public Button replaySpeed;
        public int frame;
        public int predictFrame;
    }
}