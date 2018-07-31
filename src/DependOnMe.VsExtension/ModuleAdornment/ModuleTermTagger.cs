using Compilation;
using DependOnMe.VsExtension.ContentTypeDefinition;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace DependOnMe.VsExtension.ModuleAdornment
{
    internal sealed class ModuleTermTagger : ITagger<ModuleTermTag>
    {
        private const string FullPathProp  = "FullPath";
        private const string ExtensionProp = "Extension";

        private readonly ITextDocumentFactoryService _textDocumentFactoryService;
        private readonly ITextBuffer _buffer;
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private static readonly Regex ModuleTermRegex =
            new Regex(@"MODULE\s+(?<moduleName>\w+(?:[\w|\d]*\.\w[\w|\d]*)*)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public ModuleTermTagger(ITextDocumentFactoryService textDocumentFactoryService, ITextBuffer buffer)
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

        internal static Project GetActiveProject()
        {
            DTE dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
            return GetActiveProject(dte);
        }

        internal static Project GetActiveProject(DTE dte)
        {
            Project activeProject = null;

            Array activeSolutionProjects = dte.ActiveSolutionProjects as Array;
            if (activeSolutionProjects != null && activeSolutionProjects.Length > 0)
            {
                activeProject = activeSolutionProjects.GetValue(0) as Project;
            }

            return activeProject;
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
            var fullPath = string.Empty;

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

            if (extension.Equals(ContentType.DotTest))
            {
                yield return fullPath;
            }
        }

        public IEnumerable<ITagSpan<ModuleTermTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
            {
                yield break;
            }

            var proj = GetActiveProject();
            var tests = new List<string>();

            foreach (ProjectItem projItem in proj.ProjectItems)
            {
                tests.AddRange(GetModules(projItem));
            }

            if (!tests.Any(x => x.Equals(FilePath(), StringComparison.OrdinalIgnoreCase)))
            {
                yield break;
            }

            var src = spans[0].Snapshot.GetText();
            RefTable.Instance.TryRemoveTestRefs(FilePath());
            var units = Compiler.Instance.CompileTestOnFly(src, FilePath()).OnlyValidTests();

            foreach (var span in spans)
            {
                var containingTest = units.ValidTests.FirstOrDefault(x =>
                    x.Position.Item1.AbsoluteOffset <= span.Start.Position + 1 &&
                    x.Position.Item2.AbsoluteOffset <= span.End.Position + 1);

                var inError = units
                    .Errors
                    .Any(err => err.Item1.AbsoluteOffset <= span.Start.Position || span.End.Position <= err.Item2.AbsoluteOffset);                

                //if (containingTest == null || inError)
                if (containingTest == null)
                {
                    continue;
                }

                bool HasModule(string moduleName) => containingTest
                    .RegisteredModules
                    .Select(x => x.Name)
                    .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .Where(x => x.Count() == 1)
                    .Select(x => x.First())
                    .Any(regModule => regModule.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

                var text         = span.GetText();
                var foundModules = ModuleTermRegex
                    .Matches(text)
                    .Cast<Match>()
                    .Select(m => (success: m.Success, moduleName: m.Groups["moduleName"].Value, idx: m.Index, length: m.Length))
                    .Where(m => m.success && 
                                !string.IsNullOrWhiteSpace(m.moduleName) &&
                                HasModule(m.moduleName))
                    .Select(m =>
                    {
                        var modSpan = new SnapshotSpan(span.Snapshot, new Span(span.Start.Position + m.idx, m.length));
                        var termTag = new ModuleTermTag(m.moduleName, containingTest.Name);

                        return new TagSpan<ModuleTermTag>(modSpan, termTag);
                    });

                foreach (var foundModule in foundModules)
                {
                    yield return foundModule;
                }
            }
        }
    }
}
