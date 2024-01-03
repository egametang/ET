using System;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UILSRoomComponent))]
    [FriendOf(typeof (UILSRoomComponent))]
    public static partial class UILSRoomComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UILSRoomComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            GameObject replay = rc.Get<GameObject>("Replay");
            GameObject play = rc.Get<GameObject>("Play");

            self.frameText = rc.Get<GameObject>("progress").GetComponent<Text>();

            Room room = self.Room();
            if (room.IsReplay)
            {
                replay.SetActive(true);
                play.SetActive(false);
                self.totalFrame = rc.Get<GameObject>("framecount").GetComponent<Text>();
                self.jumpToField = rc.Get<GameObject>("jumpToCount").GetComponent<InputField>();
                self.jump = rc.Get<GameObject>("jump").GetComponent<Button>();
                self.jump.onClick.AddListener(self.JumpReplay);
                self.replaySpeed = rc.Get<GameObject>("speed").GetComponent<Button>();
                self.replaySpeed.onClick.AddListener(self.OnReplaySpeedClicked);

                self.totalFrame.text = self.Room().Replay.FrameInputs.Count.ToString();
            }
            else
            {
                replay.SetActive(false);
                play.SetActive(true);
                self.predictFrameText = rc.Get<GameObject>("predict").GetComponent<Text>();
                self.saveReplay = rc.Get<GameObject>("SaveReplay").GetComponent<Button>();
                self.saveName = rc.Get<GameObject>("SaveName").GetComponent<InputField>();
                self.saveReplay.onClick.AddListener(self.OnSaveReplay);
            }
        }

        [EntitySystem]
        private static void Update(this UILSRoomComponent self)
        {
            Room room = self.Room();
            if (self.frame != room.AuthorityFrame)
            {
                self.frame = room.AuthorityFrame;
                self.frameText.text = room.AuthorityFrame.ToString();
            }

            if (!room.IsReplay)
            {
                if (self.predictFrame != room.PredictionFrame)
                {
                    self.predictFrame = room.PredictionFrame;
                    self.predictFrameText.text = room.PredictionFrame.ToString();
                }
            }
        }

        private static void OnSaveReplay(this UILSRoomComponent self)
        {
            string name = self.saveName.text;

            LSClientHelper.SaveReplay(self.Room(), name);
        }

        private static void JumpReplay(this UILSRoomComponent self)
        {
            int toFrame = int.Parse(self.jumpToField.text);
            LSClientHelper.JumpReplay(self.Room(), toFrame);
        }

        private static void OnReplaySpeedClicked(this UILSRoomComponent self)
        {
            LSReplayUpdater lsReplayUpdater = self.Room().GetComponent<LSReplayUpdater>();
            lsReplayUpdater.ChangeReplaySpeed();
            self.replaySpeed.gameObject.Get<GameObject>("Text").GetComponent<Text>().text = $"X{lsReplayUpdater.ReplaySpeed}";
        }
    }
}