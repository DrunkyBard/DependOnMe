using CompilationTable;
using DependOnMe.VsExtension.Messaging;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DependOnMe.VsExtension.ModuleDeclarationTagging
{
    internal sealed class ModuleDeclarationTagger : ITagger<ModuleDeclarationTag>
    {
        private static readonly IEnumerable<ITagSpan<ModuleDeclarationTag>> EmptyTags = Enumerable.Empty<ITagSpan<ModuleDeclarationTag>>();

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private HashSet<string> _containingModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);        

        private static readonly Regex ModuleTermRegex =
            new Regex(@"DEPENDENCYMODULE (?<moduleName>\w+(?:[\w|\d]*\.\w[\w|\d]*)*)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public IEnumerable<ITagSpan<ModuleDeclarationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
            {
                return EmptyTags;
            }

            var wholeText = spans.First().Snapshot.GetText();
            var foundModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Match match in ModuleTermRegex.Matches(wholeText))
            {
                var moduleName = match.Groups["moduleName"].Value;

                if (!match.Success ||
                    string.IsNullOrWhiteSpace(moduleName) ||
                    CompilationUnitTable.Instance.IsModuleDefined(moduleName))
                {
                    continue;
                }

                foundModules.Add(moduleName);
            }
            
            var removedModules = _containingModules.Except(foundModules).ToArray();
            var newModules     = foundModules.Except(_containingModules).ToArray();

            foreach (var removedModule in removedModules)
            {
                var remove = ModuleHub.Instance.ModulePool.TryRelease(removedModule);

                if (remove.success)
                {
                    ModuleHub
                        .Instance
                        .ModuleRemoved(remove.releasedModule);
                }
            }

            foreach (var newModuleName in newModules)
            {
                var newModule = ModuleHub
                    .Instance
                    .ModulePool
                    .Request(newModuleName);
                ModuleHub
                    .Instance
                    .ModuleCreated(newModule);
            }

            _containingModules = foundModules;

            return EmptyTags;
        }
    }
}
