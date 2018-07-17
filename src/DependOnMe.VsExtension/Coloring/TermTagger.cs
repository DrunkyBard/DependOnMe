using CodeJam;
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
        private readonly Func<string, int, IReadOnlyCollection<TextColoring.TermColor>> _getColor;
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public TermTagger(Func<string, int, IReadOnlyCollection<TextColoring.TermColor>> getColor)
        {
            Code.NotNull(getColor, nameof(getColor));

            _getColor = getColor;
        }

        IEnumerable<ITagSpan<TermTag>> ITagger<TermTag>.GetTags(NormalizedSnapshotSpanCollection spans) => 
            spans
                .SelectMany(curSpan => 
                        _getColor(curSpan.GetText(), curSpan.Start)
                        .Select(termColor =>
                        {
                            var todoSpan = new SnapshotSpan(curSpan.Snapshot, new Span(termColor.StartPos - 1, termColor.Length));

                            return new TagSpan<TermTag>(todoSpan, new TermTag(termColor.Classifier));
                        }));
    }
}
