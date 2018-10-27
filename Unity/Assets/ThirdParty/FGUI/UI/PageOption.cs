
namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class PageOption
	{
		Controller _controller;
		string _id;

		public PageOption()
		{
		}

		public Controller controller
		{
			set
			{
				_controller = value;
			}
		}

		public int index
		{
			set
			{
				_id = _controller.GetPageId(value);
			}
			get
			{
				if (_id != null)
					return _controller.GetPageIndexById(_id);
				else
					return -1;
			}
		}

		public string name
		{
			set
			{
				_id = _controller.GetPageIdByName(value);
			}
			get
			{
				if (_id != null)
					return _controller.GetPageNameById(_id);
				else
					return null;
			}
		}

		public void Clear()
		{
			_id = null;
		}

		public string id
		{

			set
			{
				_id = value;
			}
			get
			{
				return _id;
			}
		}

	}
}
