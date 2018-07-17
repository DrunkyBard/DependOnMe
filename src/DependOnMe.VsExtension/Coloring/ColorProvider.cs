using DependOnMe.VsExtension.ContentTypeDefinition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;


namespace DependOnMe.VsExtension.Coloring
{
    [Export(typeof(IClassifierProvider))]
    [ContentType(ContentType.Test)]
    internal class TestColorClassifierProvider : IClassifierProvider
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

    [Export(typeof(IClassifierProvider))]
    [ContentType(ContentType.Module)]
    internal class ModuleColorClassifierProvider : IClassifierProvider
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
