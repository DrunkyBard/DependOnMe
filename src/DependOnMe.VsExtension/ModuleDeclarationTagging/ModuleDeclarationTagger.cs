using Compilation;
using CompilationTable;
using DependOnMe.VsExtension.Messaging;
using DependOnMe.VsExtension.ModuleAdornment.UI;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using ModuleRegistration = DslAst.Extension.ValidModuleRegistration;

namespace DependOnMe.VsExtension.ModuleDeclarationTagging
{
    internal sealed class ModuleDeclarationTagger : ITagger<ModuleDeclarationTag>
    {
        private static readonly IEnumerable<ITagSpan<ModuleDeclarationTag>> EmptyTags = Enumerable.Empty<ITagSpan<ModuleDeclarationTag>>();
        private static readonly Compiler Compiler = new Compiler();

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private readonly ModuleTermPool _modulePool = ModuleHub.Instance.ModulePool;

        private ModuleRegistration[] _previousUnits;

        private void ProcessNewModule(ModuleRegistration validModule)
        {
            if (CompilationUnitTable.Instance.IsModuleDefined(validModule.Name))
            {
                ModuleHub.Instance.ModuleDuplicated(validModule.Name);

                return;
            }

            var newModule = _modulePool.Request(
                validModule.Name,
                validModule.ClassRegistrations.ToViewModels(),
                validModule.ModuleRegistrations.CollectSubModules());

            ModuleHub.Instance.ModuleCreated(newModule);
        }

        public IEnumerable<ITagSpan<ModuleDeclarationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
            {
                return EmptyTags;
            }

            var wholeText       = spans.First().Snapshot.GetText();
            var compilationUnit = Compiler.CompileModule(wholeText).OnlyValidModules();

            if (_previousUnits == null)
            {
                _previousUnits = compilationUnit;

                foreach (var validModule in compilationUnit)
                {
                    ProcessNewModule(validModule);
                }
            }
            else
            {
                Func<ModuleRegistration, ModuleRegistration, bool> equals = (l, r) => l.Name.Equals(r.Name, StringComparison.OrdinalIgnoreCase);
                Func<ModuleRegistration, int> getHashCode = m => m.Name.ToUpperInvariant().GetHashCode();
                var comparer = EqualityComparerFactory.Create(equals, getHashCode);
                var (newModules, sameModules, removedModules) = compilationUnit.Split(_previousUnits, comparer);

                foreach (var validModuleRegistration in newModules)
                {
                    ProcessNewModule(validModuleRegistration);
                }

                foreach (var validModuleRegistration in removedModules)
                {
                    _modulePool
                        .TryRelease(validModuleRegistration.Name)
                        .ContinueWith(
                            releasedModule => ModuleHub.Instance.ModuleRemoved(releasedModule),
                            () => throw new InvalidOperationException($"Broken invariant: trying to release module \'{validModuleRegistration.Name}\' that doesnt exist"));
                }

                foreach (var sameModule in sameModules)
                {
                    var module = _modulePool.Request(sameModule.leftIntersection.Name);
                    sameModule
                        .rightIntersection
                        .ClassRegistrations
                        .Select(x => new PlainDependency(x.Dependency, x.Implementation))
                        .ForEach(oldDep => module.Remove(oldDep));
                    sameModule
                        .leftIntersection
                        .ClassRegistrations
                        .Select(x => new PlainDependency(x.Dependency, x.Implementation))
                        .ForEach(newDep => module.Add(newDep));
                    
                    sameModule
                        .rightIntersection
                        .ModuleRegistrations
                        .ForEach(oldModule => _modulePool.TryRequest(oldModule.Name).WhenHasValueThen(m => module.Remove(m)));
                    sameModule
                        .leftIntersection
                        .ModuleRegistrations
                        .ForEach(newModule => _modulePool.TryRequest(newModule.Name).WhenHasValueThen(m => module.Add(m)));
                }
            }

            _previousUnits = compilationUnit;

            return EmptyTags;
        }
    }
}
