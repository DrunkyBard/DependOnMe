using Compilation;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace DependOnMe.VsExtension.ModuleAdornment
{
    internal sealed class ModuleTermTagger : ITagger<ModuleTermTag>
    {
        private readonly ITextDocumentFactoryService _textDocumentFactoryService;
        private readonly ITextBuffer _buffer;
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private static readonly Regex ModuleTermRegex =
            new Regex(@"MODULE (?<moduleName>\w+(?:[\w|\d]*\.\w[\w|\d]*)*)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static Compiler _compiler = new Compiler();

        public ModuleTermTagger(ITextDocumentFactoryService textDocumentFactoryService, ITextBuffer buffer)
        {
            _textDocumentFactoryService = textDocumentFactoryService;
            _buffer = buffer;
        }

        private string FilePath()
        {
            var success = _textDocumentFactoryService.TryGetTextDocument(_buffer, out var document);
            Debug.Assert(success);

            return document.FilePath;
        }

        public IEnumerable<ITagSpan<ModuleTermTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
            {
                yield break;
            }

            var src = spans[0].Snapshot.GetText();
            var units = _compiler.CompileTestOnFly(src, FilePath()).OnlyValidTests();

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

                bool HasModule(string moduleName) => containingTest
                    .RegisteredModules
                    .Any(regModule => regModule.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

                var text       = span.GetText();
                //var match      = ModuleTermRegex.Match(text);
                //var moduleName = match.Groups["moduleName"].Value;

                var foundModules = ModuleTermRegex
                    .Matches(text)
                    .Cast<Match>()
                    .Select(m => (success: m.Success, moduleName: m.Groups["moduleName"].Value, idx: m.Index, length: m.Length))
                    .Where(m => m.success && 
                                !string.IsNullOrWhiteSpace(m.moduleName) &&
                                HasModule(m.moduleName))
                    .Select(m =>
                    {
                        var modSpan = new SnapshotSpan(span.Snapshot, new Span(span.Start.Position + m.idx, m.length));
                        var termTag = new ModuleTermTag(m.moduleName, containingTest.Name);

                        return new TagSpan<ModuleTermTag>(modSpan, termTag);
                    });

                foreach (var foundModule in foundModules)
                {
                    yield return foundModule;
                }

                //var foundModule = containingTest
                //    .RegisteredModules
                //    .Any(regModule => regModule.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

                //if (!match.Success || 
                //    string.IsNullOrWhiteSpace(moduleName) ||
                //    !foundModule)
                //{
                //    continue;
                //}

                //var modSpan = new SnapshotSpan(span.Snapshot, new Span(span.Start.Position + match.Index, match.Length));
                ////var termTag = new ModuleTermTag(moduleName, containingTest.Name, 0, 10, 0, 0, 0, PositionAffinity.Successor, modSpan, this);
                ////var termTag = ModuleHub
                ////    .Instance
                ////    .ModulePool
                ////    .TryRequest(moduleName)
                ////    .ContinueWith(
                ////        _  => SpacedTag(containingTest.Name, moduleName, modSpan),
                ////        () => NormalTag(containingTest.Name, moduleName, modSpan));

                //var termTag = new ModuleTermTag(moduleName, containingTest.Name);

                //yield return new TagSpan<ModuleTermTag>(modSpan, termTag);
            }
        }

        //private ModuleTermTag SpacedTag(string testName, string moduleName, SnapshotSpan modSpan)
        //    => new ModuleTermTag(moduleName, testName, 0, 10, 0, 0, 0, PositionAffinity.Successor, modSpan, this);

        //private ModuleTermTag NormalTag(string testName, string moduleName, SnapshotSpan modSpan)
        //    => new ModuleTermTag(moduleName, testName, 0, 0, 0, 0, 0, PositionAffinity.Successor, modSpan, this);
    }
}
