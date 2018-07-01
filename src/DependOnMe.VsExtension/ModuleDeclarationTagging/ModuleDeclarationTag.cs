using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace DependOnMe.VsExtension.ModuleDeclarationTagging
{
    internal sealed class ModuleDeclarationTag : SpaceNegotiatingAdornmentTag
    {
        public readonly string ModuleName;

        public ModuleDeclarationTag(string moduleName, SnapshotSpan span) : base(0, 0, 0, 0, 0, PositionAffinity.Successor, span, null) 
            => ModuleName = moduleName;
    }
}