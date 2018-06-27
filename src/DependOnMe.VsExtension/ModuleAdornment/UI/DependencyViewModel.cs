using System.Collections.ObjectModel;
using System.Linq;

namespace DependOnMe.VsExtension.ModuleAdornment.UI
{
    public sealed class PlainDependency
    {
        public string Dependency { get; set; }

        public string Implementation { get; set; }
    }

    public sealed class DependencyModule
    {
        public DependencyModule(
            ObservableCollection<PlainDependency> plainDependencies,
            ObservableCollection<DependencyModule> innerModules) 
            => Dependencies  = new ObservableCollection<object>(plainDependencies.Concat(innerModules.Cast<object>()));

        public string ModuleName { get; set; }

        public ObservableCollection<object> Dependencies { get; set; }
    }
}
