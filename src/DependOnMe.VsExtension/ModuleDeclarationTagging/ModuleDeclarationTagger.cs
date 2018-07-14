using Compilation;
using CompilationUnit;
using DependOnMe.VsExtension.Messaging;
using DependOnMe.VsExtension.ModuleAdornment.UI;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ModuleRegistration = DslAst.Extension.ValidModuleRegistration;

namespace DependOnMe.VsExtension.ModuleDeclarationTagging
{
    internal sealed class ModuleDeclarationTagger : ITagger<ModuleDeclarationTag>
    {
        private readonly ITextDocumentFactoryService _textDocumentFactoryService;
        private readonly ITextBuffer _buffer;
        private static readonly IEnumerable<ITagSpan<ModuleDeclarationTag>> EmptyTags = Enumerable.Empty<ITagSpan<ModuleDeclarationTag>>();
        private static readonly Compiler Compiler = new Compiler();

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private readonly ModuleTermPool _modulePool = ModuleHub.Instance.ModulePool;

        private ModuleRegistration[] _previousUnits;

        public ModuleDeclarationTagger(ITextDocumentFactoryService textDocumentFactoryService, ITextBuffer buffer)
        {
            _textDocumentFactoryService = textDocumentFactoryService;
            _buffer = buffer;
        }

        private string FilePath()
        {
            var success = _textDocumentFactoryService.TryGetTextDocument(_buffer, out var document);
            Debug.Assert(success);

            return document.FilePath;
        }

        private void ProcessNewModule(Occurence<ModuleRegistration> validModule)
        {
            var moduleName = validModule.Key.Name;
            var fUnit = new FileCompilationUnit<ModuleRegistration>(FilePath(), validModule.Key);

            if (RefTable.Instance.HasDuplicates(moduleName))
            {
                _modulePool.Request(fUnit);
                ModuleHub.Instance.ModuleDuplicated(moduleName);

                return;
            }

            //_modulePool.Request(fUnit);

            if (validModule.Occurences.Count == 1)
            {
                var newModule = _modulePool.Request(
                    fUnit,
                    validModule.Key.ClassRegistrations.ToViewModels(),
                    validModule.Key.ModuleRegistrations.CollectSubModules());

                ModuleHub.Instance.ModuleCreated(newModule);
            }
            else
            {
                ModuleHub.Instance.ModuleDuplicated(moduleName);
            }
        }

        private void ProcessNewModule(ModuleRegistration validModule)
        {
            var fUnit = new FileCompilationUnit<ModuleRegistration>(FilePath(), validModule);

            if (RefTable.Instance.HasDuplicates(validModule.Name))
            {
                _modulePool.Request(fUnit);
                ModuleHub.Instance.ModuleDuplicated(validModule.Name);

                return;
            }

            var newModule = _modulePool.Request(
                fUnit,
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

            RefTable.Instance.TryRemoveDeclarations(FilePath());

            var wholeText       = spans.First().Snapshot.GetText();
            var compilationUnit = Compiler.CompileModuleOnFly(wholeText, FilePath()).OnlyValidModules();

            if (_previousUnits == null)
            {
                _previousUnits = compilationUnit.ValidModules;

                foreach (var validModule in compilationUnit.ValidModules)
                {
                    ProcessNewModule(validModule);
                }
            }
            else
            {
                Func<ModuleRegistration, ModuleRegistration, bool> equals = (l, r) => l.Name.Equals(r.Name, StringComparison.OrdinalIgnoreCase);
                Func<ModuleRegistration, int> getHashCode = m => m.Name.ToUpperInvariant().GetHashCode();
                var comparer = EqualityComparerFactory.Create(equals, getHashCode);
                var (newModules, sameModules, removedModules) = compilationUnit.ValidModules.SplitD(_previousUnits, comparer);

                foreach (var validModuleRegistration in newModules)
                {
                    ProcessNewModule(validModuleRegistration);
                }

                foreach (var validModuleRegistration in removedModules)
                {
                    _modulePool
                        .TryRelease(new FileCompilationUnit<string>(FilePath(), validModuleRegistration.Key.Name))
                        .WhenHasValueThen(ModuleHub.Instance.ModuleRemoved);
                }

                foreach (var sameModule in sameModules)
                {
                    var fUnit = new FileCompilationUnit<ModuleRegistration>(FilePath(), sameModule.leftIntersection.Key);
                    
                    if (sameModule.leftIntersection.Occurences.Count > 1 ||
                        RefTable.Instance.HasDuplicates(sameModule.leftIntersection.Key.Name))
                    {
                        continue;
                    }

                    var (duplicated, module) = _modulePool.Request(fUnit); 
                    Debug.Assert(!duplicated, "Ref table has no dublicate, but module pool contains it");

                    var left  = sameModule.leftIntersection.Key;
                    var right = sameModule.rightIntersection.Key;

                    module.Dependencies.Clear();

                    left
                        .ClassRegistrations
                        .Select(x => new PlainDependency(x.Dependency, x.Implementation))
                        .ForEach(newDep => module.Add(newDep));

                    left
                        .ModuleRegistrations
                        .ForEach(newModule => _modulePool.TryRequest(newModule.Name).WhenHasValueThen(m => module.Add(m)));
                    
                    //right
                    //    .ClassRegistrations
                    //    .Select(x => new PlainDependency(x.Dependency, x.Implementation))
                    //    .ForEach(oldDep => module.Remove(oldDep));
                    //left
                    //    .ClassRegistrations
                    //    .Select(x => new PlainDependency(x.Dependency, x.Implementation))
                    //    .ForEach(newDep => module.Add(newDep));
                    
                    //right
                    //    .ModuleRegistrations
                    //    .ForEach(oldModule => _modulePool.TryRequest(oldModule.Name).WhenHasValueThen(m => module.Remove(m)));
                    //left
                    //    .ModuleRegistrations
                    //    .ForEach(newModule => _modulePool.TryRequest(newModule.Name).WhenHasValueThen(m => module.Add(m)));
                }
            }

            _previousUnits = compilationUnit.ValidModules;

            return EmptyTags;
        }
    }
}
