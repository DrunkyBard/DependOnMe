using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DependOnMe.VsExtension.ModuleAdornment.UI
{
    public sealed class PlainDependency : IEquatable<PlainDependency>, INotifyPropertyChanged
    {
        private string _dependency;
        public string Dependency {
            get { return _dependency; }
            set
            {
                _dependency = value;
                OnPropertyChanged();
            }
        }

        private string _impl;
        public string Implementation
        {
            get { return _impl; }
            set
            {
                _impl = value;
                OnPropertyChanged();
            }
        }

        public PlainDependency(string dependency, string implementation)
        {
            if (string.IsNullOrWhiteSpace(dependency))
            {
                throw new ArgumentNullException(nameof(dependency));
            }

            if (string.IsNullOrWhiteSpace(implementation))
            {
                throw new ArgumentNullException(nameof(implementation));
            }

            Dependency = dependency;
            Implementation = implementation;
        }

        public bool Equals(PlainDependency other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return string.Equals(Dependency, other.Dependency, StringComparison.OrdinalIgnoreCase) && 
                   string.Equals(Implementation, other.Implementation, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj is PlainDependency && Equals((PlainDependency) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Dependency.GetHashCode() * 397;
                hashCode ^= Implementation.GetHashCode();

                return hashCode;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed class DependencyModule
    {
        public DependencyModule(
            ObservableCollection<PlainDependency> plainDependencies,
            ObservableCollection<DependencyModule> innerModules,
            string moduleName)
        {
            if (plainDependencies == null)
            {
                throw new ArgumentNullException(nameof(plainDependencies));
            }

            if (innerModules == null)
            {
                throw new ArgumentNullException(nameof(innerModules));
            }

            if (string.IsNullOrWhiteSpace(moduleName))
            {
                throw new ArgumentNullException(nameof(moduleName));
            }

            Dependencies = new ObservableCollection<object>(plainDependencies.Concat(innerModules.Cast<object>()));
            ModuleName = moduleName;
        }

        public DependencyModule(string moduleName) 
            : this(new ObservableCollection<PlainDependency>(), new ObservableCollection<DependencyModule>(), moduleName)
        { }

        public string ModuleName { get; }

        public ObservableCollection<object> Dependencies { get; }

        private static void CheckIsNull<T>(T value) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
        }

        public DependencyModule Add(PlainDependency dependency)
        {
            CheckIsNull(dependency);
            Dependencies.Add(dependency);

            return this;
        }

        public DependencyModule Add(DependencyModule module)
        {
            CheckIsNull(module);
            Dependencies.Add(module);

            return this;
        }

        public DependencyModule Remove(PlainDependency dependency)
        {
            CheckIsNull(dependency);
            Dependencies.Remove(dependency);

            return this;
        }

        public DependencyModule Remove(DependencyModule module)
        {
            CheckIsNull(module);
            Dependencies.Remove(module);

            return this;
        }
    }
}
