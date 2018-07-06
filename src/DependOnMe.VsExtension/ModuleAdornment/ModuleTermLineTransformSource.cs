using DependOnMe.VsExtension.Messaging;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;
using System.Linq;

namespace DependOnMe.VsExtension.ModuleAdornment
{
    internal sealed class ModuleTermLineTransformSource : ILineTransformSource
    {
        private readonly ITagAggregator<ModuleTermTag> _tagger;

        public ModuleTermLineTransformSource(ITagAggregator<ModuleTermTag> tagger)
        {
            _tagger = tagger;
        }

        public LineTransform GetLineTransform(ITextViewLine line, double yPosition, ViewRelativePosition placement)
        {
            var hasModules = _tagger
                .GetTags(line.ExtentAsMappingSpan)
                .Any(tag => ModuleHub
                    .Instance
                    .ModulePool.Contains(tag.Tag.ModuleName));

            if (hasModules)
            {
                return new LineTransform(10, 0, 1);
            }

            return new LineTransform(0, 0, 1);
        }
    }
}
