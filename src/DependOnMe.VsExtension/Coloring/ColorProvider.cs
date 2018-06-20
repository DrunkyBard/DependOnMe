using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;


namespace DependOnMe.VsExtension.Coloring
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("drt")]
    internal class ColorClassifierProvider : IClassifierProvider
    {
        [Import]
        internal IClassificationTypeRegistryService ClassificationRegistry;

        [Import]
        internal IBufferTagAggregatorFactoryService TagAggregatorFactory;

        public IClassifier GetClassifier(ITextBuffer buffer) => 
            new TermClassifier(
                TagAggregatorFactory.CreateTagAggregator<TermTag>(buffer), 
                ClassificationRegistry);
    }

}
