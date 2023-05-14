using System;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    public static class UILSRoomComponentSystem
    {
        public class AwakeSystem : AwakeSystem<UILSRoomComponent>
        {
            protected override void Awake(UILSRoomComponent self)
            {
                ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
                GameObject replay = rc.Get<GameObject>("Replay");
                GameObject play = rc.Get<GameObject>("Play");
                
                Room room = self.Room();
                if (room.IsReplay)
                {
                    replay.SetActive(true);
                    play.SetActive(false);
                    self.totalFrame = rc.Get<GameObject>("framecount").GetComponent<Text>();
                    self.frameText = rc.Get<GameObject>("progress").GetComponent<Text>();
                    self.jumpToField = rc.Get<GameObject>("jumpToCount").GetComponent<InputField>();
                    self.jump = rc.Get<GameObject>("jump").GetComponent<Button>();
                    self.jump.onClick.AddListener(self.JumpReplay);
                    
                    self.totalFrame.text = self.Room().Replay.FrameInputs.Count.ToString();
                }
                else
                {
                    replay.SetActive(false);
                    play.SetActive(true);
                    self.saveReplay = rc.Get<GameObject>("SaveReplay").GetComponent<Button>();
                    self.saveName = rc.Get<GameObject>("SaveName").GetComponent<InputField>();
                    self.saveReplay.onClick.AddListener(self.OnSaveReplay);
                }
            }
        }

        public class UpdateSystem: UpdateSystem<UILSRoomComponent>
        {
            protected override void Update(UILSRoomComponent self)
            {
                Room room = self.Room();
                if (room.IsReplay)
                {
                    if (self.frame == room.AuthorityFrame)
                    {
                        return;
                    }

                    self.frame = room.AuthorityFrame;
                    self.frameText.text = room.AuthorityFrame.ToString();
                }
            }
        }

        private static void OnSaveReplay(this UILSRoomComponent self)
        {
            string name = self.saveName.text;
            
            LSHelper.SaveReplay(self.Room(), name);
        }
        
        private static void JumpReplay(this UILSRoomComponent self)
        {
            int toFrame = int.Parse(self.jumpToField.text);
            LSHelper.JumpReplay(self.Room(), toFrame);
        }
    }
}