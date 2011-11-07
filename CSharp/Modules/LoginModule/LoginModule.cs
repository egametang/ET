using System;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;

namespace Module.Login
{
	[ModuleExport(typeof(LoginModule))]
	public class LoginModule : IModule
	{
		public void Initialize()
		{
		}
	}
}
