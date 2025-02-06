using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(OperaComponent))]
    public static partial class OperaComponentSystem
    {
        [EntitySystem]
        private static void Awake(this OperaComponent self)
        {
            self.mapMask = LayerMask.GetMask("Map");
        }

        [EntitySystem]
        private static void Update(this OperaComponent self)
        {
            if (Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 1000, self.mapMask))
                {
                    C2M_PathfindingResult c2MPathfindingResult = C2M_PathfindingResult.Create();
                    c2MPathfindingResult.Position = hit.point;
                    self.Root().GetComponent<ClientSenderComponent>().Send(c2MPathfindingResult);
                }
            }
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                CodeLoader.Instance.Reload();
            }
            
            if (Input.GetKeyDown(KeyCode.Q))
            {
                self.Test1().NoContext();
            }
                
            if (Input.GetKeyDown(KeyCode.W))
            {
                self.Test2().NoContext();
            }
            
            if (Input.GetKeyDown(KeyCode.A))
            {
                self.TestCancelAfter().WithContext(new ETCancellationToken());
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                C2M_TransferMap c2MTransferMap = C2M_TransferMap.Create();
                self.Root().GetComponent<ClientSenderComponent>().Call(c2MTransferMap).NoContext();
            }
        }
        
        private static async ETTask Test1(this OperaComponent self)
        {
            Log.Debug($"Croutine 1 start1 ");
            using (await self.Root().GetComponent<CoroutineLockComponent>().Wait(1, 20000, 3000))
            {
                await self.Root().GetComponent<TimerComponent>().WaitAsync(6000);
            }

            Log.Debug($"Croutine 1 end1");
        }
            
        private static async ETTask Test2(this OperaComponent self)
        {
            ETCancellationToken oldCancellationToken = await ETTaskHelper.GetContextAsync<ETCancellationToken>();
            Log.Debug($"Croutine 2 start2");
            using (await self.Root().GetComponent<CoroutineLockComponent>().Wait(1, 20000, 3000))
            {
                await self.Root().GetComponent<TimerComponent>().WaitAsync(1000);
            }
            Log.Debug($"Croutine 2 end2");
        }
        
        private static async ETTask TestCancelAfter(this OperaComponent self)
        {
            ETCancellationToken oldCancellationToken = await ETTaskHelper.GetContextAsync<ETCancellationToken>();
            
            Log.Debug($"TestCancelAfter start");
            ETCancellationToken newCancellationToken = new();
            await self.Root().GetComponent<TimerComponent>().WaitAsync(3000).TimeoutAsync(newCancellationToken, 1000);
            if (newCancellationToken.IsCancel())
            {
                Log.Debug($"TestCancelAfter newCancellationToken is cancel!");
            }
            
            if (oldCancellationToken != null && !oldCancellationToken.IsCancel())
            {
                Log.Debug($"TestCancelAfter oldCancellationToken is not cancel!");
            }
            Log.Debug($"TestCancelAfter end");
        }
    }
}