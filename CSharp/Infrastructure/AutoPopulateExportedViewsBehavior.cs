//===================================================================================
// Microsoft patterns & practices
// Composite Application Guidance for Windows Presentation Foundation and Silverlight
//===================================================================================
// Copyright (c) Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===================================================================================
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
//===================================================================================
using System;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Regions;

namespace Infrastructure
{
	[Export(typeof(AutoPopulateExportedViewsBehavior))]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	public class AutoPopulateExportedViewsBehavior : RegionBehavior, IPartImportsSatisfiedNotification
	{
		protected override void OnAttach()
		{
			AddRegisteredViews();
		}

		public void OnImportsSatisfied()
		{
			AddRegisteredViews();
		}

		private void AddRegisteredViews()
		{
			if (this.Region == null)
			{
				return;
			}
			
			foreach (var viewEntry in this.RegisteredViews)
			{
				if (viewEntry.Metadata.RegionName == this.Region.Name)
				{
					var view = viewEntry.Value;

					if (!this.Region.Views.Contains(view))
					{
						this.Region.Add(view);
					}
				}
			}
		}

		[ImportMany(AllowRecomposition = true)]
		public Lazy<object, IViewRegionRegistration>[] RegisteredViews { get; set; }
	}
}
