using DependOnMe.VsExtension.ModuleAdornment.UI;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DependOnMe.VsExtension.Messaging
{
    internal struct ModuleCreated
    {
        public readonly DependencyModule CreatedModule;

        public ModuleCreated(DependencyModule createdModule)
        {
            CreatedModule = createdModule;
        }
    }

    internal struct ModuleRemoved
    {
        public readonly DependencyModule RemovedModule;

        public ModuleRemoved(DependencyModule removedModule)
        {
            RemovedModule = removedModule;
        }
    }

    internal sealed class ModuleHub
    {
        public readonly ModuleTermPool ModulePool;

        public IObservable<ModuleCreated> NewModulesStream     => _createSubject.AsObservable();

        public IObservable<ModuleRemoved> RemovedModulesStream => _removeSubject.AsObservable();

        private readonly Subject<ModuleCreated> _createSubject;

        private readonly Subject<ModuleRemoved> _removeSubject;

        private static readonly Lazy<ModuleHub> HubInstance = new Lazy<ModuleHub>(() => new ModuleHub(new ModuleTermPool()));

        public static ModuleHub Instance => HubInstance.Value;

        private ModuleHub(ModuleTermPool modulePool)
        {
            ModulePool    = modulePool;
            _createSubject = new Subject<ModuleCreated>();
            _removeSubject = new Subject<ModuleRemoved>();
        }

        public void Reset() => ModulePool.Clean();

        public void ModuleCreated(DependencyModule module) => _createSubject.OnNext(new ModuleCreated(module));

        public void ModuleRemoved(DependencyModule module) => _removeSubject.OnNext(new ModuleRemoved(module));
    }
}
