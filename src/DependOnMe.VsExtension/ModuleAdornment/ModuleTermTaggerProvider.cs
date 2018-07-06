using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace DependOnMe.VsExtension.ModuleAdornment
{
    internal sealed class ModuleTermTag : ITag
    {
        public readonly string ModuleName;
        public readonly string TestName;

        public ModuleTermTag(string moduleName, string testName) 
        {
            ModuleName = moduleName;
            TestName = testName;
        }
    }

    [Export(typeof(ITaggerProvider))]
    [ContentType("drt")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [TagType(typeof(ModuleTermTag))]
    internal sealed class ModuleTermTaggerProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
            => new ModuleTermTagger() as ITagger<T>;

        //public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        //    => textView.Properties.GetOrCreateSingletonProperty(() =>
        //        new ModuleTermTagger((IWpfTextView)textView, buffer) as ITagger<T>);
    }
}
