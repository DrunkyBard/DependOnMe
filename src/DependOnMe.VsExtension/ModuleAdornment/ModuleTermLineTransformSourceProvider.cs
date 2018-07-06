using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace DependOnMe.VsExtension.ModuleAdornment
{
    [Export(typeof(ILineTransformSourceProvider))]
    [ContentType("drt")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class ModuleTermLineTransformSourceProvider : ILineTransformSourceProvider
    {
        [Import]
        internal IBufferTagAggregatorFactoryService TagAggregatorFactory;

        public ILineTransformSource Create(IWpfTextView textView) 
            => textView.Properties.GetOrCreateSingletonProperty(
                () => new ModuleTermLineTransformSource(TagAggregatorFactory.CreateTagAggregator<ModuleTermTag>(textView.TextBuffer)));
    }
}
