﻿using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace DependOnMe.VsExtension.ModuleAdornment.UI
{
    public sealed class PlainDependency
    {
        public string Dependency { get; set; }

        public string Implementation { get; set; }

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
    }
}
