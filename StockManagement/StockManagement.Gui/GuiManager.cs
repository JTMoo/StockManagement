using StockManagement.Gui.ViewModel;

namespace StockManagement.Gui;


internal class GuiManager
{
	internal static readonly GuiManager Instance = new GuiManager();

	internal MainViewModel MainViewModel { get; set; }
}
