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

        private void OnToolbarClicked(object sender, MouseButtonEventArgs e)
        {
            _mouseDownInToolbar = true;
            _dragOffset         = e.GetPosition(DgDownloadsInfo);
            var q = DgDownloadsInfo.CaptureMouse();
        }

        private void OnToolbarMoving(object sender, MouseEventArgs e)
        {
            if (_mouseDownInToolbar)
            {
                MoveUserControl(e);
            }
        }

        private void MoveUserControl(MouseEventArgs e)
        {
            var mousePos = e.GetPosition(DgDownloadsInfo);
            var newX     = LocalTranslateTransform.X + (mousePos.X - _dragOffset.X);
            var newY     = LocalTranslateTransform.Y + (mousePos.Y - _dragOffset.Y);
            LocalTranslateTransform.X = newX;
            LocalTranslateTransform.Y = newY;
        }

        private void OnToolbarReleased(object sender, MouseButtonEventArgs e)
        {
            _mouseDownInToolbar = false;
            DgDownloadsInfo.ReleaseMouseCapture();
        }
    }
}
