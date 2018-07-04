using System.Windows;
using System.Windows.Input;

namespace DependOnMe.VsExtension.ModuleAdornment.UI
{
    public partial class ModuleTree
	{
		public ModuleTree(DependencyModule viewModel)
		{
		    InitializeComponent();
		    DataContext = viewModel;
        }

        private bool  _mouseDownInToolbar;
	    private Point _dragOffset;
	    private bool  _drag;

        private void HeaderMove(object sender, MouseEventArgs e)
        {
            if (_mouseDownInToolbar)
            {
                MoveUserControl(e);
            }
        }

        private void MoveUserControl(MouseEventArgs e)
        {
            var mousePos = e.GetPosition(FarmsTree);
            var newX = LocalTranslateTransform.X + (mousePos.X - _dragOffset.X);
            var newY = LocalTranslateTransform.Y + (mousePos.Y - _dragOffset.Y);
            LocalTranslateTransform.X = newX;
            LocalTranslateTransform.Y = newY;
        }

        private void HeaderClickUp(object sender, MouseButtonEventArgs e)
        {
            if (_drag)
            {
                _mouseDownInToolbar = false;
                ModuleHeader.ReleaseMouseCapture();
                _drag = !_drag;
            }
            else
            {
                _mouseDownInToolbar = true;
                _dragOffset = e.GetPosition(FarmsTree);
                ModuleHeader.CaptureMouse();
                _drag = !_drag;
            }
        }
    }
}
