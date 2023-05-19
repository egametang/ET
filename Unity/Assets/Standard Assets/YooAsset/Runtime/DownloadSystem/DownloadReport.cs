
namespace YooAsset
{
	public struct DownloadReport
	{
		/// <summary>
		/// 下载进度（0f~1f）
		/// </summary>
		public float Progress;

		/// <summary>
		/// 需要下载的总字节数
		/// </summary>
		public ulong TotalSize;

		/// <summary>
		/// 已经下载的字节数
		/// </summary>
		public ulong DownloadedBytes;

		public static DownloadReport CreateDefaultReport()
		{
			DownloadReport report = new DownloadReport();
			report.Progress = 0f;
			report.TotalSize = 0;
			report.DownloadedBytes = 0;
			return report;
		}
	}
}