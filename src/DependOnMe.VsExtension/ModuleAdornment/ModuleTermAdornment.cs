using DependOnMe.VsExtension.ModuleAdornment.UI;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;

namespace DependOnMe.VsExtension.ModuleAdornment
{
    internal sealed class ModuleTermAdornment
    {
        private static readonly Regex ModuleTermRegex =
            new Regex(@"MODULE (?<moduleName>\w+(?:[\w|\d]*\.\w[\w|\d]*)*)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

		private readonly IAdornmentLayer _layer;
		private readonly IAdornmentLayer _btnLayer;


        private readonly IWpfTextView _view;
        private readonly ITagAggregator<ModuleTermTag> _tagger;

        private readonly Brush _brush;

        private readonly Pen _pen;

        public ModuleTermAdornment(IWpfTextView view, ITagAggregator<ModuleTermTag> tagger)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            _layer = view.GetAdornmentLayer("DrtModuleTermAdornment");
            _btnLayer = view.GetAdornmentLayer("DrtModuleBtnTermAdornment");

            _view = view;
            _tagger = tagger;
            _view.LayoutChanged += OnLayoutChanged;

            _brush = new SolidColorBrush(Color.FromArgb(0x20, 0x00, 0x00, 0xff));
            _brush.Freeze();

            var penBrush = new SolidColorBrush(Color.FromRgb(76, 76, 79));
            penBrush.Freeze();
            _pen = new Pen(penBrush, 2);
            _pen.Freeze();
        }

        internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            foreach (ITextViewLine line in e.NewOrReformattedLines)
            {
                CreateVisuals(line);
            }
        }

        private (bool success, int position, int length, string modName) MatchFileNamePattern(string line)
        {
            var match = ModuleTermRegex.Match(line);

            if (!match.Success)
            {
                return (false, 0, 0, null);
            }

            var moduleName = match.Groups["moduleName"].Value;

            if (string.IsNullOrWhiteSpace(moduleName))
            {
                return (false, 0, 0, null);
            }

            return (true, match.Index, match.Length, moduleName);
        }

        private void CreateVisuals(ITextViewLine line)
        {
            IWpfTextViewLineCollection textViewLines = _view.TextViewLines;

            var match = MatchFileNamePattern(line.Extent.GetText());

            if (!match.success)
            {
                return;
            }


            //SnapshotSpan span     = new SnapshotSpan(_view.TextSnapshot, Span.FromBounds(match.position, match.position + match.length));
            var span = new SnapshotSpan(line.Snapshot, new Span(line.Start.Position + match.position, match.length));

            var tag = _tagger.GetTags(span).First();
            var tagSpan = tag.Span.GetSpans(span.Snapshot)[0];
            var geometry = textViewLines.GetMarkerGeometry(tagSpan);

            if (geometry != null)
            {
                //var drawing = new GeometryDrawing(_brush, _pen, geometry);
                //drawing.Freeze();

                //var drawingImage = new DrawingImage(drawing);
                //drawingImage.Freeze();

                //var image = new Image { Source = drawingImage };
                var depView = new Dependencies();
                var btn = new ModuleButton(10,10, depView);
                //var btn = new ModuleButton
                //{
                //    Height = line.Height,
                //    Width  = line.Height
                //};



                //Canvas.SetLeft(btn, geometry.Bounds.Right + 0.1);
                //Canvas.SetTop (btn, geometry.Bounds.Top);

                // Align the image with the top of the bounds of the text geometry
                //Canvas.SetLeft(image, geometry.Bounds.Left);
                //Canvas.SetTop(image, geometry.Bounds.Top);

                Canvas.SetLeft(btn, geometry.Bounds.Left);
                Canvas.SetTop(btn, geometry.Bounds.Top-10);

                Canvas.SetLeft(depView, geometry.Bounds.Right);
                Canvas.SetTop(depView, geometry.Bounds.Bottom);

                //_layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, tagSpan, null, image, null);
                _btnLayer.AddAdornment(AdornmentPositioningBehavior.TextRelative, tagSpan, null, btn, null);
                _btnLayer.AddAdornment(AdornmentPositioningBehavior.OwnerControlled, tagSpan, null, depView, null);
            }
        }

    }
}
