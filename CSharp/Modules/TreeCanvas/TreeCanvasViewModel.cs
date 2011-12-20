using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.ViewModel;
using NLog;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using System.Collections.ObjectModel;
using System.Windows;
using Google.ProtocolBuffers;

namespace TreeCanvas
{
	[Export(typeof(TreeCanvasViewModel))]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	class TreeCanvasViewModel : NotificationObject
	{
	}
}

