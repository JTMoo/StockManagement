namespace StockManagement.Kernel.Model;

public class SparePart : StockItem
{
	internal override void Register()
	{
		MainManager.Instance.SparePartManager.Register(this);
	}
}
