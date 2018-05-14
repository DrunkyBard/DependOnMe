using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace DependOnMe.VsExtension.Intellisense
{
    [Export(typeof(IVsTextViewCreationListener))]
	[ContentType("drt")]
	[TextViewRole(PredefinedTextViewRoles.Editable)]
	public sealed class TestCompletionHandlerProvider : IVsTextViewCreationListener
	{
		[Import]
		public IVsEditorAdaptersFactoryService AdapterService;
		[Import]
		public ICompletionBroker CompletionBroker { get; set; }

	    [Import]
	    internal SVsServiceProvider ServiceProvider;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
		{
			ITextView textView = AdapterService.GetWpfTextView(textViewAdapter);

		    if (textView == null)
			{
			    return;
			}
			
			TestCompletionCommandHandler CreateCommandHandler() => new TestCompletionCommandHandler(textViewAdapter, textView, this);

			textView.Properties.GetOrCreateSingletonProperty(CreateCommandHandler);
		}
	}
}
