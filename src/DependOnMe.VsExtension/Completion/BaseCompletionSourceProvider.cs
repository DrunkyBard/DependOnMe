using Microsoft.FSharp.Text.Lexing;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace DependOnMe.VsExtension.Completion
{
    public abstract class BaseCompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        public ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        public SVsServiceProvider ServiceProvider;

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer) => new CompletionSource(textBuffer, ServiceProvider, GetSuggestions);

        protected abstract IReadOnlyCollection<string> GetSuggestions(string fileName, string src, Position position);
    }
}
