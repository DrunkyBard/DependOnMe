using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace DependOnMe.VsExtension.Intellisense
{
    [Export(typeof(ICompletionSourceProvider))]
	[ContentType("hid")]
	[Name("token completion")]
	public sealed class TestCompletionSourceProvider : ICompletionSourceProvider
	{
		[Import]
		public ITextStructureNavigatorSelectorService NavigatorService { get; set; }

		public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer) => new TestCompletionSource(this, textBuffer);
	}
}
