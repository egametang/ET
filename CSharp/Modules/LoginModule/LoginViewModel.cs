using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.ViewModel;

namespace Module.Login
{
	[Export(typeof(LoginViewModel))]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	public class LoginViewModel : NotificationObject
	{
		public LoginViewModel()
		{
		}
	}
}
