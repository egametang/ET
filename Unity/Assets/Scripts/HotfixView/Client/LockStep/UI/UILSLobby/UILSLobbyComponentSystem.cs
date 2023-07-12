using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UILSLobbyComponent))]
    [FriendOf(typeof(UILSLobbyComponent))]
    public static partial class UILSLobbyComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UILSLobbyComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();

            self.enterMap = rc.Get<GameObject>("EnterMap");
            self.enterMap.GetComponent<Button>().onClick.AddListener(() =>
            {
                self.EnterMap().Coroutine();
            });
            
            self.replay = rc.Get<GameObject>("Replay").GetComponent<Button>();
            self.replayPath = rc.Get<GameObject>("ReplayPath").GetComponent<InputField>();
            self.replay.onClick.AddListener(self.Replay);
        }

        private static async ETTask EnterMap(this UILSLobbyComponent self)
        {
            await EnterMapHelper.Match(self.Fiber());
        }
        
        private static void Replay(this UILSLobbyComponent self)
        {
            byte[] bytes = File.ReadAllBytes(self.replayPath.text);
            
            Replay replay = MemoryPackHelper.Deserialize(typeof (Replay), bytes, 0, bytes.Length) as Replay;
            Log.Debug($"start replay: {replay.Snapshots.Count} {replay.FrameInputs.Count} {replay.UnitInfos.Count}");
            LSSceneChangeHelper.SceneChangeToReplay(self.Root(), replay).Coroutine();
        }
    }
}