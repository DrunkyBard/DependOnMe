using Microsoft.FSharp.Text.Lexing;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using System.Collections.Generic;
using System.Linq;

namespace DependOnMe.VsExtension.Completion
{
    public sealed class TestCompletionSource : ICompletionSource
	{
		private readonly TestCompletionSourceProvider _sourceProvider;
		private readonly ITextBuffer _textBuffer;
	    private readonly SVsServiceProvider _provider;

	    public TestCompletionSource(TestCompletionSourceProvider sourceProvider, ITextBuffer textBuffer, SVsServiceProvider provider)
		{
			_sourceProvider = sourceProvider;
			_textBuffer = textBuffer;
            _provider = provider;
        }

		public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
		{
	        var completions = new List<Microsoft.VisualStudio.Language.Intellisense.Completion>(4);
		    var navigator = _sourceProvider.NavigatorService.GetTextStructureNavigator(_textBuffer);
		    var fileName = GetFileName();
            var dte = (EnvDTE.DTE)_provider.GetService(typeof(EnvDTE.DTE));
            var ts = dte.ActiveWindow.Selection as EnvDTE.TextSelection;
		    var src = session.TextView.TextSnapshot.GetText();
            
            var suggestions = DslCompletion.suggestFrom(fileName, src, new Position(fileName, ts.CurrentLine, 0, ts.CurrentColumn));
		    var s = suggestions.Select(x => new Microsoft.VisualStudio.Language.Intellisense.Completion(x, x, x, null, null));
		    completions.AddRange(s);

		    ITextSnapshot snapshot = _textBuffer.CurrentSnapshot;
            var triggerPoint = (SnapshotPoint)session.GetTriggerPoint(snapshot);
		    var line  = triggerPoint.GetContainingLine();
		    var start = triggerPoint;

		    while (start > line.Start && !char.IsWhiteSpace((start - 1).GetChar()))
		    {
		        start -= 1;
		    }

            var applicableTo = snapshot.CreateTrackingSpan(new SnapshotSpan(start, triggerPoint), SpanTrackingMode.EdgeInclusive);



            //         foreach (string str in new []{ "addition", "adaptation", "subtraction", "summation" })
            //{
            //	completions.Add(new Completion(str, str, str, null, null));
            //}

            completionSets.Add(new CompletionSet(
				"Tokens",    //the non-localized title of the tab
				"Tokens",    //the display title of the tab
				//FindTokenSpanAtPosition(session.GetTriggerPoint(_textBuffer), session),
				applicableTo,
				completions,
				null));
		}

	    private string GetFileName()
	    {
	        if (_textBuffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument textDocument))
	        {
	            return textDocument.FilePath;
	        }

	        return null;
	    }

	    private ITrackingSpan FindTokenSpanAtPosition(ITrackingPoint point, ICompletionSession session)
		{
			var currentPoint = session.TextView.Caret.Position.BufferPosition - 1;
            var navigator = _sourceProvider.NavigatorService.GetTextStructureNavigator(_textBuffer);
			var extent = navigator.GetExtentOfWord(currentPoint);
		    var findTokenSpanAtPosition = currentPoint.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
		    var line = session.TextView.TextSnapshot.GetLineFromPosition(extent.Span.Start.Position);
		    var g = extent.Span.Span.Start - line.Start.Position;
            return findTokenSpanAtPosition;
		}

		public void Dispose()
		{ }
	}
}
