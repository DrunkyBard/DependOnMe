using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;


namespace DependOnMe.VsExtension.Coloring
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("drt")]
    internal class ColorProvider : IClassifierProvider
    {
        [Import]
        internal IClassificationTypeRegistryService ClassificationRegistry;

        private static TermClassifier _diffClassifier;

        public IClassifier GetClassifier(ITextBuffer buffer) => _diffClassifier ?? (_diffClassifier = new TermClassifier(ClassificationRegistry));
    }

}
