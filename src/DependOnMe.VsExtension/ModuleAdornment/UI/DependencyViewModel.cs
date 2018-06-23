using System.Collections.ObjectModel;

namespace DependOnMe.VsExtension.ModuleAdornment.UI
{
    public sealed class DependencyViewModel
    {
        public string Dependency { get; set; }

        public string Implementation { get; set; }
    }

    public sealed class DependenciesViewModel
    {
        public ObservableCollection<DependencyViewModel> Dependencies { get; set; }
    }
}
