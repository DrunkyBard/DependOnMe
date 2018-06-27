using DependOnMe.VsExtension.ModuleAdornment.UI;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace DependOnMe.VsExtension.ModuleAdornment
{
    internal sealed class ModuleTermAdornment
    {
        private static readonly Regex ModuleTermRegex =
            new Regex(@"MODULE (?<moduleName>\w+(?:[\w|\d]*\.\w[\w|\d]*)*)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly IReadOnlyCollection<ModuleMatch> EmptyMatch = new List<ModuleMatch>().AsReadOnly();

		private readonly IAdornmentLayer _btnLayer;


        private readonly IWpfTextView _view;
        private readonly ITagAggregator<ModuleTermTag> _tagger;

        private readonly Dictionary<int, List<(string moduleName, ModuleTree dep, ModuleButton modBtn)>> _lineAdornments
            = new Dictionary<int, List<(string, ModuleTree, ModuleButton)>>();

        public ModuleTermAdornment(IWpfTextView view, ITagAggregator<ModuleTermTag> tagger)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            _btnLayer = view.GetAdornmentLayer("DrtModuleBtnTermAdornment");

            _view = view;
            _tagger = tagger;
            _view.LayoutChanged += OnLayoutChanged;
        }

        internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            foreach (ITextViewLine line in e.NewOrReformattedLines)
            {
                CreateVisuals(line);
            }
        }

        private struct ModuleMatch
        {
            public readonly int Position;
            public readonly int Length;
            public readonly string ModuleName;

            public ModuleMatch(int position, int length, string moduleName)
            {
                Position   = position;
                Length     = length;
                ModuleName = moduleName;
            }
        }

        private IReadOnlyCollection<ModuleMatch> MatchFileNamePattern(string line)
        {
            var regExMatches = ModuleTermRegex.Matches(line);

            if (regExMatches.Count == 0)
            {
                return EmptyMatch;
            }

            var matches = new List<ModuleMatch>();

            foreach (Match regExMatch in regExMatches)
            {
                var moduleName = regExMatch.Groups["moduleName"].Value;

                if (string.IsNullOrWhiteSpace(moduleName))
                {
                    continue;
                }

                matches.Add(new ModuleMatch(regExMatch.Index, regExMatch.Length, moduleName));
            }

            return matches.AsReadOnly();
        }

        private void CreateVisuals(ITextViewLine line)
        {
            var matches    = MatchFileNamePattern(line.Extent.GetText());
            var lineNumber = line.Snapshot.GetLineNumberFromPosition(line.Start);

            if (matches.Count == 0)
            {
                _lineAdornments.Remove(lineNumber);

                return;
            }

            if (_lineAdornments.TryGetValue(lineNumber, out var adornmentDefs))
            {
                adornmentDefs
                    .RemoveAll(x => !matches.Any(y => x.moduleName.Equals(y.ModuleName, StringComparison.OrdinalIgnoreCase)));
                var existingAdornments = adornmentDefs
                    .Join(
                        matches,
                        x => x.moduleName,
                        x => x.ModuleName,
                        (tuple, match) => (tuple, match),
                        StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                var newModules = matches
                    .Where(x => !adornmentDefs.Any(y => x.ModuleName.Equals(y.moduleName, StringComparison.OrdinalIgnoreCase)))
                    .ToArray();

                foreach (var (tuple, match) in existingAdornments)
                {
                    _btnLayer.RemoveAdornment(tuple.dep);
                    _btnLayer.RemoveAdornment(tuple.modBtn);
                    Render(tuple.dep, tuple.modBtn, line, match);
                }

                foreach (var moduleMatch in newModules)
                {
                    var depView = new ModuleTree();
                    var btnView = new ModuleButton(10, 10, depView);
                    Render(depView, btnView, line, moduleMatch);
                    adornmentDefs.Add((moduleMatch.ModuleName, depView, btnView));
                }
            }
            else
            {
                var newAdornments = new List<(string moduleName, ModuleTree dep, ModuleButton modBtn)>();

                foreach (var moduleMatch in matches)
                {
                    var depView = new ModuleTree();
                    var btnView = new ModuleButton(10, 10, depView);
                    Render(depView, btnView, line, moduleMatch);
                    newAdornments.Add((moduleMatch.ModuleName, depView, btnView));
                }

                _lineAdornments.Add(lineNumber, newAdornments);
            }
        }

        private void Render(ModuleTree depView, ModuleButton btnView, ITextViewLine line, ModuleMatch moduleMatch)
        {
            var textViewLines = _view.TextViewLines;
            var span = new SnapshotSpan(line.Snapshot, new Span(line.Start.Position + moduleMatch.Position, moduleMatch.Length));

            var tag = _tagger.GetTags(span).First();
            var tagSpan = tag.Span.GetSpans(span.Snapshot)[0];

            var geometry = textViewLines.GetMarkerGeometry(tagSpan);

            Canvas.SetLeft(btnView, geometry.Bounds.Left);
            Canvas.SetTop(btnView, geometry.Bounds.Top - 10);

            Canvas.SetLeft(depView, geometry.Bounds.Right);
            Canvas.SetTop(depView, geometry.Bounds.Bottom);

            _btnLayer.AddAdornment(AdornmentPositioningBehavior.TextRelative, tagSpan, null, btnView, null);
            _btnLayer.AddAdornment(AdornmentPositioningBehavior.OwnerControlled, tagSpan, null, depView, null);
        }
    }
}
