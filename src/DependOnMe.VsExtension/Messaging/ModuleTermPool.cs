using DependOnMe.VsExtension.ModuleAdornment.UI;
using System.Collections.Concurrent;

namespace DependOnMe.VsExtension.Messaging
{
    internal sealed class ModuleTermPool
    {
        private readonly ConcurrentDictionary<string, DependencyModule> _availableModules;

        public ModuleTermPool() => _availableModules = new ConcurrentDictionary<string, DependencyModule>();

        public DependencyModule Request(string moduleName) => _availableModules.GetOrAdd(moduleName, name => new DependencyModule(name));

        public bool Contains(string moduleName) => _availableModules.ContainsKey(moduleName);

        public (bool success, DependencyModule releasedModule) TryRelease(string moduleName)
        {
            if (_availableModules.TryRemove(moduleName, out var module))
            {
                return (true, module);
            }

            return (false, null);
        }

        public void Clean() => _availableModules.Clear();
    }
}
