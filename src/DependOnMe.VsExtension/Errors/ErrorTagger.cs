using CodeJam;
using Microsoft.FSharp.Text.Lexing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using FlattenError = Errors.FlattenError;

namespace DependOnMe.VsExtension.Errors
{
    internal sealed class ErrorTagger<T> : ITagger<IErrorTag>
    {
        private readonly ITextView _textView;
        private readonly ITextBuffer _buffer;
        private readonly Func<string, string, (T unit, IReadOnlyCollection<FlattenError> errors)> _compile;
        private readonly Func<T, IReadOnlyCollection<FlattenError>> _checkSemantic;
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public ErrorTagger(
            ITextView textView, 
            ITextBuffer buffer,
            Func<string, string, (T unit, IReadOnlyCollection<FlattenError> errors)> compile,
            Func<T, IReadOnlyCollection<FlattenError>> checkSemantic)
        {
            Code.NotNull(textView, nameof(textView));
            Code.NotNull(buffer, nameof(buffer));
            Code.NotNull(compile, nameof(compile));
            Code.NotNull(checkSemantic, nameof(checkSemantic));

            _textView      = textView;
            _buffer        = buffer;
            _compile       = compile;
            _checkSemantic = checkSemantic;
        }

        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans) => 
            _buffer
            .TryGetFileName()
            .ContinueWith(
                fileName => ErrorTags(fileName, spans),
                Enumerable.Empty<ITagSpan<IErrorTag>>);

        private IEnumerable<ITagSpan<IErrorTag>> ErrorTags(string fileName, NormalizedSnapshotSpanCollection spans)
        {
            var (unit, flattenErrors) = _compile(_textView.TextSnapshot.GetText(), fileName);

            return flattenErrors
                .Select(err => (span: SnapshotSpan(err.From, err.To, _textView.TextSnapshot), msg: err.Message))
                .Concat(SemanticErrors(unit, _textView.TextSnapshot))
                .Where(err  => spans.IntersectsWith(err.span))
                .Select(err => new TagSpan<IErrorTag>(err.span, new ErrorTag(PredefinedErrorTypeNames.SyntaxError, err.msg)));
        }

        private IEnumerable<(SnapshotSpan span, string msg)> SemanticErrors(T unit, ITextSnapshot snapshot)
            => _checkSemantic(unit).Select(err => (SnapshotSpan(err.From, err.To, snapshot), err.Message));

        private static SnapshotSpan SnapshotSpan(Position from, Position to, ITextSnapshot snapshot)
        {
            var offset = from.AbsoluteOffset - 1; // because compiler starts from 1
            var length = to.AbsoluteOffset - offset - 1;

            return new SnapshotSpan(snapshot, offset, length);
        }
    }
}
