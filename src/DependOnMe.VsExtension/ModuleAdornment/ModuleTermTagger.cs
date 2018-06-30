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
            foreach (var span in spans)
            {
                var text = span.GetText();
                var match = ModuleTermRegex.Match(text);

                if (!match.Success || 
                    string.IsNullOrWhiteSpace(match.Groups["moduleName"].Value))
                {
                    continue;
                }
                
                var modSpan = new SnapshotSpan(span.Snapshot, new Span(span.Start.Position + match.Index, match.Length));
                var termTag = new ModuleTermTag(match.Groups["moduleName"].Value, 0, 10, 0, 0, 0, PositionAffinity.Successor, modSpan, this);

                yield return new TagSpan<ModuleTermTag>(modSpan, termTag);
            }
        }
    }
}
