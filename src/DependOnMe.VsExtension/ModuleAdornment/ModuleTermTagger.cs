using DependOnMe.VsExtension.Messaging;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DependOnMe.VsExtension.ModuleAdornment
{
    internal sealed class ModuleTermTagger : ITagger<ModuleTermTag>
    {
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private static readonly Regex ModuleTermRegex =
            new Regex(@"MODULE (?<moduleName>\w+(?:[\w|\d]*\.\w[\w|\d]*)*)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public IEnumerable<ITagSpan<ModuleTermTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var availableModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var span in spans)
            {
                var text       = span.GetText();
                var match      = ModuleTermRegex.Match(text);
                var moduleName = match.Groups["moduleName"].Value;

                if (!match.Success || 
                    string.IsNullOrWhiteSpace(moduleName) ||
                    availableModules.Contains(moduleName) ||
                    !ModuleHub.Instance.ModulePool.Contains(moduleName))
                {
                    continue;
                }

                availableModules.Add(moduleName);

                var modSpan = new SnapshotSpan(span.Snapshot, new Span(span.Start.Position + match.Index, match.Length));
                var termTag = new ModuleTermTag(moduleName, 0, 10, 0, 0, 0, PositionAffinity.Successor, modSpan, this);

                yield return new TagSpan<ModuleTermTag>(modSpan, termTag);
            }
        }
    }
}
