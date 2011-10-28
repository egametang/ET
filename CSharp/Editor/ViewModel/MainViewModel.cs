using GalaSoft.MvvmLight;
using Editor.Model;

namespace Editor.ViewModel
{
	/// <summary>
	/// This class contains properties that the main View can data bind to.
	/// <para>
	/// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
	/// </para>
	/// <para>
	/// See http://www.galasoft.ch/mvvm/getstarted
	/// </para>
	/// </summary>
	public class MainViewModel : ViewModelBase
	{
		private readonly IDataService dataService;

		/// <summary>
		/// The <see cref="WelcomeTitle" /> property's name.
		/// </summary>
		public const string WelcomeTitlePropertyName = "WelcomeTitle";

		private string welcomeTitle = string.Empty;

		/// <summary>
		/// Gets the WelcomeTitle property.
		/// Changes to that property's value raise the PropertyChanged event. 
		/// </summary>
		public string WelcomeTitle
		{
			get
			{
				return welcomeTitle;
			}

			set
			{
				if (welcomeTitle == value)
				{
					return;
				}

				welcomeTitle = value;
				RaisePropertyChanged(WelcomeTitlePropertyName);
			}
		}

		/// <summary>
		/// Initializes a new instance of the MainViewModel class.
		/// </summary>
		public MainViewModel(IDataService dataService)
		{
			this.dataService = dataService;
			this.dataService.GetData(
				(item, error) =>
				{
					if (error != null)
					{
						// Report error here
						return;
					}

					WelcomeTitle = item.Title;
				});
		}

		////public override void Cleanup()
		////{
		////    // Clean up if needed

		////    base.Cleanup();
		////}
	}
}