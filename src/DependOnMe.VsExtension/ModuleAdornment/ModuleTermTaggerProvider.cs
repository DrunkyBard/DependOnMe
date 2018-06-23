using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace DependOnMe.VsExtension.ModuleAdornment
{
    internal sealed class ModuleTermTag : SpaceNegotiatingAdornmentTag
    {
        public ModuleTermTag(
            double width, 
            double topSpace, 
            double baseline, 
            double textHeight, 
            double bottomSpace, 
            PositionAffinity affinity, 
            object identityTag, 
            object providerTag) : base(width, topSpace, baseline, textHeight, bottomSpace, affinity, identityTag, providerTag)
        { }
    }

    [Export(typeof(ITaggerProvider))]
    [ContentType("drt")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [TagType(typeof(SpaceNegotiatingAdornmentTag))]
    [TagType(typeof(ModuleTermTag))]
    internal sealed class ModuleTermTaggerProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
            => new ModuleTermTagger() as ITagger<T>;
    }
}
