using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace DependOnMe.VsExtension.ModuleDeclarationTagging
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("drm")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class MockModuleDeclarationListener : IWpfTextViewCreationListener
    {
        [Import]
        internal IBufferTagAggregatorFactoryService TagAggregatorFactory;

        public void TextViewCreated(IWpfTextView textView)
        {
            textView.Properties.GetOrCreateSingletonProperty(()
                => new DummyAdornment(
                    textView,
                    TagAggregatorFactory.CreateTagAggregator<ModuleDeclarationTag>(textView.TextBuffer)));
        }

        private sealed class DummyAdornment
        {
            private readonly ITagAggregator<ModuleDeclarationTag> _tagger;

            public DummyAdornment(IWpfTextView view, ITagAggregator<ModuleDeclarationTag> tagger)
            {
                _tagger = tagger;

                view.LayoutChanged += ViewOnLayoutChanged;
            }

            private void ViewOnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
            {
                foreach (var line in e.NewOrReformattedLines)
                {
                    _tagger.GetTags(line.ExtentAsMappingSpan);
                }
            }
        }
    }
}
