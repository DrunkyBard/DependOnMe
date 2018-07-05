using Compilation;
using DependOnMe.VsExtension.Messaging;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DependOnMe.VsExtension.ModuleAdornment
{
    internal sealed class ModuleTermTagger : ITagger<ModuleTermTag>
    {
        private readonly IWpfTextView _textView;
        private readonly ITextBuffer _buffer;
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private static readonly Regex ModuleTermRegex =
            new Regex(@"MODULE (?<moduleName>\w+(?:[\w|\d]*\.\w[\w|\d]*)*)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        //public ModuleTermTagger(IWpfTextView textView, ITextBuffer buffer)
        //{
        //    _textView = textView;
        //    _buffer = buffer;

        //    _textView.LayoutChanged += (sender, args) =>
        //    {
        //        var a = 1;
        //    };

        //    _textView.BufferGraph.GraphBuffersChanged += (sender, args) =>
        //    {
        //        var a = 1;
        //    };

        //    _buffer.Changed += (sender, args) =>
        //    {
        //        var a = 1;
        //    };
        //}

        private static Compiler _compiler = new Compiler();

        public IEnumerable<ITagSpan<ModuleTermTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
            {
                yield break;
            }

            var src = spans[0].Snapshot.GetText();
            var units = _compiler.CompileTest(src).OnlyValidTests();

            foreach (var span in spans)
            {
                var containingTest = units.ValidTests.FirstOrDefault(x =>
                    x.Position.Item1.AbsoluteOffset < span.Start.Position &&
                    span.End.Position < x.Position.Item2.AbsoluteOffset);
                var inError = units
                    .Errors
                    .Any(err => err.Item1.AbsoluteOffset <= span.Start.Position || span.End.Position <= err.Item2.AbsoluteOffset);

                if (containingTest == null || inError)
                {
                    continue;
                }

                var text       = span.GetText();
                var match      = ModuleTermRegex.Match(text);
                var moduleName = match.Groups["moduleName"].Value;

                var foundModule = containingTest
                    .RegisteredModules
                    .Any(regModule => regModule.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

                if (!match.Success || 
                    string.IsNullOrWhiteSpace(moduleName) ||
                    !foundModule)
                {
                    continue;
                }

                var modSpan = new SnapshotSpan(span.Snapshot, new Span(span.Start.Position + match.Index, match.Length));
                //var termTag = new ModuleTermTag(moduleName, containingTest.Name, 0, 10, 0, 0, 0, PositionAffinity.Successor, modSpan, this);
                var termTag = ModuleHub
                    .Instance
                    .ModulePool
                    .TryRequest(moduleName)
                    .ContinueWith(
                        _  => SpacedTag(containingTest.Name, moduleName, modSpan),
                        () => NormalTag(containingTest.Name, moduleName, modSpan));


                yield return new TagSpan<ModuleTermTag>(modSpan, termTag);
            }
        }

        private ModuleTermTag SpacedTag(string testName, string moduleName, SnapshotSpan modSpan)
            => new ModuleTermTag(moduleName, testName, 0, 10, 0, 0, 0, PositionAffinity.Successor, modSpan, this);

        private ModuleTermTag NormalTag(string testName, string moduleName, SnapshotSpan modSpan)
            => new ModuleTermTag(moduleName, testName, 0, 0, 0, 0, 0, PositionAffinity.Successor, modSpan, this);
    }
}
