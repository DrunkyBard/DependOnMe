using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DependOnMe.VsExtension.Coloring
{
    public class TermClassifier : IClassifier
    {
        private readonly IClassificationTypeRegistryService _classificationTypeRegistry;

        internal TermClassifier(IClassificationTypeRegistryService registry)
        {
            _classificationTypeRegistry = registry;
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        /// <summary>
        /// Classify the given spans, which, for diff files, classifies
        /// a line at a time.
        /// </summary>
        /// <param name="span">The span of interest in this projection buffer.</param>
        /// <returns>The list of <see cref="ClassificationSpan"/> as contributed by the source buffers.</returns>
        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            var snapshot = span.Snapshot;
            if (snapshot.Length == 0)
            {
                return Enumerable.Empty<ClassificationSpan>().ToList();
            }

            var spans = new List<ClassificationSpan>();
            var startLine = span.Start.GetContainingLine().LineNumber;
            var endLine = span.End.GetContainingLine().LineNumber;
            var classifiers = Enumerable
                .Range(startLine, endLine - startLine)
                .SelectMany(lineNumber =>
                {
                    var line = snapshot.GetLineFromLineNumber(lineNumber);
                    var text = line.Snapshot.GetText();
                    
                    return TextColoring
                        .colorLine(text, line.Start.Position)
                        //.colorLine(text, 0)
                        .Select(c =>
                        {
                            var classifier = _classificationTypeRegistry.GetClassificationType(c.Classifier);
                            var newSpan    = new Span(c.StartPos, c.Length);
                            var snapSpan   = new SnapshotSpan(line.Snapshot, newSpan);

                            return new ClassificationSpan(snapSpan, classifier);
                        }).ToArray();
                }).ToArray();

            spans.AddRange(classifiers);

            return spans;
        }
    }
}
