namespace Model
{
	public interface IUIFactory
	{
		UI Create(Scene scene, int type, UI parent);
	}
}