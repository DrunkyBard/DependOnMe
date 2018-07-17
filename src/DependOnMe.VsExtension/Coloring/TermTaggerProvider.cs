using DependOnMe.VsExtension.ContentTypeDefinition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace DependOnMe.VsExtension.Coloring
{
    [Export(typeof(ITaggerProvider))]
    [ContentType(ContentType.Test)]
    [TagType(typeof(TermTag))]
    internal sealed class TestTermTaggerProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag 
            => new TermTagger(TestTextColoring.colorLine) as ITagger<T>;
    }

    [Export(typeof(ITaggerProvider))]
    [ContentType(ContentType.Module)]
    [TagType(typeof(TermTag))]
    internal sealed class ModuleTermTaggerProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag 
            => new TermTagger(ModuleTextColoring.colorLine) as ITagger<T>;
    }
}
