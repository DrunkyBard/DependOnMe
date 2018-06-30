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
            ObservableCollection<DependencyModule> innerModules,
            string moduleName)
        {
            Dependencies = new ObservableCollection<object>(plainDependencies.Concat(innerModules.Cast<object>()));
            ModuleName = moduleName;
        }

        public DependencyModule(string moduleName) 
            : this(new ObservableCollection<PlainDependency>(), new ObservableCollection<DependencyModule>(), moduleName)
        { }

        public string ModuleName { get; }

        public ObservableCollection<object> Dependencies { get; }
    }
}
