using DependOnMe.VsExtension.ModuleAdornment.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace DependOnMe.VsExtension.Messaging
{
    internal sealed class ModuleTermPool
    {
        private readonly ConcurrentDictionary<string, DependencyModule> _availableModules;

        public ModuleTermPool() => _availableModules = new ConcurrentDictionary<string, DependencyModule>();

        public DependencyModule Request(string moduleName) 
            => string.IsNullOrWhiteSpace(moduleName) 
                ? throw new ArgumentNullException(nameof(moduleName))
                : _availableModules.GetOrAdd(moduleName, name => new DependencyModule(name));

        public DependencyModule Request(
            string moduleName,
            ObservableCollection<PlainDependency> plainDependencies,
            ObservableCollection<DependencyModule> innerModules) 
            => string.IsNullOrWhiteSpace(moduleName) 
                ? throw new ArgumentNullException(nameof(moduleName))
                : _availableModules.GetOrAdd(moduleName, name => new DependencyModule(plainDependencies, innerModules, moduleName));

        public Some<DependencyModule> TryRequest(string moduleName)
        {
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

        public (bool success, DependencyModule releasedModule) TryRelease(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
            {
                throw new ArgumentNullException(nameof(moduleName));
            }

            if (_availableModules.TryRemove(moduleName, out var module))
            {
                return (true, module);
            }
            
            return (false, null);
        }

        public void Clean() => _availableModules.Clear();
    }
}
