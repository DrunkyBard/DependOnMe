using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace DependOnMe.VsExtension.ModuleDeclarationTagging
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("drm")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    [TagType(typeof(SpaceNegotiatingAdornmentTag))]
    [TagType(typeof(ModuleDeclarationTag))]   
    internal sealed class ModuleDeclarationTaggerProvider : ITaggerProvider
    {
        [Import]
        internal IBufferTagAggregatorFactoryService BufferTagAggregatorFactoryService;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag 
            => buffer.Properties.GetOrCreateSingletonProperty(() => new ModuleDeclarationTagger()) as ITagger<T>;
    }
}
