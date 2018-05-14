using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System.Collections.Generic;

namespace DependOnMe.VsExtension.Intellisense
{
    public sealed class TestCompletionSource : ICompletionSource
	{
		private readonly TestCompletionSourceProvider _sourceProvider;
		private readonly ITextBuffer _textBuffer;

		public TestCompletionSource(TestCompletionSourceProvider sourceProvider, ITextBuffer textBuffer)
		{
			_sourceProvider = sourceProvider;
			_textBuffer = textBuffer;
		}

		public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
		{
	        var completions = new List<Completion>(4);

			foreach (string str in new []{ "addition", "adaptation", "subtraction", "summation" })
			{
				completions.Add(new Completion(str, str, str, null, null));
			}

			completionSets.Add(new CompletionSet(
				"Tokens",    //the non-localized title of the tab
				"Tokens",    //the display title of the tab
				FindTokenSpanAtPosition(session.GetTriggerPoint(_textBuffer), session),
				completions,
				null));
		}

		private ITrackingSpan FindTokenSpanAtPosition(ITrackingPoint point, ICompletionSession session)
		{
			var currentPoint = session.TextView.Caret.Position.BufferPosition - 1;
			var navigator = _sourceProvider.NavigatorService.GetTextStructureNavigator(_textBuffer);
			var extent = navigator.GetExtentOfWord(currentPoint);

			return currentPoint.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
		}

		public void Dispose()
		{ }
	}
}
