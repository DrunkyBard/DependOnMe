using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DependOnMe.VsExtension.Coloring
{
    internal sealed class TermTag : ITag
    {
        public readonly string Classifier;

        public TermTag(string classifier)
        {
            if (string.IsNullOrWhiteSpace(classifier))
            {
                throw new ArgumentNullException(nameof(classifier));
            }

            Classifier = classifier;
        }
    }

    internal class TermTagger : ITagger<TermTag>
    {
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        IEnumerable<ITagSpan<TermTag>> ITagger<TermTag>.GetTags(NormalizedSnapshotSpanCollection spans) => 
            spans
                .SelectMany(curSpan => 
                    TextColoring
                        .colorLine(curSpan.GetText(), curSpan.Start)
                        .Select(termColor =>
                        {
                            var todoSpan = new SnapshotSpan(curSpan.Snapshot, new Span(termColor.StartPos - 1, termColor.Length));

                            return new TagSpan<TermTag>(todoSpan, new TermTag(termColor.Classifier));
                        }));
    }
}
