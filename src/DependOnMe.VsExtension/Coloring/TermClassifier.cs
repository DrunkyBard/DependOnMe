using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DependOnMe.VsExtension.Coloring
{
    internal sealed class TermClassifier : IClassifier
    {
        private readonly ITagAggregator<TermTag> _tagger;
        private readonly IClassificationTypeRegistryService _classificationRegistry;

        internal TermClassifier(ITagAggregator<TermTag> tagger, IClassificationTypeRegistryService classificationRegistry)
        {
            _tagger = tagger;
            _classificationRegistry = classificationRegistry;
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span) => 
            _tagger
                .GetTags(span)
                .Select(tagSpan =>
                {
                    var todoSpan           = tagSpan.Span.GetSpans(span.Snapshot).First();
                    var classificationType = _classificationRegistry.GetClassificationType(tagSpan.Tag.Classifier);

                    return new ClassificationSpan(todoSpan, classificationType);
                }).ToList();
    }
}
