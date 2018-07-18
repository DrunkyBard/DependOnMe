using Compilation;
using CompilationUnit;
using DependOnMe.VsExtension.ContentTypeDefinition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FlattenError = Errors.FlattenError;

namespace DependOnMe.VsExtension.Errors
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType(ContentType.Test)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [TextViewRole(PredefinedTextViewRoles.Analyzable)]
    internal sealed class TestErrorTaggerProvider : IViewTaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
            => textView
                .Properties
                .GetOrCreateSingletonProperty(() => new ErrorTagger<TestCompilationUnit>(textView, buffer, Compile, CheckSemantic)) as ITagger<T>;

        private static (TestCompilationUnit unit, IReadOnlyCollection<FlattenError> errors) Compile(string src, string fileName)
        {
            var unit = Compiler.Instance.CompileTestOnFly(src, fileName);

            return (unit, unit.Errors.Select(x => x.Flatten()).AsReadOnly());
        }

        private static IReadOnlyCollection<FlattenError> CheckSemantic(TestCompilationUnit unit)
            => Semantic.checkTestSemantic(unit).AsReadOnly();
    }

    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType(ContentType.Module)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [TextViewRole(PredefinedTextViewRoles.Analyzable)]
    internal sealed class ModuleErrorTaggerProvider : IViewTaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
            => textView
                .Properties
                .GetOrCreateSingletonProperty(() => new ErrorTagger<ModuleCompilationUnit>(textView, buffer, Compile, CheckSemantic)) as ITagger<T>;

        private static (ModuleCompilationUnit unit, IReadOnlyCollection<FlattenError> errors) Compile(string src, string fileName)
        {
            var unit = Compiler.Instance.CompileModuleOnFly(src, fileName);

            return (unit, unit.Errors.Select(x => x.Flatten()).AsReadOnly());
        }

        private static IReadOnlyCollection<FlattenError> CheckSemantic(ModuleCompilationUnit unit)
            => Semantic.checkModuleSemantic(unit).AsReadOnly();
    }
}
