using System;
using System.Globalization;
using System.Windows.Controls;

namespace Modules.Robot
{
	public class NumValidation : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			if ((string) value == "")
			{
				return new ValidationResult(true, null);
			}
			try
			{
				int.Parse((string) value);
			}
			catch (Exception e)
			{
				return new ValidationResult(false, string.Format("Illegal characters or {0}", e.Message));
			}

			return new ValidationResult(true, null);
		}
	}
}
