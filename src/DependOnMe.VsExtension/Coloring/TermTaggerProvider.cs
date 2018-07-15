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
    internal sealed class TermTaggerProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag => new TermTagger() as ITagger<T>;
    }
}
