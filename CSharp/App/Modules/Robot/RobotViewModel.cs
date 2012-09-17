using System;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.ViewModel;
using NLog;

namespace Modules.Robot
{
	[Export(contractType: typeof (RobotViewModel)), PartCreationPolicy(creationPolicy: CreationPolicy.NonShared)]
	internal class RobotViewModel : NotificationObject
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private string logText = "";

		public string LogText
		{
			get
			{
				return this.logText;
			}
			set
			{
				if (this.logText == value)
				{
					return;
				}
				this.logText = value;
				this.RaisePropertyChanged("LogText");
			}
		}

		public void Start()
		{
			this.LogText += "11111111111" + Environment.NewLine;
		}
	}
}