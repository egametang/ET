
public static class PatchEventDispatcher
{
	public static void SendPatchStepsChangeMsg(EPatchStates currentStates)
	{
		PatchEventMessageDefine.PatchStatesChange msg = new PatchEventMessageDefine.PatchStatesChange();
		msg.CurrentStates = currentStates;
		EventManager.SendMessage(msg);
	}
	public static void SendFoundUpdateFilesMsg(int totalCount, long totalSizeBytes)
	{
		PatchEventMessageDefine.FoundUpdateFiles msg = new PatchEventMessageDefine.FoundUpdateFiles();
		msg.TotalCount = totalCount;
		msg.TotalSizeBytes = totalSizeBytes;
		EventManager.SendMessage(msg);
	}
	public static void SendDownloadProgressUpdateMsg(int totalDownloadCount, int currentDownloadCount, long totalDownloadSizeBytes, long currentDownloadSizeBytes)
	{
		PatchEventMessageDefine.DownloadProgressUpdate msg = new PatchEventMessageDefine.DownloadProgressUpdate();
		msg.TotalDownloadCount = totalDownloadCount;
		msg.CurrentDownloadCount = currentDownloadCount;
		msg.TotalDownloadSizeBytes = totalDownloadSizeBytes;
		msg.CurrentDownloadSizeBytes = currentDownloadSizeBytes;
		EventManager.SendMessage(msg);
	}
	public static void SendStaticVersionUpdateFailedMsg()
	{
		PatchEventMessageDefine.StaticVersionUpdateFailed msg = new PatchEventMessageDefine.StaticVersionUpdateFailed();
		EventManager.SendMessage(msg);
	}
	public static void SendPatchManifestUpdateFailedMsg()
	{
		PatchEventMessageDefine.PatchManifestUpdateFailed msg = new PatchEventMessageDefine.PatchManifestUpdateFailed();
		EventManager.SendMessage(msg);
	}
	public static void SendWebFileDownloadFailedMsg(string fileName, string error)
	{
		PatchEventMessageDefine.WebFileDownloadFailed msg = new PatchEventMessageDefine.WebFileDownloadFailed();
		msg.FileName = fileName;
		msg.Error = error;
		EventManager.SendMessage(msg);
	}
}