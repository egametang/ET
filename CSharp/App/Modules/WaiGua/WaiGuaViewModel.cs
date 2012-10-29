using System;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.ViewModel;

namespace Modules.WaiGua
{
	[Export(contractType: typeof (WaiGuaViewModel)), PartCreationPolicy(creationPolicy: CreationPolicy.NonShared)]
	internal class WaiGuaViewModel : NotificationObject
	{
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