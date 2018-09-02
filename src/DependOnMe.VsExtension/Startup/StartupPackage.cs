using Compilation;
using CompilationUnit;
using DependOnMe.VsExtension.ContentTypeDefinition;
using DependOnMe.VsExtension.Messaging;
using DslAst;
using EnvDTE;
using EnvDTE80;
using JetBrains.Annotations;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Events;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using SolutionEvents = Microsoft.VisualStudio.Shell.Events.SolutionEvents;

namespace DependOnMe.VsExtension.Startup
{
    internal static class EnvDteConstants
    {
        public const string VsProjectItemKindPhysicalFile = "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}";
    }

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = false)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.None)]
    [UsedImplicitly]
    public sealed class StartupPackage : Package
    {
        [Import]
        internal SVsServiceProvider Provider;

        private const string FullPathProp  = "FullPath";
        private const string ExtensionProp = "Extension";

        private DTE _dte;
        private Events _events;
        private Events2 _events2;
        private ProjectItemsEvents _projEvents;
        private ProjectItemsEvents _projEvents1;
        private ProjectItemsEvents _projEvents2;

        private IDisposable _itemAddSubscription;
        private IDisposable _itemRemoveSubscription;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
                        
            SolutionEvents.OnAfterOpenSolution  -= HandleOpenSolution;
            SolutionEvents.OnAfterCloseSolution -= Cleanup;

            _itemAddSubscription?.Dispose();
            _itemRemoveSubscription?.Dispose();
        }

        protected override void Initialize()
        {
            bool isSolutionLoaded = IsSolutionLoadedAsync();

            if (isSolutionLoaded)
            {
                HandleOpenSolution(null, null);
            }

            _dte = (DTE)GetGlobalService(typeof(DTE));
            //var dte2 = (DTE2)GetGlobalService(typeof(DTE2));
            //_events = _dte.Events;
            _events2 = (Events2)_dte.Events;
            //_projEvents = (ProjectItemsEvents)_events.GetObject("CSharpProjectItemsEvents");
            _projEvents2 = _events2.ProjectItemsEvents;

            bool OnlyDslFiles(ProjectItem projItem)
                => projItem.Kind.Equals(EnvDteConstants.VsProjectItemKindPhysicalFile, StringComparison.OrdinalIgnoreCase) &&
                   projItem.Name != null && 
                   (Path.GetExtension(projItem.Name).Equals(ContentType.DotModule) || Path.GetExtension(projItem.Name).Equals(ContentType.DotTest));

            _itemAddSubscription = Observable
                .FromEvent<_dispProjectItemsEvents_ItemAddedEventHandler, ProjectItem>(
                    h => _projEvents2.ItemAdded += h,
                    h => _projEvents2.ItemAdded -= h)
                .Where(OnlyDslFiles)
                .Subscribe(OnNewDslItem);

            _itemRemoveSubscription = Observable
                .FromEvent<_dispProjectItemsEvents_ItemRemovedEventHandler, ProjectItem>(
                    h => _projEvents2.ItemRemoved += h,
                    h => _projEvents2.ItemRemoved -= h)
                .Where(OnlyDslFiles)
                .Subscribe(OnDeleteDslItem);
            
            //_projEvents2.ItemAdded += item =>
            //{
            //    var fNames    = new List<string>();

            //    for (short i = 0; i < item.FileCount; i++)
            //    {
            //        fNames.Add(item.FileNames[i]);
            //    }
                
            //    var iName     = item.Name;
            //    var iObj      = item.Object;
            //    var iExtNames = item.ExtenderNames;
            //    var kind      = item.Kind;
            //    //var isFolder  = item.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder;

            //    //var t = new EnvDTE.Constants();

            //    var a = 1;
            //};
            //_projEvents2.ItemRemoved += item =>
            //{
            //    var a = 1;
            //};
            //_projEvents.ItemRemoved += item =>
            //{
            //    var q = 1;
            //};
            //_events.SolutionItemsEvents.ItemRemoved += item =>
            //{
            //    var a = 1;
            //};

            SolutionEvents.OnAfterOpenSolution  += HandleOpenSolution;
            SolutionEvents.OnAfterCloseSolution += Cleanup;
        }

        private static void OnNewDslItem(ProjectItem item)
        {
            var fPath     = item.FileNames[0];
            var extension = Path.GetExtension(fPath);

            if (string.Equals(extension, ContentType.DotTest))
            {
                RefTable.Instance.TryRemoveTestRefs(fPath);
                Compiler.Instance.CompileTest(fPath);

                return;
            }

            if (string.Equals(extension, ContentType.DotModule))
            {
                RefTable.Instance.TryRemoveDeclarations(fPath);
                var moduleUnit = Compiler.Instance.CompileModule(fPath);
                moduleUnit
                    .CompilationUnit
                    .OnlyValidModules()
                    .ValidModules
                    .Select(validModule => new FileCompilationUnit<Extension.ValidModuleRegistration>(moduleUnit.FilePath, validModule))
                    .ForEach(x =>
                    {
                        ModuleHub.Instance.ModulePool.Request(
                            x,
                            x.CompilationUnit.ClassRegistrations.ToViewModels(),
                            x.CompilationUnit.ModuleRegistrations.CollectSubModules());
                    });

                return;
            }

            Debug.Fail($"Unexpected content in OnNewDslItem subscription: {fPath}");
        }

        private static void OnDeleteDslItem(ProjectItem item)
        {
            var fPath     = item.FileNames[0];
            var extension = Path.GetExtension(fPath);

            if (string.Equals(extension, ContentType.DotTest))
            {
                RefTable.Instance.TryRemoveTestRefs(fPath);

                return;
            }

            if (string.Equals(extension, ContentType.DotModule))
            {
                RefTable
                    .Instance
                    .GetAllModulesFrom(fPath)                    
                    .ForEach(modName =>
                        ModuleHub
                            .Instance.ModulePool
                            .TryRelease(new FileCompilationUnit<string>(fPath, modName))
                            .WhenHasValueThen(ModuleHub.Instance.ModuleRemoved));

                return;
            }

            Debug.Fail($"Unexpected content in OnNewDslItem subscription: {fPath}");
        }

        private void Cleanup(object _, EventArgs __)
        {
            RefTable.Instance.Clean();
            Parser.testIndex.Clear();
            Parser.errorLogger = null;
            ModuleParser.testIndex.Clear();
            ModuleParser.errorLogger = null;
            ModuleHub.Instance.ModulePool.Clean();
        }

        private bool IsSolutionLoadedAsync()
        {
            var solService = (IVsSolution) GetService(typeof(SVsSolution));
            ErrorHandler.ThrowOnFailure(solService.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object value));

            return value is bool isSolOpen && isSolOpen;
        }

        private void HandleOpenSolution(object _, OpenSolutionEventArgs __)
        {            
            var dte = (DTE)GetGlobalService(typeof(DTE));
            var projects = dte.Solution.Projects;
            var modules  = new List<string>();
            
            foreach (Project myProject in projects)
            foreach (ProjectItem myProjectProjectItem in myProject.ProjectItems)
            {
                modules.AddRange(GetModules(myProjectProjectItem));
            }

            if (modules.Count == 0)
            {
                return;
            }

            var moduleUnits = Compiler.Instance.CompileModules(modules.ToArray());

            moduleUnits
                .SelectMany(mUnit => mUnit
                    .CompilationUnit
                    .OnlyValidModules()
                    .ValidModules
                    .Select(validModule => new FileCompilationUnit<Extension.ValidModuleRegistration>(mUnit.FilePath, validModule)))
                .ForEach(x =>
                {
                    ModuleHub.Instance.ModulePool.Request(
                        x,
                        x.CompilationUnit.ClassRegistrations.ToViewModels(),
                        x.CompilationUnit.ModuleRegistrations.CollectSubModules());
                });
        }

        IEnumerable<string> GetModules(ProjectItem item)
        {
            if (item.ProjectItems == null)
            {
                yield break;
            }

            foreach (ProjectItem projItem in item.ProjectItems)
            foreach (var contentType in GetModules(projItem))
            {
                yield return contentType;
            }

            var extension = string.Empty;
            var fullPath  = string.Empty;

            foreach (Property itemProperty in item.Properties)
            {
                if (itemProperty.Name.Equals(FullPathProp))
                {
                    fullPath = (string)itemProperty.Value;
                }

                if (itemProperty.Name.Equals(ExtensionProp))
                {
                    extension = (string)itemProperty.Value;
                }

                if (!string.IsNullOrWhiteSpace(fullPath) && !string.IsNullOrWhiteSpace(extension))
                {
                    break;
                }
            }

            if (extension.Equals(ContentType.DotModule))
            {
                yield return fullPath;
            }
        }
    }
}
