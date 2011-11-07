using System;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;

namespace Module.Login
{
	/// <summary>
	/// A module for the quickstart.
	/// </summary>
	[ModuleExport(typeof(LoginModule))]
	public class LoginModule : IModule
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ModuleA"/> class.
		/// </summary>
		/// <param name="logger">The logger.</param>
		/// <param name="moduleTracker">The module tracker.</param>
		[ImportingConstructor]
		public LoginModule()
		{
		}

		/// <summary>
		/// Notifies the module that it has be initialized.
		/// </summary>
		public void Initialize()
		{
		}
	}
}
