using DependOnMe.VsExtension.ContentTypeDefinition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;

namespace DependOnMe.VsExtension.ModuleAdornment
{
    internal sealed class ModuleTermTag : ITag, IEquatable<ModuleTermTag>
    {
        public readonly string ModuleName;
        public readonly string TestName;

        public ModuleTermTag(string moduleName, string testName) 
        {
            ModuleName = moduleName;
            TestName = testName;
        }

        public bool Equals(ModuleTermTag other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(ModuleName, other.ModuleName, StringComparison.OrdinalIgnoreCase) && 
                   string.Equals(TestName, other.TestName, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is ModuleTermTag tag && Equals(tag);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ModuleName != null ? ModuleName.ToUpperInvariant().GetHashCode() : 0) * 397) ^ 
                       (TestName != null ? TestName.ToUpperInvariant().GetHashCode() : 0);
            }
        }
    }

    [Export(typeof(ITaggerProvider))]
    [ContentType(ContentType.Test)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [TagType(typeof(ModuleTermTag))]
    internal sealed class ModuleTermTaggerProvider : ITaggerProvider
    {
        [Import]
        internal ITextDocumentFactoryService TextDocumentFactoryService;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
            => new ModuleTermTagger(TextDocumentFactoryService, buffer) as ITagger<T>;
    }
}
