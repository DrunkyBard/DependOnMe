using CodeJam;
using Microsoft.FSharp.Text.Lexing; // TODO: remove FsLex dependency
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DependOnMe.VsExtension.Completion
{
    public sealed class CompletionSource : ICompletionSource
	{
		private readonly ITextBuffer _textBuffer;
	    private readonly SVsServiceProvider _provider;
	    private readonly Func<string, string, Position, IReadOnlyCollection<string>> _getSuggestions;

	    public CompletionSource(ITextBuffer textBuffer, SVsServiceProvider provider, Func<string, string, Position, IReadOnlyCollection<string>> getSuggestions)
		{
            Code.NotNull(textBuffer, nameof(textBuffer));
            Code.NotNull(provider, nameof(provider));
            Code.NotNull(getSuggestions, nameof(getSuggestions));

		    _textBuffer     = textBuffer;
            _provider       = provider;
		    _getSuggestions = getSuggestions;
		}

		public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
		{
		    Code.NotNull(session, nameof(session));
		    Code.NotNull(completionSets, nameof(completionSets));

            var triggerPointOpt = session.GetTriggerPoint(_textBuffer.CurrentSnapshot);
		    var fileName        = _textBuffer.TryGetFileName();

		    if (!triggerPointOpt.HasValue || !fileName.IsSome)
		    {
		        return;
		    }

		    var triggerPoint = triggerPointOpt.Value;
            var dteService   = (EnvDTE.DTE)_provider.GetService(typeof(EnvDTE.DTE));
            var selection    = (EnvDTE.TextSelection)dteService.ActiveWindow.Selection;
		    var src          = session.TextView.TextSnapshot.GetText();
		    var suggestions  = _getSuggestions(fileName.Value, src, new Position(fileName.Value, selection.CurrentLine, 0, selection.CurrentColumn));
		    var completions  = suggestions.Select(x => new Microsoft.VisualStudio.Language.Intellisense.Completion(x, x, x, null, null));

		    var start = triggerPoint;
		    var line  = triggerPoint.GetContainingLine();

            while (start > line.Start && !char.IsWhiteSpace((start - 1).GetChar()))
		    {
		        start -= 1;
		    }

            var applicableTo = _textBuffer.CurrentSnapshot.CreateTrackingSpan(new SnapshotSpan(start, triggerPoint), SpanTrackingMode.EdgeInclusive);
            completionSets.Add(new CompletionSet(
				"Tokens",    
				"Tokens",    
				applicableTo,
				completions,
				null));
		}

		public void Dispose()
		{ }
	}
}
