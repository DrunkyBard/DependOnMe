using Compilation;
using DependOnMe.VsExtension.ContentTypeDefinition;
using DependOnMe.VsExtension.Messaging;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SolutionEvents = Microsoft.VisualStudio.Shell.Events.SolutionEvents;
using Task = System.Threading.Tasks.Task;

namespace DependOnMe.VsExtension.Startup
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = false)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.None)]
    public sealed class StartupPackage : AsyncPackage
    {
        private const string FullPathProp  = "FullPath";
        private const string ExtensionProp = "Extension";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            bool isSolutionLoaded = await IsSolutionLoadedAsync();

            if (isSolutionLoaded)
            {
                HandleOpenSolution();
            }

            SolutionEvents.OnAfterOpenSolution += (_, __) => HandleOpenSolution();
        }

        private async Task<bool> IsSolutionLoadedAsync()
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            var solService = (IVsSolution) await GetServiceAsync(typeof(SVsSolution));
            ErrorHandler.ThrowOnFailure(solService.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object value));

            return value is bool isSolOpen && isSolOpen;
        }

        private void HandleOpenSolution()
        {            
            var dte = (DTE)GetGlobalService(typeof(DTE));
            var projects = dte.Solution.Projects;
            var modules  = new List<string>();
            var compiler = new Compiler();

            foreach (Project myProject in projects)
            foreach (ProjectItem myProjectProjectItem in myProject.ProjectItems)
            {
                modules.AddRange(GetModules(myProjectProjectItem));
            }

            if (modules.Count == 0)
            {
                return;
            }

            var moduleUnits = compiler.CompileModule(modules.ToArray());

            moduleUnits
                .SelectMany(mUnit => mUnit.CompilationUnit.OnlyValidModules())
                .ForEach(x =>
                {
                    ModuleHub.Instance.ModulePool.Request(
                        x.Name,
                        x.ClassRegistrations.ToViewModels(),
                        x.ModuleRegistrations.CollectSubModules());
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
