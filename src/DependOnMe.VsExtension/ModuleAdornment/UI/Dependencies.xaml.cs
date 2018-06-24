using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DependOnMe.VsExtension.ModuleAdornment.UI
{
    public partial class Dependencies : UserControl
    {

        public Dependencies()
        {
            DataContext = new DependenciesViewModel
            {
                Dependencies = new ObservableCollection<DependencyViewModel>(
                    new List<DependencyViewModel>
                    {
                        new DependencyViewModel{ Dependency = "A", Implementation = "B"},
                        new DependencyViewModel{ Dependency = "D", Implementation = "D"},
                        new DependencyViewModel{ Dependency = "D", Implementation = "D"},
                        new DependencyViewModel{ Dependency = "D", Implementation = "D"},
                        new DependencyViewModel{ Dependency = "D", Implementation = "D"},
                        new DependencyViewModel{ Dependency = "D", Implementation = "D"},
                        new DependencyViewModel{ Dependency = "D", Implementation = "D"},
                        new DependencyViewModel{ Dependency = "D", Implementation = "D"},
                        new DependencyViewModel{ Dependency = "D", Implementation = "D"},
                        new DependencyViewModel{ Dependency = "D", Implementation = "D"},
                        new DependencyViewModel{ Dependency = "D", Implementation = "D"},
                    })
            };
            InitializeComponent();
        }

        private bool _mouseDownInToolbar;
        private Point _dragOffset;
        private bool _drag;

        private void HeaderClickDown(object sender, MouseButtonEventArgs e)
        {
            //_view.VisualElement.Cursor = null;
            //_mouseDownInToolbar = true;
            //_dragOffset = e.GetPosition(DgDownloadsInfo);
            //ModuleHeader.CaptureMouse();
            
        }

        private void HeaderMove(object sender, MouseEventArgs e)
        {
            if (_mouseDownInToolbar)
            {
                moveUserControl(e);
            }
        }

        private void moveUserControl(MouseEventArgs e)
        {
            var mousePos = e.GetPosition(DgDownloadsInfo);
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
                _mouseDownInToolbar        = true;
                _dragOffset                = e.GetPosition(DgDownloadsInfo);
                ModuleHeader.CaptureMouse();
                _drag = !_drag;
            }
        }
    }
}
