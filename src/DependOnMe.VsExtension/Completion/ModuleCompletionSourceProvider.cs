using DependOnMe.VsExtension.ContentTypeDefinition;
using Microsoft.FSharp.Text.Lexing;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Utilities;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace DependOnMe.VsExtension.Completion
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType(ContentType.Module)]
    [Name("Module token completion")]
    public sealed class ModuleCompletionSourceProvider : BaseCompletionSourceProvider
    {
        protected override IReadOnlyCollection<string> GetSuggestions(string fileName, string src, Position position)
            => ModuleDslCompletion.suggestFrom(fileName, src, position);
    }
}
