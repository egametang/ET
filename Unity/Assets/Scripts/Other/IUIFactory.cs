namespace Model
{
	public interface IUIFactory
	{
		UI Create(Scene scene, UIType type, UI parent);
	}
}