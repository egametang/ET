using System;
using Model;
using UnityEngine;
using UnityEngine.UI;

namespace Hotfix
{
	[ObjectEvent]
	public class UILobbyComponentEvent : ObjectEvent<UILobbyComponent>, IAwake
	{
		public void Awake()
		{
			this.Get().Awake();
		}
	}
	
	public class UILobbyComponent : Component
	{
		private GameObject sendBtn;
		private GameObject sendRpcBtn;
		private GameObject transfer1Btn;
		private GameObject transfer2Btn;

		public void Awake()
		{
			ReferenceCollector rc = this.GetEntity<UI>().GameObject.GetComponent<ReferenceCollector>();
			this.sendBtn = rc.Get<GameObject>("Send");
			this.sendRpcBtn = rc.Get<GameObject>("SendRpc");
			this.sendBtn.GetComponent<Button>().onClick.Add(this.OnSend);
			this.sendRpcBtn.GetComponent<Button>().onClick.Add(this.OnSendRpc);

			this.transfer1Btn = rc.Get<GameObject>("Transfer1");
			this.transfer2Btn = rc.Get<GameObject>("Transfer2");
			this.transfer1Btn.GetComponent<Button>().onClick.Add(this.OnTransfer1);
			this.transfer2Btn.GetComponent<Button>().onClick.Add(this.OnTransfer2);
		}

		private void OnSend()
		{
			// 发送一个actor消息
			SessionComponent.Instance.Session.Send(new Actor_Test() { Info = "message client->gate->map->gate->client" });
		}

		private async void OnSendRpc()
		{
			try
			{
				// 向actor发起一次rpc调用
				Actor_TestResponse response = await SessionComponent.Instance.Session.Call<Actor_TestResponse>(new Actor_TestRequest() { request = "request actor test rpc" });
				Log.Info($"recv response: {MongoHelper.ToJson(response)}");
			}
			catch (Exception e)
			{
				Log.Error(e.ToStr());
			}
		}

		private async void OnTransfer1()
		{
			try
			{
				Actor_TransferResponse response = await SessionComponent.Instance.Session.Call<Actor_TransferResponse>(new Actor_TransferRequest() {MapIndex = 0});
				Log.Info($"传送成功! {MongoHelper.ToJson(response)}");
			}
			catch (Exception e)
			{
				Log.Error(e.ToStr());
			}
		}

		private async void OnTransfer2()
		{
			Actor_TransferResponse response = await SessionComponent.Instance.Session.Call<Actor_TransferResponse>(new Actor_TransferRequest() { MapIndex = 1 });
			Log.Info($"传送成功! {MongoHelper.ToJson(response)}");
		}
	}
}
