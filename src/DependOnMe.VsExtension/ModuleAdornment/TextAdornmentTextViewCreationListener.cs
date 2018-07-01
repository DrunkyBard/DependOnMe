using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace DependOnMe.VsExtension.ModuleAdornment
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("drt")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class TextAdornmentTextViewCreationListener : IWpfTextViewCreationListener
    {
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("DrtModuleTermAdornment")]
        [Order(After = PredefinedAdornmentLayers.Selection, Before = PredefinedAdornmentLayers.Text)]
        private AdornmentLayerDefinition _editorAdornmentLayer;

        [Export(typeof(AdornmentLayerDefinition))]
        [Name("DrtModuleBtnTermAdornment")]
        [Order(After = PredefinedAdornmentLayers.Caret)]
        private AdornmentLayerDefinition _buttonAdornmentLayer;

        [Import]
        internal IBufferTagAggregatorFactoryService TagAggregatorFactory;

        public void TextViewCreated(IWpfTextView textView)
        {
            textView.Properties.GetOrCreateSingletonProperty(()
                => new ModuleTermAdornment(
                    textView,
                    TagAggregatorFactory.CreateTagAggregator<ModuleTermTag>(textView.TextBuffer)));
        }
    }
}
