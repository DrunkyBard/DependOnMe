using Compilation;
using CompilationUnit;
using DependOnMe.VsExtension.ContentTypeDefinition;
using DependOnMe.VsExtension.Messaging;
using DslAst;
using EnvDTE;
using JetBrains.Annotations;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Events;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using SolutionEvents = Microsoft.VisualStudio.Shell.Events.SolutionEvents;

namespace DependOnMe.VsExtension.Startup
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = false)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.None)]
    [UsedImplicitly]
    public sealed class StartupPackage : Package
    {
        private const string FullPathProp  = "FullPath";
        private const string ExtensionProp = "Extension";

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            SolutionEvents.OnAfterOpenSolution  -= HandleOpenSolution;
            SolutionEvents.OnAfterCloseSolution -= Cleanup;
        }

        protected override void Initialize()
        {
            bool isSolutionLoaded = IsSolutionLoadedAsync();

            if (isSolutionLoaded)
            {
                HandleOpenSolution(null, null);
            }
            
            SolutionEvents.OnAfterOpenSolution  += HandleOpenSolution;
            SolutionEvents.OnAfterCloseSolution += Cleanup;
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

            var moduleUnits = Compiler.Instance.CompileModule(modules.ToArray());

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
