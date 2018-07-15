using Compilation;
using CompilationUnit;
using DependOnMe.VsExtension.ModuleAdornment.UI;
using DslAst;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DependOnMe.VsExtension.Messaging
{
    internal sealed class ModuleTermPool
    {
        private readonly ConcurrentDictionary<string, DependencyModule> _availableModules;

        public ModuleTermPool() => _availableModules = new ConcurrentDictionary<string, DependencyModule>();

        public (bool duplicated, DependencyModule module) Request(FileCompilationUnit<Extension.ValidModuleRegistration> moduleUnit)
        {
            if (moduleUnit == null)
            {
                throw new ArgumentNullException(nameof(moduleUnit));
            }

            //RefTable.Instance.AddDeclaration(moduleUnit);
            var moduleName = moduleUnit.CompilationUnit.Name;

            if (RefTable.Instance.HasDuplicates(moduleName))
            {
                _availableModules.TryRemove(moduleName, out _);

                return (true, null);
            }

            var newModule = _availableModules.GetOrAdd(moduleName, name => new DependencyModule(name));

            return (false, newModule);
        }

        public DependencyModule Request(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
            {
                throw new ArgumentNullException(nameof(moduleName));
            }

            if (_availableModules.TryGetValue(moduleName, out var module))
            {
                return module;
            }

            throw new KeyNotFoundException($"Module {moduleName} is not defined");
        }

        public DependencyModule Request(
            FileCompilationUnit<Extension.ValidModuleRegistration> moduleUnit,
            ObservableCollection<PlainDependency> plainDependencies,
            ObservableCollection<DependencyModule> innerModules)
        {
            if (moduleUnit == null)
            {
                throw new ArgumentNullException(nameof(moduleUnit));
            }
            
            //RefTable.Instance.AddDeclaration(moduleUnit);

            var moduleName = moduleUnit.CompilationUnit.Name;

            return _availableModules.GetOrAdd(moduleName, name => new DependencyModule(plainDependencies, innerModules, name));
        }

        public Some<DependencyModule> TryRequest(string moduleName)
        {
            if (RefTable.Instance.HasDuplicates(moduleName))
            {
                _availableModules.TryRemove(moduleName, out _);

                return Some<DependencyModule>.None;
            }

            if (_availableModules.TryGetValue(moduleName, out var module))
            {
                return Some<DependencyModule>.Create(module);
            }

            return Some<DependencyModule>.None;
        }

        public bool Contains(string moduleName) 
            => string.IsNullOrWhiteSpace(moduleName)
                ? throw new ArgumentNullException(nameof(moduleName))
                : _availableModules.ContainsKey(moduleName);

        public Some<DependencyModule> TryRelease(FileCompilationUnit<string> moduleUnit)
        {
            if (moduleUnit == null)
            {
                throw new ArgumentNullException(nameof(moduleUnit));
            }

            RefTable.Instance.TryRemoveDeclaration(moduleUnit);

            if (_availableModules.TryRemove(moduleUnit.CompilationUnit, out var module))
            {
                return Some<DependencyModule>.Create(module);
            }
            
            return Some<DependencyModule>.None;
        }

        public void Clean() => _availableModules.Clear();
    }
}
