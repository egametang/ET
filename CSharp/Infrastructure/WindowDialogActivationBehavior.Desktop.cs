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
namespace Infrastructure
{
    /// <summary>
    /// Specifies the <see cref="DialogActivationBehavior"/> class for using the behavior on WPF.
    /// </summary>
    public class WindowDialogActivationBehavior : DialogActivationBehavior
    {
        /// <summary>
        /// Creates a wrapper for the WPF <see cref="System.Windows.Window"/>.
        /// </summary>
        /// <returns>Instance of the <see cref="System.Windows.Window"/> wrapper.</returns>
        protected override IWindow CreateWindow()
        {
            return new WindowWrapper();
        }
    }
}
