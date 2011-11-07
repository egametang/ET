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
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;

namespace Infrastructure
{
    /// <summary>
    /// Defines a behavior that executes a <see cref="ICommand"/> when the Return key is pressed inside a <see cref="TextBox"/>.
    /// </summary>
    /// <remarks>This behavior also supports setting a basic watermark on the <see cref="TextBox"/>.</remarks>
    public class ReturnCommandBehavior : CommandBehaviorBase<TextBox>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ReturnCommandBehavior"/>.
        /// </summary>
        /// <param name="textBox">The <see cref="TextBox"/> over which the <see cref="ICommand"/> will work.</param>
        public ReturnCommandBehavior(TextBox textBox)
            : base(textBox)
        {
            textBox.AcceptsReturn = false;
            textBox.KeyDown += (s, e) => this.KeyPressed(e.Key);
            textBox.GotFocus += (s, e) => this.GotFocus();
            textBox.LostFocus += (s, e) => this.LostFocus();
        }

        /// <summary>
        /// Gets or Sets the text which is set as water mark on the <see cref="TextBox"/>.
        /// </summary>
        public string DefaultTextAfterCommandExecution { get; set; }

        /// <summary>
        /// Executes the <see cref="ICommand"/> when <paramref name="key"/> is <see cref="Key.Enter"/>.
        /// </summary>
        /// <param name="key">The key pressed on the <see cref="TextBox"/>.</param>
        protected void KeyPressed(Key key)
        {
            if (key == Key.Enter && TargetObject != null)
            {
                this.CommandParameter = TargetObject.Text;
                ExecuteCommand();

                this.ResetText();
            }
        }

        private void GotFocus()
        {
            if (TargetObject != null && TargetObject.Text == this.DefaultTextAfterCommandExecution)
            {
                this.ResetText();
            }
        }

        private void ResetText()
        {
            TargetObject.Text = string.Empty;
        }

        private void LostFocus()
        {
            if (TargetObject != null && string.IsNullOrEmpty(TargetObject.Text) && this.DefaultTextAfterCommandExecution != null)
            {
                TargetObject.Text = this.DefaultTextAfterCommandExecution;
            }
        }
    }
}
