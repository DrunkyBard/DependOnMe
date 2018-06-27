using System.Windows;
using System.Windows.Controls;

namespace DependOnMe.VsExtension.ModuleAdornment.UI
{
    public partial class ModuleButton : UserControl
	{
	    private readonly ModuleTree _view;

	    public ModuleButton(double btnHeight, double btnWidth, ModuleTree view)
	    {
	        _view = view;
	        InitializeComponent();
	        RectBtn.Height = btnHeight;
	        RectBtn.Width  = btnWidth;
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            _view.Visibility = Visibility.Visible;
        }

        private void ToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _view.Visibility = Visibility.Collapsed;
        }
    }
}
